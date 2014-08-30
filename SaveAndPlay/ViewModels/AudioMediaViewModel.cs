#region Apache License, Version 2.0 
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
#endregion

using Id3;
using SaveAndPlay.DAL.Entities;
using SaveAndPlay.Helpers;
using SaveAndPlay.Resources;
using SaveAndPlay.Scheduler;
using SaveAndPlay.ViewModels.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;

namespace SaveAndPlay.ViewModels
{
    public class AudioMediaViewModel : BaseViewModel, IDataServiceSchedulerClient
    {
        public class PlaylistWrapper
        {
            public bool IsChecked { get; set; }
            public Playlist Playlist { get; set; }
        }

        private List<PlaylistWrapper> oldPlaylists;
        private bool alreadyLoaded = false;

        private AudioMedia audioMedia;
        public AudioMedia AudioMedia
        {
            get { return this.audioMedia; }
            set { this.audioMedia = value; RaisePropertyChanged("AudioMedia"); }
        }

        private List<Album> albums;
        public List<Album> Albums
        {
            get { return this.albums; }
            set { this.albums = value; RaisePropertyChanged("Albums"); }
        }

        private List<Artist> artists;
        public List<Artist> Artists
        {
            get { return this.artists; }
            set { this.artists = value; RaisePropertyChanged("Artists"); }
        }

        private string songTitle;
        public string SongTitle
        {
            get { return songTitle; }
            set { songTitle = value; RaisePropertyChanged("SongTitle"); }
        }

        private string currentAlbum;
        public string CurrentAlbum
        {
            get { return currentAlbum; }
            set { currentAlbum = value; RaisePropertyChanged("CurrentAlbum"); }
        }

        private string currentArtist;
        public string CurrentArtist
        {
            get { return currentArtist; }
            set { currentArtist = value; RaisePropertyChanged("CurrentArtist"); }
        }

        private bool isProtected;
        public bool IsProtected
        {
            get { return isProtected; }
            set { isProtected = value; RaisePropertyChanged("IsProtected"); }
        }

        private List<PlaylistWrapper> playlists;
        public List<PlaylistWrapper> Playlists
        {
            get { return playlists; }
            set { playlists = value; RaisePropertyChanged("Playlists"); }
        }

        private bool noPlaylist;
        public bool NoPlaylist
        {
            get { return this.noPlaylist; }
            set { this.noPlaylist = value; RaisePropertyChanged("NoPlaylist"); }
        }

        private Album listPickedAlbum;
        public Album ListPickedAlbum
        {
            get { return this.listPickedAlbum; }
            set
            {
                this.listPickedAlbum = value;
                if (value != null)
                {
                    this.CurrentAlbum = value.Id == -2 ? string.Empty : value.Title;
                    ListPickedAlbum = null;
                }
            }
        }

        private Artist listPickedArtist;
        public Artist ListPickedArtist
        {
            get { return this.listPickedArtist; }
            set
            {
                listPickedArtist = value;
                if (value != null)
                {
                    this.CurrentArtist = value.Id == -2 ? string.Empty : value.Name;
                    this.LoadAlbums(this.CurrentArtist);
                    ListPickedArtist = null;
                }
            }
        }

        public AudioMediaViewModel()
        {
        }

        public void Load()
        {
            if (App.DataCacheServiceInstance != null)
            {
                var parameter = this.NavigationViewService.GetParametersFromUri();
                var id = Int32.Parse(parameter.First().Value);

                this.AudioMedia = App.DataCacheServiceInstance.Songs.FirstOrDefault(x => x.Id == id);
                this.SongTitle = this.AudioMedia.Title;
                if (!alreadyLoaded)
                {
                    this.CurrentArtist = this.AudioMedia.Album.Artist.Name;
                    this.LoadAlbums(this.CurrentArtist, false);
                    this.CurrentAlbum = this.AudioMedia.Album.Title;
                }
                this.IsProtected = this.AudioMedia.IsProtected;
                var artists = App.DataCacheServiceInstance.ArtistsVisible.ToList();
                artists.Insert(0, new Artist() { Id = -2, Name = Resources.MusicList.UndefinedLabel });
                artists.Insert(0, new Artist() { Id = -1, Name = Resources.MusicList.CancelLabel });
                this.Artists = artists;
                this.Playlists = App.DataCacheServiceInstance.Playlists.Select(x => new PlaylistWrapper()
                {
                    IsChecked = x.Songs.Any(y => y.Id == this.AudioMedia.Id),
                    Playlist = x
                }).ToList();
                this.oldPlaylists = App.DataCacheServiceInstance.Playlists.Select(x => new PlaylistWrapper()
                {
                    IsChecked = x.Songs.Any(y => y.Id == this.AudioMedia.Id),
                    Playlist = x
                }).ToList(); ;
                alreadyLoaded = true;
            }
        }

        public void Unload()
        {
            this.SongTitle = null;
            this.IsProtected = false;
            this.Playlists = null;
        }

        public void Save()
        {
            if (!string.IsNullOrEmpty(this.SongTitle.Trim()))
            {
                var playlistToAdd = new List<int>();
                var playlistToRemove = new List<int>();
                foreach (var oldPlaylist in oldPlaylists)
                {
                    var currentPlaylist = this.Playlists.First(x => x.Playlist.Id == oldPlaylist.Playlist.Id);
                    if (oldPlaylist.IsChecked && !currentPlaylist.IsChecked)
                    {
                        playlistToRemove.Add(currentPlaylist.Playlist.Id);
                    }
                    else if (!oldPlaylist.IsChecked && currentPlaylist.IsChecked)
                    {
                        playlistToAdd.Add(currentPlaylist.Playlist.Id);
                    }
                }
                App.DataCacheServiceInstance.UpdateAudioMedia(this.AudioMedia,
                                                              this.SongTitle,
                                                              this.CurrentAlbum,
                                                              this.CurrentArtist,
                                                              this.IsProtected,
                                                              playlistToAdd,
                                                              playlistToRemove,
                                                              TaskParam.AsyncContract("UpdateAudioMedia"));
                this.NavigationViewService.GoBack();
            }
            else
            {
                this.DialogViewService.MessageBoxOk(Main.Error, MusicList.TitleMustBeFilled);
            }
        }

        public void Delete()
        {
            if (this.AudioMedia != null)
            {
                if (this.DialogViewService.MessageBoxOKCancel(Resources.Main.Warning, string.Format(Resources.MusicList.ConfirmDelete, this.AudioMedia.Title)))
                {
                    try
                    {
                        IsolatedStorageFileHelper.DeleteFileIfExists(Path.Combine(this.MediaSupportService.GetStorageDirectory(), this.AudioMedia.FilesystemId));
                        App.DataCacheServiceInstance.DeleteAudioMedia(this.AudioMedia, TaskParam.AsyncContract("DeleteAudioMedia"));
                        this.NavigationViewService.GoBack();
                    }
                    catch (IsolatedStorageException)
                    {
                        this.DialogViewService.MessageBoxOk(Resources.Main.Error, string.Format(Resources.MusicList.PlayingError, this.AudioMedia.Title));
                        return;
                    }
                }
            }
        }

        public void AlbumChanged(string albumStr)
        {
            var album = App.DataCacheServiceInstance.Albums.FirstOrDefault(x => x.Title == albumStr);
            if (album != null)
            {
                this.ListPickedAlbum = album;
            }
        }

        public void ArtistChanged(string artistStr)
        {
            this.LoadAlbums(artistStr, false);
        }

        public void LoadAlbums(string artistStr, bool resetCurrentAlbum = true)
        {
            var artist = App.DataCacheServiceInstance.ArtistsVisible.FirstOrDefault(x => x.Name.Trim().ToUpper() == artistStr.Trim().ToUpper());
            if (artist != null)
            {
                var albums = artist.AlbumsVisible;
                albums.Insert(0, new Album() { Id = -2, Title = Resources.MusicList.UndefinedLabel });
                albums.Insert(0, new Album() { Id = -1, Title = Resources.MusicList.CancelLabel });
                this.Albums = albums;
                if (resetCurrentAlbum)
                {
                    this.ListPickedAlbum = null;
                    this.CurrentAlbum = string.Empty;
                }
            }
            else
            {
                this.Albums = new List<Album>();
            }
        }

        public void SendToVideoLibrary()
        {
            if (this.DialogViewService.MessageBoxOKCancel(Resources.Main.Warning, string.Format(Resources.MusicList.ConfirmSendToVideo, this.AudioMedia.Title)))
            {
                App.DataCacheServiceInstance.SendToVideoLibrary(this.AudioMedia, TaskParam.AsyncContract("SendToVideoLibrary"));
                this.NavigationViewService.GoBack();
            }
        }

        public void Tag()
        {
            if (this.AudioMedia != null)
            {
                try
                {
                    using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        var streamFile = store.OpenFile(Path.Combine(this.MediaSupportService.GetStorageDirectory(), this.AudioMedia.FilesystemId), FileMode.Open);
                        using (var streamMP3 = new Mp3Stream(streamFile))
                        {
                            Id3Tag tag = streamMP3.GetTag(Id3TagFamily.FileStartTag);
                            this.CurrentAlbum = tag.Album ?? string.Empty;
                            this.CurrentArtist = tag.Artists != null ? tag.Artists.Value ?? string.Empty : string.Empty;
                            this.SongTitle = tag.Title;
                        }
                    }
                }
                catch (Exception e)
                {
                    this.DialogViewService.MessageBoxOk(Main.Error, MusicList.TagError);
                }
            }
        }

        public void Reset()
        {
            alreadyLoaded = false;
        }

        public void TaskRunning(string current, List<string> toCome)
        {
            if (!this.DataLoadingScopeViewService.WaitingForAll(current, toCome))
            {
                this.Ready();
            }
        }

        public void TaskCompleted(DataAsyncResult result, string current, List<string> toCome)
        {
            if (!this.DataLoadingScopeViewService.WaitingForAll(null, toCome))
            {
                this.Ready();
            }
        }

        private void Ready()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                this.Load();
                this.NoPlaylist = this.Playlists.Count <= 0;
                base.MessengerInstance.Send(ScreenLock.Release());
            });
        }
    }
}

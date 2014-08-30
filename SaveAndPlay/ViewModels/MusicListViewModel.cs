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

using GalaSoft.MvvmLight.Command;
using SaveAndPlay.DAL.Entities;
using SaveAndPlay.Helpers;
using SaveAndPlay.Scheduler;
using SaveAndPlay.ViewModels.Messages;
using SaveAndPlay.ViewService;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace SaveAndPlay.ViewModels
{
    public class MusicListViewModel : BaseViewModel, IDataServiceSchedulerClient
    {
        //public RelayCommand<AudioMedia> ReadAudioMediaCommand { get; private set; }
        //public RelayCommand<Album> ReadAlbumCommand { get; private set; }
        //public RelayCommand<Artist> ReadArtistCommand { get; private set; }
        //public RelayCommand<Playlist> ReadPlaylistCommand { get; private set; }

        private List<AudioMedia> songs;
        public List<AudioMedia> Songs
        {
            get { return this.songs; }
            set { this.songs = value; RaisePropertyChanged("Songs"); }
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

        private List<Playlist> playlists;
        public List<Playlist> Playlists
        {
            get { return this.playlists; }
            set { this.playlists = value; RaisePropertyChanged("Playlists"); }
        }

        private bool noSong;
        public bool NoSong
        {
            get { return this.noSong; }
            set { this.noSong = value; RaisePropertyChanged("NoSong"); }
        }

        private bool noAlbum;
        public bool NoAlbum
        {
            get { return this.noAlbum; }
            set { this.noAlbum = value; RaisePropertyChanged("NoAlbum"); }
        }

        private bool noArtist;
        public bool NoArtist
        {
            get { return this.noArtist; }
            set { this.noArtist = value; RaisePropertyChanged("NoArtist"); }
        }

        private bool noPlaylist;
        public bool NoPlaylist
        {
            get { return this.noPlaylist; }
            set { this.noPlaylist = value; RaisePropertyChanged("NoPlaylist"); }
        }

        private AudioMedia selectedSong;
        public AudioMedia SelectedSong
        {
            get { return this.selectedSong; }
            set
            {
                if (value != null)
                {
                    this.selectedSong = value;
                    this.NavigationViewService.NavigateTo("/UI/Pages/AudioMediaPage.xaml", new NavigationParameter("songId", selectedSong.Id.ToString()));
                }
            }
        }

        private Album selectedAlbum;
        public Album SelectedAlbum
        {
            get { return this.selectedAlbum; }
            set
            {
                if (value != null)
                {
                    this.selectedAlbum = value;
                    this.NavigationViewService.NavigateTo("/UI/Pages/AlbumPage.xaml", new NavigationParameter("albumId", selectedAlbum.Id.ToString()));
                }
            }
        }

        private Artist selectedArtist;
        public Artist SelectedArtist
        {
            get { return this.selectedArtist; }
            set
            {
                if (value != null)
                {
                    this.selectedArtist = value;
                    this.NavigationViewService.NavigateTo("/UI/Pages/ArtistPage.xaml", new NavigationParameter("artistId", selectedArtist.Id.ToString()));
                }
            }
        }

        private Playlist selectedPlaylist;
        public Playlist SelectedPlaylist
        {
            get { return this.selectedPlaylist; }
            set
            {
                if (value != null)
                {
                    this.selectedPlaylist = value;
                    this.NavigationViewService.NavigateTo("/UI/Pages/PlaylistPage.xaml", new NavigationParameter("playlistId", selectedPlaylist.Id.ToString()));
                }
            }
        }

        public MusicListViewModel()
        {
            //this.ReadAudioMediaCommand = new RelayCommand<AudioMedia>((o) =>
            //{
            //    if (o != null)
            //    {
            //        this.PlaybackAgentService.Play(o);
            //    }
            //});
            //this.ReadAlbumCommand = new RelayCommand<Album>((o) =>
            //{
            //    if (o != null)
            //    {
            //        this.PlaybackAgentService.Play(o);
            //    }
            //});
            //this.ReadArtistCommand = new RelayCommand<Artist>((o) =>
            //{
            //    if (o != null)
            //    {
            //        this.PlaybackAgentService.Play(o);
            //    }
            //});
            //this.ReadPlaylistCommand = new RelayCommand<Playlist>((o) =>
            //{
            //    if (o != null)
            //    {
            //        this.PlaybackAgentService.Play(o);
            //    }
            //});
        }

        public void NewPlaylist()
        {
            this.NavigationViewService.NavigateTo("/UI/Pages/PlaylistPage.xaml", new NavigationParameter("playlistId", "-1"));
        }

        public void Unload()
        {
            this.Songs = null;
            this.Albums = null;
            this.Artists = null;
            this.Playlists = null;
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
            UIHelper.DelayUIInvocation(500, () =>
            {
                this.Songs = App.DataCacheServiceInstance.SongsVisible.ToList();
                this.Albums = App.DataCacheServiceInstance.AlbumsVisible.ToList();
                this.Artists = App.DataCacheServiceInstance.ArtistsVisible.ToList();
                this.Playlists = App.DataCacheServiceInstance.Playlists.ToList();
                this.NoSong = this.Songs.Count <= 0;
                this.NoAlbum = this.Albums.Count <= 0;
                this.NoArtist = this.Artists.Count <= 0;
                this.NoPlaylist = this.Playlists.Count <= 0;
                base.MessengerInstance.Send(ScreenLock.Release());
            });
        }
    }
}

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

using SaveAndPlay.DAL.Entities;
using SaveAndPlay.Scheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading;

namespace SaveAndPlay.DAL.Services
{
    public class DataCacheService : BaseDataServiceScheduler
    {
        public List<VideoMedia> Videos { get; private set; }
        public IEnumerable<VideoMedia> VideosVisible { get { return this.Videos.Where(x => x.Visible); } }

        public List<AudioMedia> Songs { get; private set; }
        public IEnumerable<AudioMedia> SongsVisible { get { return this.Songs.Where(x => x.Visible); } }

        public List<Album> Albums { get; private set; }
        public IEnumerable<Album> AlbumsVisible { get { return this.Albums.Where(x => x.Visible); } }

        public List<Artist> Artists { get; private set; }
        public IEnumerable<Artist> ArtistsVisible { get { return this.Artists.Where(x => x.Visible); } }

        public List<Playlist> Playlists { get; private set; }
        public IEnumerable<Playlist> PlaylistsVisible { get { return this.Playlists.Where(x => x.Visible); } }

        public List<PlaylistAudioMedia> PlaylistsAudioMedia { get; private set; }

        public DataCacheService()
        {
            this.Initialize(TaskParam.AsyncContract("Initialize"));
        }

        private DataAsyncResult Initialize(TaskParam taskParam)
        {
            return this.EnqueueRequest(taskParam, () =>
            {
                var entities = base.GlobalDataService.SelectAll();
                this.Videos = entities.Videos;
                this.Songs = entities.Songs;
                this.Albums = entities.Albums;
                this.Artists = entities.Artists;
                this.Playlists = entities.Playlists;
                this.PlaylistsAudioMedia = entities.PlaylistsAudioMedia;
                this.Bind(this.Artists, this.Albums, this.Songs, this.Playlists, this.PlaylistsAudioMedia);
                this.CheckFilesystemIds(this.Songs, this.Videos);
                this.DisableProtectModeInternal();
                return null;
            });
        }

        public DataAsyncResult AddAudioMedia(string title, string album, string artist, string filesystemId, bool isProtected, TaskParam taskParam)
        {
            return this.EnqueueRequest(taskParam, () =>
            {
                this.AddAudioMediaInternal(title, album, artist, filesystemId, isProtected);
                return null;
            });
        }

        private void AddAudioMediaInternal(string title, string album, string artist, string filesystemId, bool isProtected)
        {
            // find or create artist entity
            var artistEntity = this.Artists.FirstOrDefault(x => x.Name.Trim().ToUpper() == artist.Trim().ToUpper());
            if (artistEntity == null)
            {
                artistEntity = new Artist() { Name = artist };
                base.ArtistDataService.Insert(artistEntity);
                this.Artists.Add(artistEntity);
            }

            // find or create album entity
            var albumEntity = artistEntity.Albums.FirstOrDefault(x => x.Title.Trim().ToUpper() == album.Trim().ToUpper());
            if (albumEntity == null)
            {
                albumEntity = new Album() { ArtistId = artistEntity.Id, Title = album };
                base.AlbumDataService.Insert(albumEntity);
                artistEntity.Albums.Add(albumEntity);
                this.Albums.Add(albumEntity);
                albumEntity.Artist = artistEntity;
            }

            // create audiomedia entity
            var audioMediaEntity = new AudioMedia()
            {
                AlbumId = albumEntity.Id,
                FilesystemId = filesystemId,
                Title = title,
                IsProtected = isProtected,
            };
            base.AudioMediaDataService.Insert(audioMediaEntity);
            this.Songs.Add(audioMediaEntity);
            albumEntity.Songs.Add(audioMediaEntity);
            audioMediaEntity.Album = albumEntity;
            albumEntity.Artist = artistEntity;
        }

        public DataAsyncResult UpdateAudioMedia(AudioMedia entity, string title, string album, string artist, bool isProtected, List<int> playlistsToAdd, List<int> playlistsToRemove, TaskParam taskParam)
        {
            return this.EnqueueRequest(taskParam, () =>
            {
                // find or create artist entity
                var artistEntity = this.Artists.FirstOrDefault(x => x.Name.Trim().ToUpper() == artist.Trim().ToUpper());
                if (artistEntity == null)
                {
                    artistEntity = new Artist() { Name = artist };
                    base.ArtistDataService.Insert(artistEntity);
                    this.Artists.Add(artistEntity);
                }

                // find or create album entity
                var albumEntity = artistEntity.Albums.FirstOrDefault(x => x.Title.Trim().ToUpper() == album.Trim().ToUpper());
                if (albumEntity == null)
                {
                    albumEntity = new Album() { ArtistId = artistEntity.Id, Title = album };
                    base.AlbumDataService.Insert(albumEntity);
                    artistEntity.Albums.Add(albumEntity);
                    this.Albums.Add(albumEntity);
                    albumEntity.Artist = artistEntity;
                }

                // update audiomedia entity
                int previousAlbumId = entity.Album.Id;
                entity.Title = title;
                entity.IsProtected = isProtected;
                entity.AlbumId = albumEntity.Id;
                entity.Album = albumEntity;
                base.AudioMediaDataService.Update(entity);

                // reflect changes into database and the cache
                if (previousAlbumId != albumEntity.Id)
                {
                    albumEntity.Songs.Add(entity);
                    var previousAlbum = this.Albums.FirstOrDefault(x => x.Id == previousAlbumId);
                    previousAlbum.Songs.Remove(entity);
                    if (previousAlbum.Songs.Count == 0)
                    {
                        var previousArtist = previousAlbum.Artist;
                        base.AlbumDataService.Delete(new Album() { Id = previousAlbum.Id });
                        previousArtist.Albums.Remove(previousAlbum);
                        this.Albums.Remove(previousAlbum);
                        if (previousArtist.Albums.Count == 0)
                        {
                            base.ArtistDataService.Delete(new Artist() { Id = previousArtist.Id });
                            this.Artists.Remove(previousArtist);
                        }
                    }
                }

                foreach (var item in playlistsToAdd)
                {
                    var playlist = this.Playlists.First(x => x.Id == item);
                    this.AddAudioMediaToPlaylist(entity, playlist);
                }

                foreach (var item in playlistsToRemove)
                {
                    var playlist = this.Playlists.First(x => x.Id == item);
                    this.RemoveAudioMediaFromPlaylist(entity, playlist);
                }

                return null;
            });
        }

        public void AddAudioMediaToPlaylist(AudioMedia audioMedia, Playlist playlist)
        {
            base.PlaylistAudioMediaDataService.Insert(new PlaylistAudioMedia() { PlaylistId = playlist.Id, AudioMediaId = audioMedia.Id });
            playlist.Songs.Add(audioMedia);
        }

        public void RemoveAudioMediaFromPlaylist(AudioMedia audioMedia, Playlist playlist)
        {
            base.PlaylistAudioMediaDataService.Delete(new PlaylistAudioMedia() { PlaylistId = playlist.Id, AudioMediaId = audioMedia.Id });
            playlist.Songs.Remove(audioMedia);
        }

        public DataAsyncResult DeleteAudioMedia(AudioMedia entity, TaskParam taskParam)
        {
            return this.EnqueueRequest(taskParam, () =>
            {
                DeleteAudioMediaInternal(entity);
                return null;
            });
        }

        private void DeleteAudioMediaInternal(AudioMedia entity)
        {
            // delete audio media in database
            var album = entity.Album;
            base.AudioMediaDataService.Delete(entity);
            this.Songs.Remove(entity);

            // reflect changes into database and the cache
            album.Songs.Remove(entity);
            if (album.Songs.Count == 0)
            {
                var artist = album.Artist;
                base.AlbumDataService.Delete(new Album() { Id = album.Id });
                artist.Albums.Remove(album);
                this.Albums.Remove(album);
                if (artist.Albums.Count == 0)
                {
                    base.ArtistDataService.Delete(new Artist() { Id = artist.Id });
                    this.Artists.Remove(artist);
                }
            }
            foreach (var playlist in this.Playlists.Where(x => x.Songs.Any(y => y.Id == entity.Id)))
            {
                base.PlaylistAudioMediaDataService.Delete(new PlaylistAudioMedia() { PlaylistId = playlist.Id, AudioMediaId = entity.Id });
                playlist.Songs.Remove(entity);
            }
        }

        public DataAsyncResult SendToVideoLibrary(AudioMedia entity, TaskParam taskParam)
        {
            return this.EnqueueRequest(taskParam, () =>
            {
                var title = entity.Title;
                var fileSystemId = entity.FilesystemId;
                var isProtected = entity.IsProtected;
                this.DeleteAudioMediaInternal(entity);
                this.AddVideoMediaInternal(title, fileSystemId, isProtected);
                return null;
            });
        }

        public DataAsyncResult UpdateAlbum(Album entity, TaskParam taskParam)
        {
            return this.EnqueueRequest(taskParam, () =>
            {
                base.AlbumDataService.Update(entity);
                return null;
            });
        }

        public DataAsyncResult UpdateArtist(Artist entity, TaskParam taskParam)
        {
            return this.EnqueueRequest(taskParam, () =>
            {
                base.ArtistDataService.Update(entity);
                return null;
            });
        }

        public DataAsyncResult SendToAudioLibrary(VideoMedia entity, TaskParam taskParam)
        {
            return this.EnqueueRequest(taskParam, () =>
            {
                var title = entity.Title;
                var fileSystemId = entity.FilesystemId;
                var isProtected = entity.IsProtected;
                this.DeleteVideoMediaInternal(entity);
                this.AddAudioMediaInternal(title, string.Empty, string.Empty, fileSystemId, isProtected);
                return null;
            });
        }

        public DataAsyncResult AddVideoMedia(string title, string filesystemId, bool isProtected, TaskParam taskParam)
        {
            return this.EnqueueRequest(taskParam, () =>
            {
                this.AddVideoMediaInternal(title, filesystemId, isProtected);
                return null;
            });
        }

        private void AddVideoMediaInternal(string title, string filesystemId, bool isProtected)
        {
            var videoEntity = new VideoMedia() { Title = title, FilesystemId = filesystemId, IsProtected = isProtected };
            base.VideoMediaDataService.Insert(videoEntity);
            this.Videos.Add(videoEntity);
        }

        public DataAsyncResult UpdateVideoMedia(VideoMedia entity, string title, bool isProtected, TaskParam taskParam)
        {
            return this.EnqueueRequest(taskParam, () =>
            {
                entity.Title = title;
                entity.IsProtected = isProtected;
                base.VideoMediaDataService.Update(entity);
                return null;
            });
        }


        public DataAsyncResult DeleteVideoMedia(VideoMedia entity, TaskParam taskParam)
        {
            return this.EnqueueRequest(taskParam, () =>
            {
                DeleteVideoMediaInternal(entity);
                return null;
            });
        }

        private void DeleteVideoMediaInternal(VideoMedia entity)
        {
            base.VideoMediaDataService.Delete(entity);
            this.Videos.Remove(entity);
        }

        public DataAsyncResult AddPlaylist(string title, TaskParam taskParam)
        {
            return this.EnqueueRequest(taskParam, () =>
            {
                var entity = new Playlist()
                {
                    Title = title,
                    Visible = true
                };
                base.PlaylistDataService.Insert(entity);
                this.Playlists.Add(entity);
                return null;
            });
        }

        public DataAsyncResult UpdatePlaylist(Playlist entity, TaskParam taskParam)
        {
            return this.EnqueueRequest(taskParam, () =>
            {
                base.PlaylistDataService.Update(entity);
                return null;
            });
        }

        public DataAsyncResult DeletePlaylist(Playlist entity, TaskParam taskParam)
        {
            return this.EnqueueRequest(taskParam, () =>
            {
                base.PlaylistDataService.Delete(entity);
                this.Playlists.Remove(entity);
                return null;
            });
        }

        public DataAsyncResult DisableProtectMode(TaskParam taskParam)
        {
            return this.EnqueueRequest(taskParam, () =>
            {
                this.DisableProtectModeInternal();
                return null;
            });
        }

        private void DisableProtectModeInternal()
        {
            for (int i = 0; i < this.Artists.Count; i++)
            {
                var artist = this.Artists[i];
                artist.Visible = false;
                for (int j = 0; j < artist.Albums.Count; j++)
                {
                    var album = artist.Albums[j];
                    album.Visible = false;
                    for (int k = 0; k < album.Songs.Count; k++)
                    {
                        var song = album.Songs[k];
                        song.Visible = !song.IsProtected;
                        album.Visible = !album.Visible && song.Visible ? true : album.Visible;
                    }
                    artist.Visible = !artist.Visible && album.Visible ? true : artist.Visible;
                }
            }

            for (int i = 0; i < this.Videos.Count; i++)
            {
                var video = this.Videos[i];
                video.Visible = !video.IsProtected;
            }

            for (int i = 0; i < this.Playlists.Count; i++)
            {
                var playlist = this.Playlists[i];
                playlist.Visible = playlist.Songs.Any(x => x.Visible);
            }
        }

        public DataAsyncResult EnableProtectMode(TaskParam taskParam)
        {
            return this.EnqueueRequest(taskParam, () =>
            {
                for (int i = 0; i < this.Artists.Count; i++)
                {
                    var artist = this.Artists[i];
                    artist.Visible = true;
                    for (int j = 0; j < artist.Albums.Count; j++)
                    {
                        var album = artist.Albums[j];
                        album.Visible = true;
                        for (int k = 0; k < album.Songs.Count; k++)
                        {
                            album.Songs[k].Visible = true;
                        }
                    }
                }

                for (int i = 0; i < this.Videos.Count; i++)
                {
                    this.Videos[i].Visible = true;
                }

                for (int i = 0; i < this.Playlists.Count; i++)
                {
                    this.Playlists[i].Visible = true;
                }
                return null;
            });
        }

        public DataAsyncResult DeleteAllProtectedMedia(TaskParam taskParam)
        {
            return this.EnqueueRequest(taskParam, () =>
            {
                var songsToDelete = this.Songs.Where(x => x.IsProtected).ToList();
                foreach (var song in songsToDelete)
                {
                    this.DeleteAudioMediaInternal(song);
                }
                var videosToDelete = this.Videos.Where(x => x.IsProtected).ToList();
                foreach (var video in videosToDelete)
                {
                    this.DeleteVideoMediaInternal(video);
                }
                return null;
            });
        }

        public DataAsyncResult ImportOldMedias(TaskParam taskParam)
        {
            Action<string, string, bool, string> import = (directory, type, protect, destination) =>
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.DirectoryExists(directory))
                    {
                        var files = store.GetFileNames(directory + "\\*");
                        foreach (var file in files)
                        {
                            string title = Path.GetFileNameWithoutExtension(file);
                            string extension = Path.GetExtension(file);
                            string filesystemId = Guid.NewGuid().ToString() + extension;
                            if (type == "audio")
                            {
                                this.AddAudioMediaInternal(title, "", "", filesystemId, protect);
                            }
                            else
                            {
                                this.AddVideoMediaInternal(title, filesystemId, protect);
                            }
                            store.MoveFile(Path.Combine(directory, file), Path.Combine(destination, filesystemId));
                        }
                    }
                }
            };

            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.DirectoryExists("medias"))
                {
                    store.CreateDirectory("medias");
                }
            }

            return this.EnqueueRequest(taskParam, () =>
            {
                Thread.Sleep(10000);
                import("/mediaMusic", "audio", false, "medias");
                import("/mediaMusic/protected", "audio", true, "medias");
                import("/mediaVideo", "video", false, "medias");
                import("/mediaVideo/protected", "video", true, "medias");
                this.DisableProtectModeInternal();
                return null;
            });
        }

        private void Bind(List<Artist> artists, List<Album> albums, List<AudioMedia> songs, List<Playlist> playlists, List<PlaylistAudioMedia> playlistsAudioMedia)
        {
            // build artist cache
            var artistCache = new Dictionary<int, Artist>();
            for (int i = 0; i < artists.Count; i++)
            {
                var artist = artists[i];
                artistCache.Add(artist.Id, artist);
            }

            // build album cache
            var albumCache = new Dictionary<int, Album>();
            for (int i = 0; i < albums.Count; i++)
            {
                var album = albums[i];
                albumCache.Add(album.Id, album);
            }

            // build playlist cache
            var playlistCache = new Dictionary<int, Playlist>();
            for (int i = 0; i < playlists.Count; i++)
            {
                var playlist = playlists[i];
                playlistCache.Add(playlist.Id, playlist);
            }

            var songCache = new Dictionary<int, AudioMedia>();
            for (int i = 0; i < songs.Count; i++)
            {
                var song = songs[i];
                songCache.Add(song.Id, song);
                Album found = null;
                albumCache.TryGetValue(song.AlbumId, out found);
                if (found != null)
                {
                    song.Album = found;
                    song.Album.Songs.Add(song);
                }
            }

            for (int i = 0; i < albums.Count; i++)
            {
                var album = albums[i];

                Artist tmp = null;
                artistCache.TryGetValue(album.ArtistId, out tmp);
                if (tmp != null)
                {
                    album.Artist = tmp;
                    album.Artist.Albums.Add(album);
                }
            }

            for (int i = 0; i < playlistsAudioMedia.Count; i++)
            {
                AudioMedia audioMediaTmp = null;
                Playlist playlistTmp = null;
                songCache.TryGetValue(playlistsAudioMedia[i].AudioMediaId, out audioMediaTmp);
                playlistCache.TryGetValue(playlistsAudioMedia[i].PlaylistId, out playlistTmp);
                if (audioMediaTmp != null && playlistTmp != null)
                {
                    playlistTmp.Songs.Add(audioMediaTmp);
                }
            }
        }

        private void CheckFilesystemIds(List<AudioMedia> songs, List<VideoMedia> videos)
        {
            try
            {
                Dictionary<string, bool> files = new Dictionary<string, bool>();
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var tmp = store.GetFileNames("medias/*");
                    if (tmp != null && tmp.Length > 0)
                    {
                        foreach (var item in tmp)
                        {
                            files.Add(item, false);
                        }
                    }

                    if (files != null)
                    {
                        if (songs != null)
                        {
                            var copy = songs.ToList();
                            foreach (var song in copy)
                            {
                                if (!string.IsNullOrEmpty(song.FilesystemId))
                                {
                                    if (files.ContainsKey(song.FilesystemId))
                                    {
                                        files[song.FilesystemId] = true;
                                    }
                                    else
                                    {
                                        this.DeleteAudioMediaInternal(song);
                                    }
                                }
                            }
                        }
                        if (videos != null)
                        {
                            var copy = videos.ToList();
                            foreach (var video in copy)
                            {
                                if (!string.IsNullOrEmpty(video.FilesystemId))
                                {
                                    if (files.ContainsKey(video.FilesystemId))
                                    {
                                        files[video.FilesystemId] = true;
                                    }
                                    else
                                    {
                                        this.DeleteVideoMediaInternal(video);
                                    }
                                }
                            }
                        }
                    }

                    foreach (var item in files)
                    {
                        if (!item.Value)
                        {
                            store.DeleteFile(Path.Combine("medias", item.Key));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // catch everything just to avoid a crash...
            }
        }
    }
}

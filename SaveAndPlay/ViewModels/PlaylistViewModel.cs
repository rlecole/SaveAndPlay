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
using SaveAndPlay.Resources;
using SaveAndPlay.Scheduler;
using SaveAndPlay.ViewModels.Messages;
using SaveAndPlay.ViewService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SaveAndPlay.ViewModels
{
    public class PlaylistViewModel : BaseViewModel, IDataServiceSchedulerClient
    {
        private Playlist playlist;
        public Playlist Playlist
        {
            get { return this.playlist; }
            set { this.playlist = value; RaisePropertyChanged("Playlist"); }
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
                    this.NavigationViewService.NavigateTo("/UI/Pages/AudioMediaPage.xaml",
                                                                                            new NavigationParameter("songId", selectedSong.Id.ToString()));
                }
            }
        }

        private bool noSongInPlaylist;
        public bool NoSongInPlaylist
        {
            get { return this.noSongInPlaylist; }
            set { this.noSongInPlaylist = value; RaisePropertyChanged("NoSongInPlaylist"); }
        }

        public void Save(string newTitle)
        {
            if (!string.IsNullOrEmpty(newTitle.Trim()))
            {
                if (newTitle != this.Playlist.Title)
                {
                    if (this.Playlist.Id == 0)
                    {
                        App.DataCacheServiceInstance.AddPlaylist(newTitle, TaskParam.AsyncContract("AddPlaylist"));
                    }
                    else
                    {
                        this.Playlist.Title = newTitle;
                        App.DataCacheServiceInstance.UpdatePlaylist(this.Playlist, TaskParam.AsyncContract("UpdatePlaylist"));
                    }
                }
                this.NavigationViewService.GoBack();
            }
            else
            {
                this.DialogViewService.MessageBoxOk(Main.Error, MusicList.TitleMustBeFilled);
            }
        }

        public void Delete()
        {
            if (this.Playlist.Id != 0)
            {
                if (this.DialogViewService.MessageBoxOKCancel(Resources.Main.Warning, string.Format(Resources.VideoList.ConfirmDelete, this.Playlist.Title)))
                {
                    App.DataCacheServiceInstance.DeletePlaylist(this.Playlist, TaskParam.AsyncContract("DeletePlaylist"));
                    this.NavigationViewService.GoBack();
                }
            }
        }

        private void Load()
        {
            if (App.DataCacheServiceInstance != null &&
                App.DataCacheServiceInstance.Playlists != null)
            {
                var parameters = this.NavigationViewService.GetParametersFromUri();
                var id = Int32.Parse(parameters.First().Value);
                if (id > 0)
                {
                    this.Playlist = App.DataCacheServiceInstance.Playlists.FirstOrDefault(x => x.Id == id);
                }
                else
                {
                    this.Playlist = new Playlist();
                }
                this.NoSongInPlaylist = this.Playlist.Songs == null || this.Playlist.Songs.Count <= 0;
            }
        }

        public void Unload()
        {
            this.Playlist = null;
        }

        public void TaskRunning(string current, List<string> toCome)
        {
            if (current == null && (toCome == null || toCome.Count == 0))
            {
                this.Ready();
            }
        }

        public void TaskCompleted(DataAsyncResult result, string current, List<string> toCome)
        {
            if (toCome == null || toCome.Count == 0)
            {
                this.Ready();
            }
        }

        private void Ready()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                this.Load();
                base.MessengerInstance.Send(ScreenLock.Release());
            });
        }
    }
}

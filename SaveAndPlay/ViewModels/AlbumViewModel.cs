﻿#region Apache License, Version 2.0 
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
using SaveAndPlay.ViewModels.Messages;
using SaveAndPlay.ViewService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SaveAndPlay.ViewModels
{
    public class AlbumViewModel : BaseViewModel, IDataServiceSchedulerClient
    {
        private Album album;
        public Album Album
        {
            get { return this.album; }
            set { this.album = value; RaisePropertyChanged("Album"); }
        }

        private bool isEditable;
        public bool IsEditable
        {
            get { return this.isEditable; }
            set { this.isEditable = value; RaisePropertyChanged("IsEditable"); }
        }

        private AudioMedia selectedSong;
        public AudioMedia SelectedSong
        {
            get { return this.selectedSong; }
            set
            {
                this.selectedSong = value;
                this.NavigationViewService.NavigateTo("/UI/Pages/AudioMediaPage.xaml",
                                                                                        new NavigationParameter("songId", selectedSong.Id.ToString()));
            }
        }

        public void Rename(string newTitle)
        {
            if (!string.IsNullOrEmpty(newTitle))
            {
                if (newTitle != this.Album.Title)
                {
                    this.Album.Title = newTitle;
                    App.DataCacheServiceInstance.UpdateAlbum(this.Album, TaskParam.AsyncContract("UpdateAlbum"));
                }
                this.NavigationViewService.GoBack();
            }
            else
            {
                this.DialogViewService.MessageBoxOk(Resources.Main.Error, Resources.MusicList.TitleMustBeFilled);
            }
        }

        private void Load()
        {
            if (App.DataCacheServiceInstance != null &&
                App.DataCacheServiceInstance.Albums != null)
            {
                var parameters = this.NavigationViewService.GetParametersFromUri();
                var id = Int32.Parse(parameters.First().Value);
                this.Album = App.DataCacheServiceInstance.Albums.FirstOrDefault(x => x.Id == id);
                this.IsEditable = !string.IsNullOrEmpty(this.Album.Title);
            }
        }

        public void Unload()
        {
            this.Album = null;
            this.IsEditable = true;
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
                base.MessengerInstance.Send(ScreenLock.Release());
            });
        }
    }
}

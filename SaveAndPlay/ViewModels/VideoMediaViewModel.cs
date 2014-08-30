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

using GalaSoft.MvvmLight;
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.DAL.Entities;
using SaveAndPlay.Helpers;
using SaveAndPlay.Resources;
using SaveAndPlay.Scheduler;
using SaveAndPlay.Service;
using SaveAndPlay.ViewModels.Messages;
using SaveAndPlay.ViewService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace SaveAndPlay.ViewModels
{
    public class VideoMediaViewModel : BaseViewModel, IDataServiceSchedulerClient
    {
        private VideoMedia videoMedia;
        public VideoMedia VideoMedia
        {
            get { return this.videoMedia; }
            set { this.videoMedia = value; RaisePropertyChanged("VideoMedia"); }
        }

        private string videoTitle;
        public string VideoTitle
        {
            get { return videoTitle; }
            set { videoTitle = value; RaisePropertyChanged("VideoTitle"); }
        }

        private bool isProtected;
        public bool IsProtected
        {
            get { return isProtected; }
            set { isProtected = value; RaisePropertyChanged("IsProtected"); }
        }

        public void Load()
        {
            if (App.DataCacheServiceInstance != null)
            {
                var parameters = this.NavigationViewService.GetParametersFromUri();
                var id = Int32.Parse(parameters.First().Value);

                this.VideoMedia = App.DataCacheServiceInstance.Videos.FirstOrDefault(x => x.Id == id);
                this.VideoTitle = this.VideoMedia.Title;
                this.IsProtected = this.VideoMedia.IsProtected;
            }
        }

        public void Save()
        {
            if (!string.IsNullOrEmpty(this.VideoTitle.Trim()))
            {
                App.DataCacheServiceInstance.UpdateVideoMedia(this.VideoMedia, this.VideoTitle, this.IsProtected, TaskParam.AsyncContract("UpdateVideoMedia"));
                this.NavigationViewService.GoBack();
            }
            else
            {
                this.DialogViewService.MessageBoxOk(Main.Error, MusicList.TitleMustBeFilled);
            }
        }

        public void Delete()
        {
            if (this.DialogViewService.MessageBoxOKCancel(Resources.Main.Warning, string.Format(Resources.VideoList.ConfirmDelete, this.VideoMedia.Title)))
            {
                IsolatedStorageFileHelper.DeleteFileIfExists(Path.Combine(this.MediaSupportService.GetStorageDirectory(), this.VideoMedia.FilesystemId));
                App.DataCacheServiceInstance.DeleteVideoMedia(this.VideoMedia, TaskParam.AsyncContract("DeleteVideoMedia"));
                this.NavigationViewService.GoBack();
            }
        }

        public void SendToAudioLibrary()
        {
            if (this.DialogViewService.MessageBoxOKCancel(Resources.Main.Warning, string.Format(Resources.VideoList.ConfirmSendToAudio, this.VideoMedia.Title)))
            {
                App.DataCacheServiceInstance.SendToAudioLibrary(this.VideoMedia, TaskParam.AsyncContract("SendToAudioLibrary"));
                this.NavigationViewService.GoBack();
            }
        }

        public void Unload()
        {
            this.VideoMedia = null;
            this.VideoTitle = null;
            this.IsProtected = false;
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

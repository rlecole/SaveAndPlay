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

using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.Helpers;
using SaveAndPlay.Scheduler;
using SaveAndPlay.Service;
using SaveAndPlay.ViewModels.Messages;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace SaveAndPlay.ViewModels
{
    public class ProtectedModeViewModel : BaseViewModel, IDataServiceSchedulerClient
    {
        private bool protectedMode = false;
        public bool ProtectedMode
        {
            get { return protectedMode; }
            set { protectedMode = value; base.RaisePropertyChanged("ProtectedMode", protectedMode, value, true); }
        }

        public void ValidatePassword(string password)
        {
            if (this.ApplicationSettingsService.ValidatePassword(password))
            {
                App.DataCacheServiceInstance.EnableProtectMode(TaskParam.AsyncContract("EnableProtectMode"));
                this.ProtectedMode = true;
                this.NavigationViewService.GoBack();
            }
        }

        public void Disable()
        {
            if (this.ProtectedMode)
            {
                App.DataCacheServiceInstance.DisableProtectMode(TaskParam.AsyncContract("DisableProtectMode"));
                this.ProtectedMode = false;
                this.NavigationViewService.GoBack();
            }
        }

        public void Change()
        {
            this.NavigationViewService.NavigateTo("/UI/Pages/ProtectedModeNewPasswordPage.xaml");
        }

        public void Reset()
        {
            this.PlaybackAgentService.Clear();
            var songsToDelete = App.DataCacheServiceInstance.Songs.Where(x => x.IsProtected).ToList();
            foreach (var song in songsToDelete)
            {
                try { IsolatedStorageFileHelper.DeleteFileIfExists(Path.Combine(this.MediaSupportService.GetStorageDirectory(), song.FilesystemId)); }
                catch { }
            }
            var videosToDelete = App.DataCacheServiceInstance.Videos.Where(x => x.IsProtected).ToList();
            foreach (var video in videosToDelete)
            {
                try { IsolatedStorageFileHelper.DeleteFileIfExists(Path.Combine(this.MediaSupportService.GetStorageDirectory(), video.FilesystemId)); }
                catch { }
            }

            App.DataCacheServiceInstance.DeleteAllProtectedMedia(TaskParam.AsyncAction("DeleteAllProtectedMedia", (state) =>
            {
                if (state.IsCompleted)
                {
                    this.ApplicationSettingsService.SetPassword(string.Empty);
                }
            }));
            this.NavigationViewService.GoBack();
        }

        public void Load()
        {
            var parameters = this.NavigationViewService.GetParametersFromUri();
            if (parameters != null && parameters.Count() > 0)
            {
                this.NavigationViewService.RemoveBackEntry();
            }
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
                base.MessengerInstance.Send(ScreenLock.Release());
            });
        }
    }
}

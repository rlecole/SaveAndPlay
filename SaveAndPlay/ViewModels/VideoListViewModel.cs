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
using SaveAndPlay.Scheduler;
using SaveAndPlay.ViewModels.Messages;
using SaveAndPlay.ViewService;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SaveAndPlay.ViewModels
{
    public class VideoListViewModel : BaseViewModel, IDataServiceSchedulerClient
    {
        private List<VideoMedia> videos;
        public List<VideoMedia> Videos
        {
            get { return this.videos; }
            set { this.videos = value; RaisePropertyChanged("Videos"); }
        }

        private bool noVideo;
        public bool NoVideo
        {
            get { return this.noVideo; }
            set { this.noVideo = value; RaisePropertyChanged("NoVideo"); }
        }

        private VideoMedia selectedVideo;
        public VideoMedia SelectedVideo
        {
            get { return this.selectedVideo; }
            set
            {
                if (value != null)
                {
                    this.selectedVideo = value;
                    this.NavigationViewService.NavigateTo("/UI/Pages/VideoMediaPage.xaml", new NavigationParameter("videoId", selectedVideo.Id.ToString()));
                }
            }
        }

        public void Unload()
        {
            this.Videos = null;
            this.SelectedVideo = null;
        }

        public void Play(VideoMedia media)
        {
            this.NavigationViewService.NavigateTo("/UI/Pages/VideoPlayerPage.xaml",
                                                                                     new NavigationParameter("videoId", media.Id.ToString()));
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
                this.Videos = App.DataCacheServiceInstance.VideosVisible.ToList();
                this.NoVideo = this.Videos.Count <= 0;
                base.MessengerInstance.Send(ScreenLock.Release());
            });
        }
    }
}

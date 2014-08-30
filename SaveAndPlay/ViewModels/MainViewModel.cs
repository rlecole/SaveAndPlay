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

using Microsoft.Phone.BackgroundAudio;
using SaveAndPlay.Helpers;
using SaveAndPlay.Scheduler;
using SaveAndPlay.ViewModels.Messages;
using SaveAndPlay.ViewService;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace SaveAndPlay.ViewModels
{
    public class MainViewModel : BaseViewModel, IDataServiceSchedulerClient
    {
        private DispatcherTimer playTimer = new DispatcherTimer();

        private string currentPlayingMedia;
        public string CurrentPlayingMedia
        {
            get { return currentPlayingMedia; }
            set { currentPlayingMedia = value; RaisePropertyChanged("CurrentPlayingMedia"); }
        }

        private string currentPlayingMediaPosition;
        public string CurrentPlayingMediaPosition
        {
            get { return currentPlayingMediaPosition; }
            set { currentPlayingMediaPosition = value; RaisePropertyChanged("CurrentPlayingMediaPosition"); }
        }

        private string currentPlayingMediaIcon;
        public string CurrentPlayingMediaIcon
        {
            get { return currentPlayingMediaIcon; }
            set { currentPlayingMediaIcon = value; RaisePropertyChanged("CurrentPlayingMediaIcon"); }
        }

        private string audioItems;
        public string AudioItems
        {
            get { return audioItems; }
            set { audioItems = value; RaisePropertyChanged("AudioItems"); }
        }

        private string videoItems;
        public string VideoItems
        {
            get { return videoItems; }
            set { videoItems = value; RaisePropertyChanged("VideoItems"); }
        }

        public MainViewModel()
        {
        }

        public void Load()
        {
            if (BackgroundAudioPlayer.Instance != null && BackgroundAudioPlayer.Instance.Track != null)
            {
                this.CurrentPlayingMedia = BackgroundAudioPlayer.Instance.Track.Title;
                this.CurrentPlayingMediaIcon = "/Images/musicfile.png";
                this.CurrentPlayingMediaPosition = UIHelper.FormatTimeSpan(BackgroundAudioPlayer.Instance.Position);
                this.playTimer.Interval = TimeSpan.FromMilliseconds(1000);
                this.playTimer.Tick += OnPlayTimerTick;
                this.playTimer.Start();
                BackgroundAudioPlayer.Instance.PlayStateChanged += OnPlayStateChanged;
                this.ApplicationSettingsService.SaveVideoPosition(null);
            }
        }

        private void Ready()
        {
            if (App.DataCacheServiceInstance != null &&
                App.DataCacheServiceInstance.Videos != null &&
                App.DataCacheServiceInstance.Songs != null)
            {
                if (BackgroundAudioPlayer.Instance == null || BackgroundAudioPlayer.Instance.Track == null)
                {
                    var videoPosition = this.ApplicationSettingsService.GetVideoPosition();
                    if (videoPosition != null)
                    {
                        var video = App.DataCacheServiceInstance.VideosVisible.FirstOrDefault(x => x.Id == videoPosition.Id);
                        if (video != null)
                        {
                            this.CurrentPlayingMedia = video.Title;
                            this.CurrentPlayingMediaPosition = UIHelper.FormatTimeSpan(videoPosition.Position);
                            this.CurrentPlayingMediaIcon = "/Images/videofile.png";
                        }
                    }
                }
                this.AudioItems = string.Format(Resources.Main.AudioItems, App.DataCacheServiceInstance.SongsVisible.Count());
                this.VideoItems = string.Format(Resources.Main.VideoItems, App.DataCacheServiceInstance.VideosVisible.Count());
            }
        }

        public void Unload()
        {
            BackgroundAudioPlayer.Instance.PlayStateChanged -= OnPlayStateChanged;
            this.playTimer.Tick -= OnPlayTimerTick;
            this.playTimer.Stop();
            this.AudioItems = string.Empty;
            this.VideoItems = string.Empty;
            this.CurrentPlayingMedia = string.Empty;
            this.CurrentPlayingMediaPosition = string.Empty;
            this.CurrentPlayingMediaIcon = string.Empty;
        }

        public void OnPlayStateChanged(object sender, EventArgs e)
        {
            switch (BackgroundAudioPlayer.Instance.PlayerState)
            {
                case PlayState.Playing:
                    this.CurrentPlayingMedia = BackgroundAudioPlayer.Instance.Track.Title;
                    this.CurrentPlayingMediaPosition = "00:00";
                    break;
                default:
                    break;
            }
        }

        public void OnPlayTimerTick(object sender, EventArgs e)
        {
            if (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing)
            {
                this.CurrentPlayingMediaPosition = UIHelper.FormatTimeSpan(BackgroundAudioPlayer.Instance.Position);
            }
        }

        public void TaskRunning(string current, System.Collections.Generic.List<string> toCome)
        {
            if (this.DataLoadingScopeViewService.WaitingForImport(current, toCome))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    this.MessengerInstance.Send(ScreenLock.Strict(Resources.Main.Importing));
                });
            }

            if (!this.DataLoadingScopeViewService.WaitingForAll(current, toCome))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    this.Ready();
                    this.MessengerInstance.Send(ScreenLock.Release());
                });
            }
        }

        public void TaskCompleted(DataAsyncResult result, string current, System.Collections.Generic.List<string> toCome)
        {
            if (!this.DataLoadingScopeViewService.WaitingForImport(null, toCome))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    this.MessengerInstance.Send(ScreenLock.Release());
                });
            }

            if (!this.DataLoadingScopeViewService.WaitingForAll(null, toCome))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    this.Ready();
                });
            }
        }

        public void PlayingNow()
        {
            var videoPosition = this.ApplicationSettingsService.GetVideoPosition();
            if (BackgroundAudioPlayer.Instance != null && BackgroundAudioPlayer.Instance.Track != null)
            {
                this.NavigationViewService.NavigateTo("/UI/Pages/AudioPlayerPage.xaml");
                return;
            }
            else if (videoPosition != null)
            {
                var video = App.DataCacheServiceInstance.VideosVisible.FirstOrDefault(x => x.Id == videoPosition.Id);
                if (video != null)
                {
                    this.NavigationViewService.NavigateTo("/UI/Pages/VideoPlayerPage.xaml",
                                                          new NavigationParameter("videoId", video.Id.ToString()));
                    return;
                }
            }
            this.DialogViewService.MessageBoxOk(Resources.Main.Info, Resources.Main.NothingToRead);
        }
    }
}
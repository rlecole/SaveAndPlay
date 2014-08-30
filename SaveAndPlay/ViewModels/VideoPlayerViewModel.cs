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
using GalaSoft.MvvmLight.Messaging;
using SaveAndPlay.DAL.Entities;
using SaveAndPlay.Scheduler;
using SaveAndPlay.Service;
using SaveAndPlay.ViewModels.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SaveAndPlay.ViewModels
{
    public class VideoPlayerViewModel : BaseViewModel, IDataServiceSchedulerClient
    {
        private MediaElement mediaElement;
        private DispatcherTimer progressTimer;
        private VideoMedia currentVideo;
        private Stream currentStream = null;

        private Uri selectedVideo;
        public Uri SelectedVideo
        {
            get { return this.selectedVideo; }
            set
            {
                this.selectedVideo = value;
                this.RaisePropertyChanged("SelectedVideo");
            }
        }

        private string currentProgress;
        public string CurrentProgress
        {
            get { return this.currentProgress; }
            set
            {
                this.currentProgress = value;
                RaisePropertyChanged("CurrentProgress");
            }
        }

        private double totalDuration;
        public double TotalDuration
        {
            get { return this.totalDuration; }
            set
            {
                this.totalDuration = value;
                this.RaisePropertyChanged("TotalDuration");
            }
        }

        private double currentPosition;
        public double CurrentPosition
        {
            get { return this.currentPosition; }
            set
            {
                this.currentPosition = value;
                this.RaisePropertyChanged("CurrentPosition");
            }
        }

        public ICommand MediaOpenedCommand { get; set; }

        public void MediaOpened(object param)
        {
            this.progressTimer = new DispatcherTimer();
            this.progressTimer.Interval = TimeSpan.FromMilliseconds(1000);
            this.progressTimer.Tick += new EventHandler(this.ProgressTimer_Tick);

            this.SetCurrentPosition();
            this.Play();

            var lastVideoPosition = this.ApplicationSettingsService.GetVideoPosition();
            if (lastVideoPosition != null && lastVideoPosition.Id == this.currentVideo.Id)
            {
                this.Seek(lastVideoPosition.Position.TotalSeconds);
            }
            else
            {
                this.SavePosition(true);
            }
        }

        public void MediaFailed()
        {
            this.DialogViewService.MessageBoxOk(Resources.Main.Error, Resources.VideoList.Error);
            this.NavigationViewService.GoBack();
        }

        public VideoPlayerViewModel(MediaElement mediaElement)
        {
            this.mediaElement = mediaElement;
            this.MediaOpenedCommand = new RelayCommand<object>(MediaOpened, (o) => { return true; });
        }

        public void SetVideo(VideoMedia video)
        {
            try
            {
                // Set Video
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    this.currentStream = store.OpenFile(Path.Combine(this.MediaSupportService.GetStorageDirectory(), video.FilesystemId), FileMode.Open);
                }
                this.mediaElement.SetSource(this.currentStream);

                // Stop Progress Timer
                if (this.progressTimer != null)
                {
                    if (this.progressTimer.IsEnabled)
                    {
                        this.progressTimer.Stop();
                    }
                }
            }
            catch (Exception e)
            {
                this.MediaFailed();
            }
        }

        public void Seek(double position)
        {
            if (this.mediaElement != null && this.mediaElement.CanSeek && this.currentStream != null)
            {
                this.mediaElement.Position = new TimeSpan(0, 0, (int)position);
                this.SetCurrentPosition();
            }
        }

        public void Play()
        {
            // Play Video
            if (this.mediaElement != null && this.mediaElement.CurrentState != MediaElementState.Playing &&
                this.progressTimer != null && this.currentStream != null)
            {
                this.mediaElement.Play();
                this.progressTimer.Start();
            }
        }

        public void StopVideo()
        {
            // Stop Video
            if (this.mediaElement != null && this.progressTimer != null && this.currentStream != null)
            {
                this.mediaElement.Stop();
                this.progressTimer.Stop();
                this.SetCurrentPosition();
                this.ApplicationSettingsService.SaveVideoPosition(null);
            }
        }

        public void Pause()
        {
            if (this.mediaElement != null && this.progressTimer != null && this.currentStream != null)
            {
                // We only want to Pause if the media is Playing
                if (this.mediaElement.CurrentState == MediaElementState.Playing)
                {
                    // If we can Pause the Video, Pause it
                    if (this.mediaElement.CanPause)
                    {
                        // Pause Video
                        this.mediaElement.Pause();
                    }
                    else
                    {
                        // We can't pause the video so stop it
                        this.mediaElement.Stop();
                    }

                    if (progressTimer.IsEnabled)
                    {
                        this.progressTimer.Stop();
                    }
                }
            }
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            this.SetCurrentPosition();
        }

        private void SetCurrentPosition()
        {
            if (this.mediaElement != null && this.currentStream != null)
            {
                // If the Media play is complete stop the media
                if (this.CurrentPosition > 0)
                {
                    if (this.CurrentPosition >= TotalDuration)
                    {
                        this.CurrentPosition = 0;
                        this.StopVideo();
                    }
                }

                // Update the time text e.g. 01:50 / 03:30
                this.CurrentProgress = string.Format("{0}:{1} / {2}:{3}",
                                                    Math.Floor(mediaElement.Position.TotalMinutes).ToString("00"),
                                                    mediaElement.Position.Seconds.ToString("00"),
                                                    Math.Floor(mediaElement.NaturalDuration.TimeSpan.TotalMinutes).ToString("00"),
                                                    mediaElement.NaturalDuration.TimeSpan.Seconds.ToString("00"));

                this.CurrentPosition = mediaElement.Position.TotalSeconds;
                this.TotalDuration = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            }
        }

        public void SavePosition(bool force)
        {
            if (mediaElement != null &&
                (mediaElement.CurrentState == MediaElementState.Playing ||
                mediaElement.CurrentState == MediaElementState.Paused ||
                force))
            {
                this.ApplicationSettingsService.SaveVideoPosition(new VideoPosition()
                {
                    Id = this.currentVideo.Id,
                    Position = mediaElement.Position
                });
            }
        }

        public void Clean()
        {
            if (this.currentStream != null)
            {
                this.currentStream.Close();
                this.currentStream.Dispose();
                this.currentStream = null;
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
                Messenger.Default.Send(ScreenLock.Release());
                if (App.DataCacheServiceInstance != null &&
                    App.DataCacheServiceInstance.Videos != null)
                {
                    var parameters = this.NavigationViewService.GetParametersFromUri();
                    var id = Int32.Parse(parameters.First().Value);
                    this.currentVideo = App.DataCacheServiceInstance.VideosVisible.FirstOrDefault(x => x.Id == id);
                    this.SetVideo(this.currentVideo);
                }
            });
        }
    }
}
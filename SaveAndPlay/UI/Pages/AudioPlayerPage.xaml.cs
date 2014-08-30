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
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.Helpers;
using SaveAndPlay.ViewService;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace SaveAndPlay.UI.Pages
{
    public partial class AudioPlayerPage : BasePage
    {
        public AudioTrack Track
        {
            get { return (AudioTrack)GetValue(TrackProperty); }
            set { SetValue(TrackProperty, value); }
        }
        public static readonly DependencyProperty TrackProperty =
            DependencyProperty.Register("Track", typeof(AudioTrack), typeof(AudioPlayerPage), new PropertyMetadata(null));

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(AudioPlayerPage), new PropertyMetadata(0d));

        public double CurrentPosition
        {
            get { return (double)GetValue(CurrentPositionProperty); }
            set { SetValue(CurrentPositionProperty, value); }
        }
        public static readonly DependencyProperty CurrentPositionProperty =
            DependencyProperty.Register("CurrentPosition", typeof(double), typeof(AudioPlayerPage), new PropertyMetadata(0d));

        public string CurrentTime
        {
            get { return (string)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }
        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register("CurrentTime", typeof(string), typeof(AudioPlayerPage), new PropertyMetadata(string.Empty));

        public string EndTime
        {
            get { return (string)GetValue(EndTimeProperty); }
            set { SetValue(EndTimeProperty, value); }
        }
        public static readonly DependencyProperty EndTimeProperty =
            DependencyProperty.Register("EndTime", typeof(string), typeof(AudioPlayerPage), new PropertyMetadata(string.Empty));

        private DispatcherTimer playTimer = new DispatcherTimer();

        private IPlaybackAgentService playbackAgentService;
        public IPlaybackAgentService PlaybackAgentService
        {
            get
            {
                if (this.playbackAgentService == null)
                {
                    this.playbackAgentService = ServiceLocator.Current.GetInstance<IPlaybackAgentService>();
                }
                return this.playbackAgentService;
            }
        }

        public AudioPlayerPage()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public void playTimer_Tick(object sender, EventArgs e)
        {
            if (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing)
            {
                this.CurrentPosition = BackgroundAudioPlayer.Instance.Position.TotalSeconds;
                this.CurrentTime = UIHelper.FormatTimeSpan(BackgroundAudioPlayer.Instance.Position);
            }
        }

        public void OnPlayStateChanged(object sender, EventArgs e)
        {
            switch (BackgroundAudioPlayer.Instance.PlayerState)
            {
                case PlayState.Playing:
                    this.ShowNewTrack();
                    break;
                default:
                    break;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (BackgroundAudioPlayer.Instance.Track != null)
            {
                this.Track = BackgroundAudioPlayer.Instance.Track;
                this.CurrentPosition = BackgroundAudioPlayer.Instance.Position.TotalSeconds;
                this.Maximum = BackgroundAudioPlayer.Instance.Track.Duration.TotalSeconds;
                this.CurrentTime = UIHelper.FormatTimeSpan(BackgroundAudioPlayer.Instance.Position);
                this.EndTime = UIHelper.FormatTimeSpan(BackgroundAudioPlayer.Instance.Track.Duration);
            }
            this.playTimer.Interval = TimeSpan.FromMilliseconds(1000);
            this.playTimer.Tick += playTimer_Tick;
            this.playTimer.Start();
            BackgroundAudioPlayer.Instance.PlayStateChanged += OnPlayStateChanged;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            BackgroundAudioPlayer.Instance.PlayStateChanged -= OnPlayStateChanged;
            this.playTimer.Tick -= playTimer_Tick;
            this.playTimer.Stop();
            this.Reset();
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            this.PlaybackAgentService.SetPosition(new TimeSpan(0, 0, (int)this.ProgressSlider.Value));
            BackgroundAudioPlayer.Instance.Play();
        }

        private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            this.PlaybackAgentService.Pause();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            this.PlaybackAgentService.Pause();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            this.PlaybackAgentService.Resume();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            this.NextTrackInternal();
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            this.PreviousTrackInternal();
        }

        private void Reset()
        {
            this.Track = null;
            this.EndTime = string.Empty;
            this.CurrentPosition = 0;
            this.CurrentTime = string.Empty;
            this.Maximum = 0;
        }

        private void NextTrackInternal()
        {
            this.PlaybackAgentService.Next();
        }

        private void PreviousTrackInternal()
        {
            this.PlaybackAgentService.Previous();
        }

        private void ShowNewTrack()
        {
            this.Track = BackgroundAudioPlayer.Instance.Track;
            this.Maximum = BackgroundAudioPlayer.Instance.Track.Duration.TotalSeconds;
            this.EndTime = UIHelper.FormatTimeSpan(BackgroundAudioPlayer.Instance.Track.Duration);
        }
    }
}

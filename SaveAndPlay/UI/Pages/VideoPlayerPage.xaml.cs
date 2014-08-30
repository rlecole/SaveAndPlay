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
using SaveAndPlay.ViewModels;
using SaveAndPlay.ViewService;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace SaveAndPlay.UI.Pages
{
    public partial class VideoPlayerPage : BasePage
    {
        public VideoPlayerPage()
        {
            InitializeComponent();
            this.baseProgressOverlay = this.progressOverlay;
            this.baseProgressBarLabel = this.progressBarLabel;
            this.DataContext = new VideoPlayerViewModel(this.mediaElement);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.PlaybackAgentService.Clear();
            this.progressOverlay.Visibility = System.Windows.Visibility.Visible;
            App.DataCacheServiceInstance.Register((VideoPlayerViewModel)this.DataContext);
        }
    
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            App.DataCacheServiceInstance.Unregister((VideoPlayerViewModel)this.DataContext);
            ((VideoPlayerViewModel)this.DataContext).SavePosition(false);
            ((VideoPlayerViewModel)this.DataContext).Clean();
            this.progressOverlay.Visibility = System.Windows.Visibility.Visible;
        }

        private void ProgressSlider_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            ((VideoPlayerViewModel)this.DataContext).Seek(this.ProgressSlider.Value);
            ((VideoPlayerViewModel)this.DataContext).Play();
        }

        private void ProgressSlider_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            ((VideoPlayerViewModel)this.DataContext).Pause();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            ((VideoPlayerViewModel)this.DataContext).Pause();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            ((VideoPlayerViewModel)this.DataContext).Play();
        }

        private void mediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ((VideoPlayerViewModel)this.DataContext).MediaFailed();
        }
    }
}
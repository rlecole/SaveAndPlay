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
using SaveAndPlay.DAL.Entities;
using SaveAndPlay.ViewModels;
using SaveAndPlay.ViewService;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SaveAndPlay.UI.Pages
{
    public partial class AlbumPage : BasePage
    {
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

        public AlbumPage()
        {
            InitializeComponent();
            base.baseProgressOverlay = this.progressOverlay;
            base.baseProgressBarLabel = this.baseProgressBarLabel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.progressOverlay.Visibility = System.Windows.Visibility.Visible;
            App.DataCacheServiceInstance.Register((AlbumViewModel)this.DataContext);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.DataCacheServiceInstance.Unregister((AlbumViewModel)this.DataContext);
            this.progressOverlay.Visibility = System.Windows.Visibility.Visible;
            ((AlbumViewModel)this.DataContext).Unload();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ListBox.SelectedItem != null)
            {
                //((AlbumViewModel)this.DataContext).SelectedSong = (AudioMedia)this.ListBox.SelectedItem;
                this.ListBox.SelectedItem = null;
            }
        }

        private void Save_ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            ((AlbumViewModel)this.DataContext).Rename(this.AlbumTextBox.Text);
        }

        private void SongButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                var media = ((Button)sender).DataContext as AudioMedia;
                if (media != null)
                {
                    this.PlaybackAgentService.Play(media);
                }
            }
        }
    }
}
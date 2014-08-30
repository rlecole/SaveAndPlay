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
    public partial class ArtistPage : BasePage
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

        public ArtistPage()
        {
            InitializeComponent();
            base.baseProgressOverlay = this.progressOverlay;
            base.baseProgressBarLabel = this.baseProgressBarLabel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.progressOverlay.Visibility = System.Windows.Visibility.Visible;
            App.DataCacheServiceInstance.Register((ArtistViewModel)this.DataContext);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.DataCacheServiceInstance.Unregister((ArtistViewModel)this.DataContext);
            this.progressOverlay.Visibility = System.Windows.Visibility.Visible;
            ((ArtistViewModel)this.DataContext).Unload();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ListBox.SelectedItem != null)
            {
                ((ArtistViewModel)this.DataContext).SelectedAlbum = (Album)this.ListBox.SelectedItem;
                this.ListBox.SelectedItem = null;
            }
        }

        private void Save_ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            ((ArtistViewModel)this.DataContext).Rename(this.ArtistTextBox.Text);
        }

        private void AlbumButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                var media = ((Button)sender).DataContext as Album;
                if (media != null)
                {
                    this.PlaybackAgentService.Play(media);
                }
            }
        }
    }
}
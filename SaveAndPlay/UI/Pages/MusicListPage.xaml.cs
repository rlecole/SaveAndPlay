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

using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.DAL.Entities;
using SaveAndPlay.Service;
using SaveAndPlay.ViewModels;
using SaveAndPlay.ViewModels.Messages;
using SaveAndPlay.ViewService;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SaveAndPlay.UI.Pages
{
    public partial class MusicListPage : BasePage
    {
        public MusicListPage()
        {
            InitializeComponent();
            base.baseProgressOverlay = this.progressOverlay;
            base.baseProgressBarLabel = this.baseProgressBarLabel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.progressOverlay.Visibility = System.Windows.Visibility.Visible;
            App.DataCacheServiceInstance.Register((MusicListViewModel)this.DataContext);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.DataCacheServiceInstance.Unregister((MusicListViewModel)this.DataContext);
            ((MusicListViewModel)this.DataContext).Unload();
            this.progressOverlay.Visibility = System.Windows.Visibility.Visible;
        }

        private void ApplicationBarMenuItem_Settings_Click(object sender, EventArgs e)
        {
            if (this.ApplicationSettingsService.IsDefinedPassword())
            {
                this.NavigationViewService.NavigateTo("/UI/Pages/ProtectedModePage.xaml");
            }
            else
            {
                this.NavigationViewService.NavigateTo("/UI/Pages/ProtectedModeNewPasswordPage.xaml");
            }
        }

        private void ApplicationBarMenuItem_About_Click(object sender, EventArgs e)
        {
            this.NavigationViewService.NavigateTo("/UI/Pages/AboutPage.xaml");
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

        private void ArtistButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                var media = ((Button)sender).DataContext as Artist;
                if (media != null)
                {
                    this.PlaybackAgentService.Play(media);
                }
            }
        }

        private void PlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                var media = ((Button)sender).DataContext as Playlist;
                if (media != null)
                {
                    this.PlaybackAgentService.Play(media);
                }
            }
        }

        private void SongsJumpList_SelectedItemChanged(object sender, EventArgs e)
        {
            if (this.SongsJumpList.SelectedItem != null)
            {
                ((MusicListViewModel)this.DataContext).SelectedSong = (AudioMedia)this.SongsJumpList.SelectedItem;
                this.SongsJumpList.SelectedItem = null;
            }
        }

        private void AlbumsJumpList_SelectedItemChanged(object sender, EventArgs e)
        {
            if (this.AlbumsJumpList.SelectedItem != null)
            {
                ((MusicListViewModel)this.DataContext).SelectedAlbum = (Album)this.AlbumsJumpList.SelectedItem;
                this.AlbumsJumpList.SelectedItem = null;
            }
        }

        private void ArtistsJumpList_SelectedItemChanged(object sender, EventArgs e)
        {
            if (this.ArtistsJumpList.SelectedItem != null)
            {
                ((MusicListViewModel)this.DataContext).SelectedArtist = (Artist)this.ArtistsJumpList.SelectedItem;
                this.ArtistsJumpList.SelectedItem = null;
            }
        }

        private void PlaylistsJumpList_SelectedItemChanged(object sender, EventArgs e)
        {
            if (this.PlaylistsJumpList.SelectedItem != null)
            {
                ((MusicListViewModel)this.DataContext).SelectedPlaylist = (Playlist)this.PlaylistsJumpList.SelectedItem;
                this.PlaylistsJumpList.SelectedItem = null;
            }
        }

        private void AddPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            ((MusicListViewModel)this.DataContext).NewPlaylist();
        }

        private void PlayAllRandom_Click(object sender, RoutedEventArgs e)
        {
            this.PlaybackAgentService.PlayRandom(App.DataCacheServiceInstance.SongsVisible.ToList());
        }
    }
}
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
using Microsoft.Phone.BackgroundAudio;
using SaveAndPlay.DAL.Entities;
using SaveAndPlay.ViewModels.Messages;
using SaveAndPlay.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SaveAndPlay.Service;
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.ViewService;

namespace SaveAndPlay.UI.Pages
{
    public partial class VideoListPage : BasePage
    {
        public VideoListPage()
        {
            InitializeComponent();
            base.baseProgressOverlay = this.progressOverlay;
            this.baseProgressBarLabel = this.progressBarLabel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.progressOverlay.Visibility = System.Windows.Visibility.Visible;
            App.DataCacheServiceInstance.Register((VideoListViewModel)this.DataContext);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.DataCacheServiceInstance.Unregister((VideoListViewModel)this.DataContext);
            ((VideoListViewModel)this.DataContext).Unload();
            this.progressOverlay.Visibility = System.Windows.Visibility.Visible;
        }

        private void VideosJumpList_SelectedItemChanged(object sender, EventArgs e)
        {
            if (this.VideosJumpList.SelectedItem != null)
            {
                ((VideoListViewModel)this.DataContext).SelectedVideo = (VideoMedia)this.VideosJumpList.SelectedItem;
                this.VideosJumpList.SelectedItem = null;
            }
        }

        private void VideoButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                var media = ((Button)sender).DataContext as VideoMedia;
                if (media != null)
                {
                    ((VideoListViewModel)this.DataContext).Play(media);
                }
            }
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
    }
}

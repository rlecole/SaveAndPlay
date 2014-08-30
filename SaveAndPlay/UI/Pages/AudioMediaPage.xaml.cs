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

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SaveAndPlay.DAL.Entities;
using SaveAndPlay.ViewModels;
using System;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SaveAndPlay.UI.Pages
{
    public partial class AudioMediaPage : BasePage
    {
        public AudioMediaPage()
        {
            InitializeComponent();
            base.baseProgressOverlay = this.progressOverlay;
            base.baseProgressBarLabel = this.baseProgressBarLabel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.progressOverlay.Visibility = System.Windows.Visibility.Visible;
            App.DataCacheServiceInstance.Register((AudioMediaViewModel)this.DataContext);

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.DataCacheServiceInstance.Unregister((AudioMediaViewModel)this.DataContext);
            ((AudioMediaViewModel)this.DataContext).Unload();
            if (e.NavigationMode == NavigationMode.Back)
            {
                ((AudioMediaViewModel)this.DataContext).Reset();
            }
        }

        private void PivotItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((PivotItem)e.AddedItems[0]).Name == "PropertiesPivotItem")
            {
                ((ApplicationBarIconButton)this.ApplicationBar.Buttons[2]).IsEnabled = true;
            }
            else
            {
                ((ApplicationBarIconButton)this.ApplicationBar.Buttons[2]).IsEnabled = false;
            }
        }

        private void Save_ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            ((AudioMediaViewModel)this.DataContext).Save();
        }

        private void IsVideo_ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            ((AudioMediaViewModel)this.DataContext).SendToVideoLibrary();
        }

        private void Delete_ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            ((AudioMediaViewModel)this.DataContext).Delete();
        }

        private void Tag_ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            ((AudioMediaViewModel)this.DataContext).Tag();
        }

        private void ArtistsListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ArtistsListPicker.SelectedItem != null && this.ArtistsListPicker.SelectedIndex > 0)
            {
                ((AudioMediaViewModel)this.DataContext).ListPickedArtist = (Artist)this.ArtistsListPicker.SelectedItem;
                this.ArtistsListPicker.SelectedIndex = 0;
            }
        }

        private void AlbumsListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.AlbumsListPicker.SelectedItem != null && this.AlbumsListPicker.SelectedIndex > 0)
            {
                ((AudioMediaViewModel)this.DataContext).ListPickedAlbum = (Album)this.AlbumsListPicker.SelectedItem;
                this.AlbumsListPicker.SelectedIndex = 0;
            }
        }

        private void ArtistTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((AudioMediaViewModel)this.DataContext).ArtistChanged(this.ArtistTextBox.Text);
        }

        private void Playlist_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.PlaylistListBox.SelectedItem != null)
            {
                this.PlaylistListBox.SelectedItem = null;
            }
        }
    }
}
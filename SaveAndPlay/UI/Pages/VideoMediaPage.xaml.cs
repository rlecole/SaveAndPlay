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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SaveAndPlay.ViewModels;
using SaveAndPlay.DAL.Entities;
using SaveAndPlay.UI.Pages;

namespace SaveAndPlay.UI.Pages
{
    public partial class VideoMediaPage : BasePage
    {
        public VideoMediaPage()
        {
            InitializeComponent();
            base.baseProgressOverlay = this.progressOverlay;
            this.baseProgressBarLabel = this.progressBarLabel;
        }

        private void Save_ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            ((VideoMediaViewModel)this.DataContext).Save();
        }

        private void IsAudio_ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            ((VideoMediaViewModel)this.DataContext).SendToAudioLibrary();
        }

        private void Delete_ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            ((VideoMediaViewModel)this.DataContext).Delete();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            App.DataCacheServiceInstance.Register((VideoMediaViewModel)this.DataContext);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.DataCacheServiceInstance.Unregister((VideoMediaViewModel)this.DataContext);
            ((VideoMediaViewModel)this.DataContext).Unload();
            this.progressOverlay.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
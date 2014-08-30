﻿#region Apache License, Version 2.0 
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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.ViewService;
using SaveAndPlay.ViewModels;

namespace SaveAndPlay.UI.Controls
{
    public partial class MediasControl : UserControl
    {
        public MediasControl()
        {
            InitializeComponent();
        }

        private void Playing_Click(object sender, RoutedEventArgs e)
        {
            ((MainViewModel)this.DataContext).PlayingNow();
        }

        private void Video_Click(object sender, RoutedEventArgs e)
        {
            ServiceLocator.Current.GetInstance<INavigationViewService>().NavigateTo("/UI/Pages/VideoListPage.xaml");
        }

        private void Music_Click(object sender, RoutedEventArgs e)
        {
            ServiceLocator.Current.GetInstance<INavigationViewService>().NavigateTo("/UI/Pages/MusicListPage.xaml");
        }
    }
}

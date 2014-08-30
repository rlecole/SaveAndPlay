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
using Microsoft.Phone.Tasks;
using SaveAndPlay.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace SaveAndPlay.UI.Pages
{
    public class Source
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string NavigateTo { get; set; }
        public string IconLight { get; set; }
        public string IconDark { get; set; }
    }

    public class MediaSources : ObservableCollection<Source> { }

    public partial class MediaSourcePage : BasePage
    {
        public RelayCommand<string> Navigate { get; set; }

        public MediaSourcePage()
        {
            InitializeComponent();

            this.Navigate = new RelayCommand<string>((destination) =>
            {
                if (destination.EndsWith(".xaml"))
                {
                    this.NavigationViewService.NavigateTo(destination);
                }
                else
                {
                    var webBrowserTask = new WebBrowserTask();
                    webBrowserTask.Uri = new Uri(destination, UriKind.Absolute);
                    webBrowserTask.Show();
                }
            });
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ((NewDownloadViewModel)this.DataContext).Reset();
        }

        private void Source_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SourceListBox.SelectedItem != null)
            {
                this.SourceListBox.SelectedItem = null;
            }
        }
    }
}
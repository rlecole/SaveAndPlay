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
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Phone.Tasks;
using SaveAndPlay.Resources;
using SaveAndPlay.Service;

namespace SaveAndPlay.UI.Pages
{
    public partial class AboutPage : BasePage
    {
        public string VersionLabel
        {
            get { return (string)GetValue(VersionLabelProperty); }
            set { SetValue(VersionLabelProperty, value); }
        }
        public static readonly DependencyProperty VersionLabelProperty =
            DependencyProperty.Register("VersionLabel", typeof(string), typeof(AboutPage), new PropertyMetadata(string.Empty));

        public AboutPage()
        {
            InitializeComponent();
            this.DataContext = this;
            this.VersionLabel = string.Format(About.VersionLabel, GetVersion());
        }

        private static string GetVersion()
        {
            return Regex.Match(Assembly.GetExecutingAssembly().FullName, @"Version=(?<version>[\d\.]*)").Groups["version"].Value;
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.CheckForegroundTransfers())
            {
                try
                {
                    var emailComposeTask = new EmailComposeTask();
                    emailComposeTask.Subject = "[Report] Save & Play";
                    emailComposeTask.To = "romain.lecole@gmail.com";
                    emailComposeTask.Show();
                }
                catch { }
            }
        }

        private void ReviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.CheckForegroundTransfers())
            {
                try
                {
                    var marketplaceReviewTask = new MarketplaceReviewTask();
                    marketplaceReviewTask.Show();
                }
                catch { }
            }
        }

        private void VoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.CheckForegroundTransfers())
            {
                try
                {
                    var webBrowserTask = new WebBrowserTask();
                    webBrowserTask.Uri = new Uri("http://saveandplay.uservoice.com", UriKind.Absolute);
                    webBrowserTask.Show();
                }
                catch { }
            }
        }

        private bool CheckForegroundTransfers()
        {
            bool result = false;

            if (ForegroundTransferService.AnyRequestActive)
            {
                if (MessageBox.Show(DownloadList.ActiveForegroundTransferLabel, Main.Warning, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    result = true;
                }
            }
            else
            {
                result = true;
            }
            return result;
        }
    }
}
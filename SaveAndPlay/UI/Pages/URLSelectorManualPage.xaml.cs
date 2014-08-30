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
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Shell;
using SaveAndPlay.ViewModels;

namespace SaveAndPlay.UI.Pages
{
    public partial class URLSelectorManualPage : BasePage
    {
        public URLSelectorManualPage()
        {
            InitializeComponent();
            
            base.baseProgressOverlay = this.progressOverlay;
            this.progressOverlay.Hide();
        }

        private void Validate_Click(object sender, EventArgs e)
        {
            ((NewDownloadViewModel)this.DataContext).URLValidation(URLTextBox.Text, "/UI/Pages/AddDownloadPage.xaml");
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.URLTextBox.IsEnabled = false;
                this.URLTextBox.IsEnabled = true;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ApplicationBar.IsVisible = false;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ApplicationBar.IsVisible = true;
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, CancelEventArgs e)
        {
            ((NewDownloadViewModel)this.DataContext).StopURLValidation();
        }
    }
}
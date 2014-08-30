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

using Coding4Fun.Toolkit.Controls;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Controls;
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.Resources;
using SaveAndPlay.Service;
using SaveAndPlay.ViewModels;
using SaveAndPlay.ViewModels.Messages;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SaveAndPlay.UI.Pages
{
    public partial class URLSelectorWebBrowserPage : BasePage
    {
        private static WebBrowser webBrowser = new WebBrowser();

        private const int historySize = 15;
        private const string searchUri = "http://www.bing.com/search?q=";

        private static bool isBack = false;
        private static Uri previous = null;
        private static List<Uri> history = new List<Uri>();

        public URLSelectorWebBrowserPage()
        {
            InitializeComponent();

            Messenger.Default.Register<Favorite>(this, (favorite) =>
            {
                this.WebBrowserNavigateTo(favorite.Url);
            });

            base.baseProgressOverlay = this.progressOverlay;
            this.progressOverlay.Hide();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            webBrowser.IsScriptEnabled = true;
            webBrowser.Navigating += WebBrowser_Navigating;
            webBrowser.Navigated += WebBrowser_Navigated;
            webBrowser.LoadCompleted += WebBrowser_LoadCompleted;
            webBrowser.NavigationFailed += WebBrowser_NavigationFailed;
            Grid.SetRow(webBrowser, 1);
            this.LayoutRoot.Children.Add(webBrowser);
            if (previous != null)
            {
                this.URLTextBox.Text = previous.AbsoluteUri;
            }
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            webBrowser.Navigating -= WebBrowser_Navigating;
            webBrowser.Navigated -= WebBrowser_Navigated;
            webBrowser.LoadCompleted -= WebBrowser_LoadCompleted;
            webBrowser.NavigationFailed -= WebBrowser_NavigationFailed;
            this.LayoutRoot.Children.Remove(webBrowser);
        }

        private void WebBrowserNavigateTo(string url)
        {
            try
            {
                isBack = false;
                if (!string.IsNullOrEmpty(url))
                {
                    if (!url.Contains("."))
                    {
                        webBrowser.Navigate(new Uri(searchUri + Uri.EscapeUriString(url), UriKind.Absolute));
                    }
                    else
                    {
                        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                        {
                            url = ("http://" + url);
                        }
                        if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                        {
                            webBrowser.Navigate(new Uri(url, UriKind.RelativeOrAbsolute));
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void WebBrowser_Navigating(object sender, NavigatingEventArgs e)
        {
            this.ProgressBar.IsIndeterminate = true;
            this.ProgressBar.Visibility = System.Windows.Visibility.Visible;

            var type = this.MediaSupportService.GetMediaType(e.Uri.AbsoluteUri);
            if (type != MediaType.Unknown)
            {
                e.Cancel = true;
                var result = MessageBox.Show(e.Uri.AbsoluteUri, NewDownload.URLChoice, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    this.ProgressBar.IsIndeterminate = false;
                    ServiceLocator.Current.GetInstance<NewDownloadViewModel>().URL = e.Uri.AbsoluteUri;
                    ((NewDownloadViewModel)this.DataContext).URLValidation(e.Uri.AbsoluteUri, "/UI/Pages/AddDownloadPage.xaml");
                }
            }
        }

        private void WebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                if (isBack)
                {
                    if (history.Count > 0)
                    {
                        history.RemoveAt(history.Count - 1);
                    }
                    isBack = false;
                }
                else if (previous != null)
                {
                    if (history.Count > historySize)
                    {
                        history.RemoveAt(0);
                    }
                    if (e.Uri.AbsoluteUri != previous.AbsoluteUri)
                    {
                        history.Add(previous);
                    }
                }
                this.ProgressBar.IsIndeterminate = false;
                this.URLTextBox.Text = e.Uri.AbsoluteUri;
                previous = e.Uri;
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void WebBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            this.ProgressBar.IsIndeterminate = false;
            this.ProgressBar.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void WebBrowser_NavigationFailed(object sender, System.Windows.Navigation.NavigationFailedEventArgs e)
        {
            this.ProgressBar.IsIndeterminate = false;
            this.ProgressBar.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string url = ((TextBox)sender).Text;
                    this.WebBrowserNavigateTo(url);
                    ((TextBox)sender).IsEnabled = false;
                    ((TextBox)sender).IsEnabled = true;
                }
            }
            catch (Exception)
            {
                MessageBox.Show(NewDownload.URLInvalid, NewDownload.Error, MessageBoxButton.OK);
            }
        }

        private void ApplicationBarMenuItem_AddFavorite_Click(object sender, EventArgs e)
        {
            string url = this.URLTextBox.Text;
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = ("http://" + url);
            }

            Uri validatedUri;
            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out validatedUri))
            {
                MessageBox.Show(NewDownload.FavoriteInvalidURL, NewDownload.FavoriteAddition, MessageBoxButton.OK);
                return;
            }

            InputPrompt input = new InputPrompt()
            {
                Title = NewDownload.FavoriteAddition
            };
            input.Completed += (lsender, le) =>
            {
                if (!string.IsNullOrEmpty(le.Result))
                {
                    this.ApplicationSettingsService.AddFavorite(new Favorite()
                    {
                        Title = le.Result,
                        Url = url
                    });
                    MessageBox.Show(NewDownload.FavoriteAdditionSuccess, NewDownload.FavoriteAddition, MessageBoxButton.OK);
                    Messenger.Default.Send<Update, FavoritesViewModel>(null);
                }
                else
                {
                    MessageBox.Show(NewDownload.FavoriteNameRequired, NewDownload.FavoriteAddition, MessageBoxButton.OK);
                }
            };
            input.Show();
        }

        private void ApplicationBarMenuItem_Favorites_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/UI/Pages/FavoritesPage.xaml", UriKind.Relative));
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((NewDownloadViewModel)this.DataContext).StopURLValidation();
        }

        private void ApplicationBarIconButton_RefreshButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.WebBrowserNavigateTo(this.URLTextBox.Text);
            }
            catch (Exception)
            {
                MessageBox.Show(NewDownload.URLInvalid, NewDownload.Error, MessageBoxButton.OK);
            }
        }

        private void ApplicationBarIconButton_PreviousButton_Click(object sender, EventArgs e)
        {
            if (history.Count > 0)
            {
                webBrowser.Navigate(history[history.Count - 1]);
                isBack = true;
            }
        }

        private void URLTextBox_ActionIconTapped(object sender, EventArgs e)
        {
            this.URLTextBox.Text = string.Empty;
        }
    }
}
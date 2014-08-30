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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using SaveAndPlay.ViewModels;
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.Resources;
using Microsoft.Phone.Shell;
using SaveAndPlay.Helpers;
using SaveAndPlay.ViewModels.Messages;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Navigation;

namespace SaveAndPlay.UI.Pages
{
    public partial class URLSelectorDropboxPage : BasePage
    {
        private ESessionState sessionState;
        private bool step2InProgress = false;

        public URLSelectorDropboxPage()
        {
            InitializeComponent();
            Messenger.Default.Register<SessionState>(this, (sessionState) =>
            {
                switch (sessionState.State)
                {
                    case ESessionState.Signingin:
                        if (!string.IsNullOrEmpty(sessionState.SigningInURL))
                        {
                            this.SwitchToSigningInStep2State(sessionState.SigningInURL);
                        }
                        else
                        {
                            this.sessionState = ESessionState.Signingin;
                        }
                        break;
                    case ESessionState.Connected:
                        this.SwitchToSignedInState();
                        break;
                    case ESessionState.NotConnected:
                    case ESessionState.Unknown:
                        this.SwitchToSignedoutState();
                        break;
                    default:
                        break;
                }
            });

            this.baseProgressOverlay = this.progressOverlay;
            this.baseProgressBarLabel = this.progressBarLabel;
            if (((DropboxViewModel)this.DataContext).RunningSession)
            {
                SwitchToSignedInState();
                if (((DropboxViewModel)this.DataContext).ChangingDirectory)
                {
                    this.progressBarLabel.Text = NewDownload.LoadingLabel;
                    this.progressOverlay.Show();
                }
            }
            else
            {
                SwitchToSignedoutState();
            }
        }

        private void SwitchToSigningInStep2State(string signinUrl)
        {
            this.sessionState = ESessionState.Signingin;
            this.step2InProgress = true;
            this.webBrowser.Visibility = Visibility.Visible;
            this.webBrowser.Navigate(new Uri(signinUrl, UriKind.RelativeOrAbsolute));
        }

        private void SwitchToSignedInState()
        {
            this.sessionState = ESessionState.Connected;
            this.webBrowser.Visibility = Visibility.Collapsed;
            this.FileBrowserControl.Visibility = System.Windows.Visibility.Visible;
            this.SigninLabel.Visibility = System.Windows.Visibility.Collapsed;
            var buttons = (this.Resources["ApplicationBarResources"] as ApplicationBar).Buttons;
            this.ApplicationBar.Buttons.Clear();
            this.ApplicationBar.Buttons.Add(buttons[2]);
            this.ApplicationBar.Buttons.Add(buttons[1]);
        }

        private void SwitchToSignedoutState()
        {
            this.sessionState = ESessionState.NotConnected;
            this.webBrowser.Visibility = Visibility.Collapsed;
            this.FileBrowserControl.Visibility = System.Windows.Visibility.Collapsed;
            this.SigninLabel.Visibility = System.Windows.Visibility.Visible;
            var buttons = (this.Resources["ApplicationBarResources"] as ApplicationBar).Buttons;
            this.ApplicationBar.Buttons.Clear();
            this.ApplicationBar.Buttons.Add(buttons[0]);
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.sessionState == ESessionState.Signingin)
            {
                e.Cancel = true;
                ((DropboxViewModel)this.DataContext).Logout();
            }
            else
            {
                ServiceLocator.Current.GetInstance<NewDownloadViewModel>().StopURLValidation();
            }
        }

        private void ApplicationBarIconButton_SigninButton_Click(object sender, System.EventArgs e)
        {
            ((DropboxViewModel)this.DataContext).Login();
        }

        private void ApplicationBarIconButton_SignoutButton_Click(object sender, System.EventArgs e)
        {
            ((DropboxViewModel)this.DataContext).Logout();
        }

        private void ApplicationBarIconButton_PreviousButton_Click(object sender, System.EventArgs e)
        {
            if (this.progressOverlay.Visibility != System.Windows.Visibility.Visible)
            {
                ((DropboxViewModel)this.DataContext).Back();
            }
        }

        private void WebBrowser_Navigating(object sender, NavigatingEventArgs e)
        {
            this.progressOverlay.Hide();
            if (e.Uri.AbsoluteUri.StartsWith(DropboxViewModel.ValidUrl))
            {
                e.Cancel = true;
                webBrowser.Visibility = Visibility.Collapsed;
                ((DropboxViewModel)this.DataContext).LoginStep3();
            }
        }

        private void WebBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            this.step2InProgress = false;
        }

        private void WebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
        }

        private void webBrowser_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (this.step2InProgress)
            {
                MessageBox.Show(NewDownload.DropboxLoginFailed, NewDownload.Error, MessageBoxButton.OK);
                Messenger.Default.Send<ScreenLock, URLSelectorDropboxPage>(ScreenLock.Release());
                this.SwitchToSignedoutState();
            }
            this.step2InProgress = false;
        }
    }
}
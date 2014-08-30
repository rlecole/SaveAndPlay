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
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.Helpers;
using SaveAndPlay.Resources;
using SaveAndPlay.ViewModels;
using SaveAndPlay.ViewModels.Messages;

namespace SaveAndPlay.UI.Pages
{
    public partial class URLSelectorSkydrivePage : BasePage
    {
        public URLSelectorSkydrivePage()
        {
            InitializeComponent();

            Messenger.Default.Register<SessionState>(this, (sessionState) =>
            {
                switch (sessionState.State)
                {
                    case ESessionState.Connected:
                        this.SwitchToLoggedInState();
                        break;
                    case ESessionState.NotConnected:
                    case ESessionState.Unknown:
                        this.SwitchToLogoutState();
                        break;
                    default:
                        break;
                }
            });


            this.baseProgressOverlay = this.progressOverlay;
            this.baseProgressBarLabel = this.progressBarLabel;
            if (((SkydriveViewModel)this.DataContext).RunningSession)
            {
                SwitchToLoggedInState();
                if (((SkydriveViewModel)this.DataContext).ChangingDirectory)
                {
                    this.progressBarLabel.Text = NewDownload.LoadingLabel;
                    this.progressOverlay.Show();
                }
            }
            else
            {
                SwitchToLogoutState();
            }
        }

        private void SwitchToLoggedInState()
        {
            this.FileBrowserControl.Visibility = System.Windows.Visibility.Visible;
            this.SigninLabel.Visibility = System.Windows.Visibility.Collapsed;
            var buttons = (this.Resources["ApplicationBarResources"] as ApplicationBar).Buttons;
            this.ApplicationBar.Buttons.Clear();
            this.ApplicationBar.Buttons.Add(buttons[2]);
            this.ApplicationBar.Buttons.Add(buttons[1]);
        }

        private void SwitchToLogoutState()
        {
            this.FileBrowserControl.Visibility = System.Windows.Visibility.Collapsed;
            this.SigninLabel.Visibility = System.Windows.Visibility.Visible;
            var buttons = (this.Resources["ApplicationBarResources"] as ApplicationBar).Buttons;
            this.ApplicationBar.Buttons.Clear();
            this.ApplicationBar.Buttons.Add(buttons[0]);
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ServiceLocator.Current.GetInstance<NewDownloadViewModel>().StopURLValidation();
        }

        private void ApplicationBarIconButton_SigninButton_Click(object sender, System.EventArgs e)
        {
            ((SkydriveViewModel)this.DataContext).Login();
        }

        private void ApplicationBarIconButton_SignoutButton_Click(object sender, System.EventArgs e)
        {
            ((SkydriveViewModel)this.DataContext).Logout();
        }

        private void ApplicationBarIconButton_PreviousButton_Click(object sender, System.EventArgs e)
        {
            if (this.progressOverlay.Visibility != System.Windows.Visibility.Visible)
            {
                ((SkydriveViewModel)this.DataContext).Back();
            }
        }
    }
}
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
using Microsoft.Phone.Shell;
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.Helpers;
using SaveAndPlay.Resources;
using SaveAndPlay.Service;
using SaveAndPlay.ViewModels.Messages;
using SaveAndPlay.ViewService;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace SaveAndPlay.UI.Pages
{
    public class BasePage : PhoneApplicationPage
    {
        protected TextBlock baseProgressBarLabel;
        protected ProgressOverlay baseProgressOverlay;
        private bool cancelBackKeyPress;

        protected IPlaybackAgentService playbackAgentService;
        protected IPlaybackAgentService PlaybackAgentService
        {
            get
            {
                if (this.playbackAgentService == null)
                {
                    this.playbackAgentService = ServiceLocator.Current.GetInstance<IPlaybackAgentService>();
                }
                return this.playbackAgentService;
            }
        }

        protected IApplicationSettingsService applicationSettingsService;
        protected IApplicationSettingsService ApplicationSettingsService
        {
            get
            {
                if (this.applicationSettingsService == null)
                {
                    this.applicationSettingsService = ServiceLocator.Current.GetInstance<IApplicationSettingsService>();
                }
                return this.applicationSettingsService;
            }
        }

        protected INavigationViewService navigationViewService;
        protected INavigationViewService NavigationViewService
        {
            get
            {
                if (this.navigationViewService == null)
                {
                    this.navigationViewService = ServiceLocator.Current.GetInstance<INavigationViewService>();
                }
                return navigationViewService;
            }
        }

        protected IMediaSupportService mediaSupportService;
        protected IMediaSupportService MediaSupportService
        {
            get
            {
                if (this.mediaSupportService == null)
                {
                    this.mediaSupportService = ServiceLocator.Current.GetInstance<IMediaSupportService>();
                }
                return this.mediaSupportService;
            }
        }

        public BasePage()
        {
            Messenger.Default.Register<ScreenLock>(this, (o) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (o.IsActivated)
                    {
                        if (!string.IsNullOrEmpty(o.Message) &&
                            this.baseProgressBarLabel != null)
                        {
                            this.baseProgressBarLabel.Text = o.Message;
                        }
                        if (this.ApplicationBar != null)
                        {
                            this.ApplicationBar.IsVisible = !o.HideAppBar;
                        }
                        if (this.baseProgressOverlay != null)
                        {
                            this.baseProgressOverlay.Show();
                        }
                        this.cancelBackKeyPress = o.CancelBackKeyPress;
                    }
                    else
                    {
                        if (this.baseProgressOverlay != null)
                        {
                            this.baseProgressOverlay.Hide();
                        }
                        if (this.ApplicationBar != null)
                        {
                            this.ApplicationBar.IsVisible = true;
                        }
                        this.cancelBackKeyPress = false;
                    }
                });
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.LocalizeApplicationBar();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            e.Cancel = this.cancelBackKeyPress;
        }

        virtual protected void LocalizeApplicationBar()
        {
            if (this.ApplicationBar != null)
            {
                var buttons = this.ApplicationBar.Buttons;
                if (buttons != null)
                {
                    foreach (ApplicationBarIconButton button in buttons)
                    {
                        Localisation.ApplyLocalizationToApplicationBar(button);
                    }
                }
                if (this.Resources != null)
                {
                    foreach (var key in this.Resources.Keys)
                    {
                        if (((string)key).StartsWith("ApplicationBar"))
                        {
                            buttons = (this.Resources[key] as ApplicationBar).Buttons;
                            if (buttons != null)
                            {
                                foreach (ApplicationBarIconButton button in buttons)
                                {
                                    Localisation.ApplyLocalizationToApplicationBar(button);
                                }
                            }
                        }
                    }
                }
                var menuItems = this.ApplicationBar.MenuItems;
                if (menuItems != null)
                {
                    foreach (ApplicationBarMenuItem menuItem in menuItems)
                    {
                        Localisation.ApplyLocalizationToApplicationBar(menuItem);
                    }
                }
            }
        }
    }
}

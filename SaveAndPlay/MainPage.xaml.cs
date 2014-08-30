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
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.Helpers;
using SaveAndPlay.Resources;
using SaveAndPlay.ViewModels;
using SaveAndPlay.ViewModels.Messages;
using SaveAndPlay.UI.Pages;
using SaveAndPlay.Service;
using SaveAndPlay.Scheduler;
using SaveAndPlay.ViewService;
using System.ComponentModel;

namespace SaveAndPlay
{
    public partial class MainPage : BasePage
    {
        public MainPage()
        {
            InitializeComponent();
            this.baseProgressOverlay = this.progressOverlay;
            this.baseProgressBarLabel = this.progressBarLabel;
            UIHelper.DelayUIInvocation(3000, () =>
            {
                Messenger.Default.Send<Update, DownloadListViewModel>(new Update() { Scope = UpdateScope.All });
            });
        }

        private void Panorama_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string name = "ApplicationBar" + ((PanoramaItem)e.AddedItems[0]).Tag;
            this.LoadAppBar(name);
        }

        private void LoadAppBar(string name)
        {
            UIHelper.DelayUIInvocation(500, () =>
            {
                this.ApplicationBar.Buttons.Clear();
                if (this.Resources.Contains(name))
                {
                    this.ApplicationBar.IsVisible = true;
                    var buttons = (this.Resources[name] as ApplicationBar).Buttons;
                    foreach (ApplicationBarIconButton button in buttons)
                    {
                        this.ApplicationBar.Buttons.Add(button);
                    }
                    this.ApplicationBar.Mode = ApplicationBarMode.Default;
                }
                else
                {
                    this.ApplicationBar.Mode = ApplicationBarMode.Minimized;
                }
            });
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            App.DataCacheServiceInstance.Register((MainViewModel)this.DataContext);
            ((MainViewModel)this.DataContext).Load();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.DataCacheServiceInstance.Unregister((MainViewModel)this.DataContext);
            ((MainViewModel)this.DataContext).Unload();
        }

        private void ApplicationBarIconButton_Add_Click(object sender, EventArgs e)
        {
            this.NavigationViewService.NavigateTo("/UI/Pages/MediaSourcePage.xaml");
        }

        private void ApplicationBarIconButton_Clear_Click(object sender, EventArgs e)
        {
            ServiceLocator.Current.GetInstance<DownloadListViewModel>().Clear();
        }

        private void ApplicationBarMenuItem_Settings_Click(object sender, EventArgs e)
        {
            if (this.ApplicationSettingsService.IsDefinedPassword())
            {
                this.NavigationViewService.NavigateTo("/UI/Pages/ProtectedModePage.xaml");
            }
            else
            {
                this.NavigationViewService.NavigateTo("/UI/Pages/ProtectedModeNewPasswordPage.xaml");
            }
        }

        private void ApplicationBarMenuItem_About_Click(object sender, EventArgs e)
        {
            this.NavigationViewService.NavigateTo("/UI/Pages/AboutPage.xaml");
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, CancelEventArgs e)
        {
            if (ForegroundTransferService.AnyRequestActive)
            {
                if (MessageBox.Show(DownloadList.ActiveForegroundTransferLabel, Main.Warning, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
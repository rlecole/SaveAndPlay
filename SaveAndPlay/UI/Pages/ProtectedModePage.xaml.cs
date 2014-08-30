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
using SaveAndPlay.UI.Pages;
using SaveAndPlay.ViewModels;
using SaveAndPlay.Helpers;
using SaveAndPlay.ViewService;
using Microsoft.Practices.ServiceLocation;

namespace SaveAndPlay.UI.Pages
{
    public partial class ProtectedModePage : BasePage
    {
        public IDialogViewService DialogViewService
        {
            get { return ServiceLocator.Current.GetInstance<IDialogViewService>(); }
        }

        public ProtectedModePage()
        {
            InitializeComponent();
            base.baseProgressOverlay = this.progressOverlay;
            base.baseProgressBarLabel = this.baseProgressBarLabel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ((ProtectedModeViewModel)this.DataContext).Load();
            this.ProtectedModePasswordBox.Password = string.Empty;
            this.progressOverlay.Visibility = System.Windows.Visibility.Visible;
            App.DataCacheServiceInstance.Register((ProtectedModeViewModel)this.DataContext);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.DataCacheServiceInstance.Unregister((ProtectedModeViewModel)this.DataContext);
            this.progressOverlay.Visibility = System.Windows.Visibility.Visible;
        }

        private void ChangePassword_Button_Click(object sender, RoutedEventArgs e)
        {
            ((ProtectedModeViewModel)this.DataContext).Change();
        }

        private void ResetPassword_Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.DialogViewService.MessageBoxOKCancel(SaveAndPlay.Resources.Main.Warning, SaveAndPlay.Resources.ProtectedMode.ConfirmReset))
            {
                ((ProtectedModeViewModel)this.DataContext).Reset();
            }
        }

        private void Disable_Button_Click(object sender, RoutedEventArgs e)
        {
            ((ProtectedModeViewModel)this.DataContext).Disable();
        }

        private void ProtectedModePasswordBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ((ProtectedModeViewModel)this.DataContext).ValidatePassword(this.ProtectedModePasswordBox.Password);
        }
    }
}
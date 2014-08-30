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

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.Helpers;
using SaveAndPlay.Resources;
using SaveAndPlay.Service;
using SaveAndPlay.ViewService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SaveAndPlay.ViewModels
{
    public class ProtectedModeNewPasswordViewModel : BaseViewModel
    {
        private string password;
        public string Password
        {
            get { return password; }
            set { password = value; base.RaisePropertyChanged("Password", password, password, true); }
        }

        private string newPassword;
        public string NewPassword
        {
            get { return newPassword; }
            set { newPassword = value; base.RaisePropertyChanged("NewPassword", newPassword, newPassword, true); }
        }

        private string confirmationPassword;
        public string ConfirmationPassword
        {
            get { return confirmationPassword; }
            set { confirmationPassword = value; base.RaisePropertyChanged("ConfirmationPassword", confirmationPassword, confirmationPassword, true); }
        }

        private bool isDefinedPassword;
        public bool IsDefinedPassword
        {
            get { return isDefinedPassword; }
            set { isDefinedPassword = value; base.RaisePropertyChanged("IsDefinedPassword", isDefinedPassword, isDefinedPassword, true); }
        }

        public void Load()
        {
            this.IsDefinedPassword = this.ApplicationSettingsService.IsDefinedPassword();
            this.Reset();
        }

        public void Validate()
        {
            if ((!string.IsNullOrEmpty(this.ConfirmationPassword) && !string.IsNullOrEmpty(this.NewPassword) && this.ConfirmationPassword == this.NewPassword) &&
                (!this.IsDefinedPassword || (this.IsDefinedPassword && this.ApplicationSettingsService.ValidatePassword(this.Password))))
            {
                ApplicationSettingsService.SetPassword(this.NewPassword);
                if (!this.IsDefinedPassword)
                {
                    this.NavigationViewService.NavigateTo("/UI/Pages/ProtectedModePage.xaml", new NavigationParameter("protectedModeJustCreated", "true"));
                }
                else
                {
                    this.NavigationViewService.GoBack();
                }
            }
            else
            {
                this.DialogViewService.MessageBoxOk(Main.Error, ProtectedMode.PasswordConfirmationFailed);
                this.Reset();
            }
        }

        private void Reset()
        {
            this.Password = string.Empty;
            this.NewPassword = string.Empty;
            this.ConfirmationPassword = string.Empty;
        }
    }
}

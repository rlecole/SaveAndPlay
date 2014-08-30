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

using Microsoft.Phone.Controls;
using SaveAndPlay.Helpers;
using SaveAndPlay.UI.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace SaveAndPlay.UI.Behaviors
{
    public class TextBoxFocusBehavior : Behavior<TextBox>
    {
        public TextBoxFocusBehavior()
        {
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.GotFocus += AssociatedObject_GotFocus;
            this.AssociatedObject.LostFocus += AssociatedObject_LostFocus;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
            this.AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
        }

        private void AssociatedObject_GotFocus(object sender, RoutedEventArgs e)
        {
            var page = UIHelper.GetParent<BasePage>((DependencyObject)sender);
            if (page.ApplicationBar != null)
            {
                page.ApplicationBar.IsVisible = false;
            }
        }

        private void AssociatedObject_LostFocus(object sender, RoutedEventArgs e)
        {
            var page = UIHelper.GetParent<BasePage>((DependencyObject)sender);
            if (page.ApplicationBar != null)
            {
                page.ApplicationBar.IsVisible = true;
            }
        }
    }

    public class PasswordBoxFocusBehavior : Behavior<PasswordBox>
    {
        public PasswordBoxFocusBehavior()
        {
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.GotFocus += AssociatedObject_GotFocus;
            this.AssociatedObject.LostFocus += AssociatedObject_LostFocus;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
            this.AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
        }

        private void AssociatedObject_GotFocus(object sender, RoutedEventArgs e)
        {
            var page = UIHelper.GetParent<BasePage>((DependencyObject)sender);
            if (page.ApplicationBar != null)
            {
                page.ApplicationBar.IsVisible = false;
            }
        }

        private void AssociatedObject_LostFocus(object sender, RoutedEventArgs e)
        {
            var page = UIHelper.GetParent<BasePage>((DependencyObject)sender);
            if (page.ApplicationBar != null)
            {
                page.ApplicationBar.IsVisible = true;
            }
        }
    }

    public class PhoneTextBoxFocusBehavior : Behavior<PhoneTextBox>
    {
        public PhoneTextBoxFocusBehavior()
        {
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.GotFocus += AssociatedObject_GotFocus;
            this.AssociatedObject.LostFocus += AssociatedObject_LostFocus;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
            this.AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
        }

        private void AssociatedObject_GotFocus(object sender, RoutedEventArgs e)
        {
            var page = UIHelper.GetParent<BasePage>((DependencyObject)sender);
            if (page.ApplicationBar != null)
            {
                page.ApplicationBar.IsVisible = false;
            }
        }

        private void AssociatedObject_LostFocus(object sender, RoutedEventArgs e)
        {
            var page = UIHelper.GetParent<BasePage>((DependencyObject)sender);
            if (page.ApplicationBar != null)
            {
                page.ApplicationBar.IsVisible = true;
            }
        }
    }
}

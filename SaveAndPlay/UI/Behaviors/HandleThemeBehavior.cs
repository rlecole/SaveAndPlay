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
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SaveAndPlay.UI.Behaviors
{
    public class HandleThemeBehavior
    {
        // light
        public static string GetLightUrl(DependencyObject obj)
        {
            return (string)obj.GetValue(LightUrlProperty);
        }
        public static void SetLightUrl(DependencyObject obj, string value)
        {
            obj.SetValue(LightUrlProperty, value);
        }
        public static readonly DependencyProperty LightUrlProperty =
            DependencyProperty.RegisterAttached("LightUrl",
            typeof(string), typeof(HandleThemeBehavior), new PropertyMetadata(Setup));

        // dark
        public static string GetDarkUrl(DependencyObject obj)
        {
            return (string)obj.GetValue(DarkUrlProperty);
        }
        public static void SetDarkUrl(DependencyObject obj, string value)
        {
            obj.SetValue(DarkUrlProperty, value);
        }
        public static readonly DependencyProperty DarkUrlProperty =
            DependencyProperty.RegisterAttached("DarkUrl",
            typeof(string), typeof(HandleThemeBehavior), new PropertyMetadata(Setup));

        private static void Setup(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(obj))
                return;

            var _Image = obj as Image;
            if (_Image == null)
                throw new Exception("Attach only to an Image");

            var _Dark = GetDarkUrl(_Image);
            var _Light = GetLightUrl(_Image);
            if (string.IsNullOrWhiteSpace(_Dark)
                || string.IsNullOrWhiteSpace(_Light))
                return;

            _Image.Loaded += new RoutedEventHandler(_Image_Loaded);
        }

        static void _Image_Loaded(object sender, RoutedEventArgs e)
        {
            var _Image = sender as Image;
            var _Dark = GetDarkUrl(_Image);
            var _Light = GetLightUrl(_Image);

            // what if the user didn't set this up right?
            if (string.IsNullOrWhiteSpace(_Dark)
                || string.IsNullOrWhiteSpace(_Light))
                throw new Exception("Dark & White are required");

            string _Url = string.Empty;
            switch (CurrentTheme(_Image))
            {
                case Themes.Dark:
                    _Url = _Dark;
                    break;
                case Themes.Light:
                    _Url = _Light;
                    break;
                default:
                    throw new Exception("Theme not supported");
            }
            var _Uri = new Uri(_Url, UriKind.Relative);
            var _Bitmap = new BitmapImage(_Uri);
            (sender as Image).Source = _Bitmap;
        }

        enum Themes { Dark, Light }

        static Themes CurrentTheme(Image image)
        {
            var _Resource = image.Resources["PhoneDarkThemeVisibility"];
            var _Visibility = (Visibility)_Resource;
            if (_Visibility == Visibility.Visible)
                return Themes.Dark;
            return Themes.Light;
        }
    }
}

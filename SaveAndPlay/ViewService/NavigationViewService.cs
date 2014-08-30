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
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SaveAndPlay.ViewService
{
    public class NavigationParameter
    {
        public NavigationParameter(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class NavigationViewService : INavigationViewService
    {
        private PhoneApplicationFrame _mainFrame;

        public event NavigatingCancelEventHandler Navigating;

        public string CurrentSourceString
        { 
            get
            {
                if (this.EnsureMainFrame())
                {
                    return _mainFrame.CurrentSource.OriginalString;
                }
                return string.Empty;
            }
        }

        public void NavigateTo(string uri, params NavigationParameter[] parameters)
        {
            if (EnsureMainFrame())
            {
                StringBuilder parameterStr = new StringBuilder();;
                if (parameters.Length > 0)
                {
                    parameterStr.Append("?");
                    foreach (var parameter in parameters)
                    {
                        if (parameterStr.Length > 1)
                        {
                            parameterStr.Append(";");
                        }
                        parameterStr.Append(parameter.Name);
                        parameterStr.Append("=");
                        parameterStr.Append(parameter.Value);
                    }
                }
                _mainFrame.Navigate(new Uri(uri + parameterStr, UriKind.Relative));
            }
        }

        public NavigationParameter[] GetParametersFromUri()
        {
            var uri = this.CurrentSourceString;
            NavigationParameter[] parameters = null;
            int index = uri.IndexOf('?');
            if (index > 0 && index + 1 < uri.Length - 1)
            {
                var parametersStr = uri.Substring(index + 1);
                var parametersList = parametersStr.Split(';');
                parameters = new NavigationParameter[parametersList.Length];
                int i = 0;
                foreach (var parameter in parametersList)
                {
                    string[] nameValue = parameter.Split('=');
                    parameters[i] = new NavigationParameter(nameValue[0], nameValue[1]);
                    i++;
                }
            }
            return parameters;
        }

        public void GoBack()
        {
            if (EnsureMainFrame() && _mainFrame.CanGoBack)
            {
                _mainFrame.GoBack();
            }
        }

        public void RemoveBackEntry()
        {
            if (EnsureMainFrame() && _mainFrame.BackStack.Count() >= 1)
            {
                _mainFrame.RemoveBackEntry();
            }
        }

        private bool EnsureMainFrame()
        {
            if (_mainFrame != null)
            {
                return true;
            }

            _mainFrame = Application.Current.RootVisual as PhoneApplicationFrame;

            if (_mainFrame != null)
            {
                // Could be null if the app runs inside a design tool
                _mainFrame.Navigating += (s, e) =>
                {
                    if (Navigating != null)
                    {
                        Navigating(s, e);
                    }
                };

                return true;
            }

            return false;
        }
    }
}

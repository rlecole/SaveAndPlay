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
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.Service;
using SaveAndPlay.ViewService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaveAndPlay.ViewModels
{
    public class BaseViewModel : ViewModelBase
    {
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

        protected IDialogViewService dialogViewService;
        protected IDialogViewService DialogViewService
        {
            get
            {
                if (this.dialogViewService == null)
                {
                    this.dialogViewService = ServiceLocator.Current.GetInstance<IDialogViewService>();
                }
                return this.dialogViewService;
            }
        }

        protected IDataLoadingScopeViewService dataLoadingScopeViewService;
        protected IDataLoadingScopeViewService DataLoadingScopeViewService
        {
            get
            {
                if (this.dataLoadingScopeViewService == null)
                {
                    this.dataLoadingScopeViewService = ServiceLocator.Current.GetInstance<IDataLoadingScopeViewService>();
                }
                return this.dataLoadingScopeViewService;
            }
        }

        private INavigationViewService navigationViewService;
        public INavigationViewService NavigationViewService
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

        private IPlaybackAgentService playbackAgentService;
        public IPlaybackAgentService PlaybackAgentService
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
    }
}

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

using GalaSoft.MvvmLight.Command;
using Microsoft.Live;
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.Converters;
using SaveAndPlay.Helpers;
using SaveAndPlay.Resources;
using SaveAndPlay.Service;
using SaveAndPlay.UI.Controls.FileBrowserControlEntities;
using SaveAndPlay.UI.Pages;
using SaveAndPlay.ViewModels.Messages;
using SaveAndPlay.ViewService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;

namespace SaveAndPlay.ViewModels
{
    public class SkydriveViewModel : BaseFileBrowserViewModel
    {
        private static readonly string[] scopes = new string[]
        { 
            "wl.signin",
            //"wl.basic",
            "wl.skydrive",
            "wl.photos"
        };

        private LiveConnectClient liveConnectClient;
        private LiveAuthClient liveAuthClient;

        public override bool RunningSession
        {
            get
            {
                return this.liveConnectClient != null;
            }
        }

        public SkydriveViewModel()
        {
            this.SelectVideo = new RelayCommand<VideoFile>((file) =>
            {
                if (this.DialogViewService.MessageBoxOKCancel(NewDownload.URLChoice, file.Title))
                {
                    base.MessengerInstance.Send<ScreenLock, URLSelectorSkydrivePage>(ScreenLock.Simple(NewDownload.URLValidation));
                    ServiceLocator.Current.GetInstance<NewDownloadViewModel>().URL = file.URL;
                    ServiceLocator.Current.GetInstance<NewDownloadViewModel>().URLValidation(file.URL, "/UI/Pages/AddDownloadPage.xaml");
                }
            });

            this.SelectAudio = new RelayCommand<AudioFile>((file) =>
            {
                if (this.DialogViewService.MessageBoxOKCancel(NewDownload.URLChoice, file.Title))
                {
                    base.MessengerInstance.Send<ScreenLock, URLSelectorSkydrivePage>(ScreenLock.Simple(NewDownload.URLValidation));
                    ServiceLocator.Current.GetInstance<NewDownloadViewModel>().URL = file.URL;
                    ServiceLocator.Current.GetInstance<NewDownloadViewModel>().URLValidation(file.URL, "/UI/Pages/AddDownloadPage.xaml");
                }
            });

            this.SelectFolder = new RelayCommand<Folder>((folder) =>
            {
                try
                {
                    base.MessengerInstance.Send<ScreenLock, URLSelectorSkydrivePage>(ScreenLock.Simple(NewDownload.LoadingLabel));
                    this.targetFolder = folder;
                    if (this.liveConnectClient != null)
                    {
                        this.liveConnectClient.GetAsync("/" + folder.Id + "/files");
                    }
                }
                catch (Exception)
                {
                    this.Logout();
                }
            });
        }

        public void Login()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                this.DialogViewService.MessageBoxOk(NewDownload.Error, NewDownload.SkydriveLoginFailed);
                return;
            }
            base.MessengerInstance.Send<ScreenLock, URLSelectorSkydrivePage>(ScreenLock.Backable(NewDownload.LoadingLabel));
            this.liveAuthClient = new LiveAuthClient("00000000440E1B33");
            this.liveAuthClient.InitializeCompleted += OnAuthClientInitializeCompleted;
            this.liveAuthClient.InitializeAsync(scopes);
        }

        public void Logout()
        {
            if (this.liveAuthClient != null)
            {
                this.liveAuthClient.Logout();
                this.liveAuthClient = null;
                this.liveConnectClient = null;
            }
            base.MessengerInstance.Send<ScreenLock, URLSelectorSkydrivePage>(ScreenLock.Release());
            base.MessengerInstance.Send<SessionState, URLSelectorSkydrivePage>(new SessionState() { State = ESessionState.NotConnected });
        }

        public void Back()
        {
            if (this.history.Count > 0)
            {
                base.MessengerInstance.Send<ScreenLock, URLSelectorSkydrivePage>(ScreenLock.Simple(NewDownload.LoadingLabel));
                this.isBack = true;
                this.targetFolder = this.history.Peek();
                string path = this.targetFolder == null ? "me/skydrive/files" : "/" + this.targetFolder.Id + "/files";
                this.liveConnectClient.GetAsync(path);
            }
        }

        private void OnAuthClientInitializeCompleted(object sender, LoginCompletedEventArgs e)
        {
            if (e.Status == LiveConnectSessionStatus.Connected)
            {
                this.liveConnectClient = new LiveConnectClient(e.Session);
                base.MessengerInstance.Send<SessionState, URLSelectorSkydrivePage>(new SessionState() { State = SessionStateConverter.Convert(e.Status) });
            }
            else
            {
                this.liveAuthClient.LoginCompleted += OnAuthClientLoginCompleted;
                this.liveAuthClient.LoginAsync(scopes);
            }
        }

        private void OnAuthClientLoginCompleted(object sender, LoginCompletedEventArgs e)
        {
            if (e.Status == LiveConnectSessionStatus.Connected)
            {
                this.liveConnectClient = new LiveConnectClient(e.Session);
                this.targetFolder = null;
                this.currentFolder = null;
                this.history.Clear();
                liveConnectClient.GetAsync("me/skydrive/files");
                liveConnectClient.GetCompleted += this.OnGetCompleted;
            }
            else
            {
                base.MessengerInstance.Send<ScreenLock, URLSelectorSkydrivePage>(ScreenLock.Release());
            }
            base.MessengerInstance.Send<SessionState, URLSelectorSkydrivePage>(new SessionState() { State = SessionStateConverter.Convert(e.Status) });
        }

        private void OnGetCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                base.HandleHistory();

                this.Root = this.targetFolder == null ? NewDownload.RootDirectoryLabel : this.targetFolder.Title;
                this.Data = new ObservableCollection<BaseFile>();
                List<object> data = (List<object>)e.Result["data"];
                foreach (IDictionary<string, object> content in data)
                {
                    string name = (string)content["name"];
                    string type = (string)content["type"];
                    var mediaType = this.MediaSupportService.GetMediaType(name);
                    switch (mediaType)
                    {
                        case MediaType.Unknown:
                            if (type == "folder" || type == "album")
                            {
                                this.Data.Add(new Folder()
                                {
                                    Title = name,
                                    FilesCount = (int)content["count"],
                                    URL = (string)content["link"],
                                    Id = (string)content["id"]
                                });
                            }
                            break;
                        case MediaType.Video:
                            this.Data.Add(new VideoFile()
                            {
                                Title = name,
                                URL = (string)content["source"],
                                Size = FileSizeConverter.Convert((int)content["size"])
                            });
                            break;
                        case MediaType.Audio:
                            this.Data.Add(new AudioFile()
                            {
                                Title = name,
                                URL = (string)content["source"],
                                Size = FileSizeConverter.Convert((int)content["size"])
                            });
                            break;
                        default:
                            break;
                    }
                }
                this.Data = new ObservableCollection<BaseFile>(this.Data.OrderBy(d => d, new FileTypeComparer()).ThenBy(d => d.Title));
                this.EmptyFolder = !this.Data.Any();
            }
            else
            {
                this.DialogViewService.MessageBoxOk(NewDownload.Error, NewDownload.BrowseFolderFailedLabel);
            }
            base.MessengerInstance.Send<ScreenLock, URLSelectorSkydrivePage>(ScreenLock.Release());
        }
    }
}

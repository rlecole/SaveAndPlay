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

using DropNet;
using DropNet.Authenticators;
using DropNet.Exceptions;
using DropNet.Models;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.Converters;
using SaveAndPlay.Resources;
using SaveAndPlay.Service;
using SaveAndPlay.UI.Controls.FileBrowserControlEntities;
using SaveAndPlay.UI.Pages;
using SaveAndPlay.ViewModels.Messages;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;

namespace SaveAndPlay.ViewModels
{
    public class DropboxViewModel : BaseFileBrowserViewModel
    {
        public const string ValidUrl = "http://www.valid.test/";

        private DropNetClient dropNetClient;

        public override bool RunningSession
        {
            get
            {
                return this.dropNetClient != null;
            }
        }

        public DropboxViewModel()
        {
            this.SelectVideo = new RelayCommand<VideoFile>((file) =>
            {
                if (this.DialogViewService.MessageBoxOKCancel(NewDownload.URLChoice, file.Title))
                {
                    ServiceLocator.Current.GetInstance<NewDownloadViewModel>().URLValidation(this.BuildUrlFromPath(file.URL), "/UI/Pages/AddDownloadPage.xaml");
                    ServiceLocator.Current.GetInstance<NewDownloadViewModel>().URL = this.BuildUrlFromPath(file.URL);
                }
            });

            this.SelectAudio = new RelayCommand<AudioFile>((file) =>
            {
                if (this.DialogViewService.MessageBoxOKCancel(NewDownload.URLChoice, file.Title))
                {
                    string urlToValidate = this.BuildUrlFromPath(file.URL);
                    ServiceLocator.Current.GetInstance<NewDownloadViewModel>().URL = this.BuildUrlFromPath(file.URL);
                    ServiceLocator.Current.GetInstance<NewDownloadViewModel>().URLValidation(urlToValidate, "/UI/Pages/AddDownloadPage.xaml");
                }
            });

            this.SelectFolder = new RelayCommand<Folder>((folder) =>
            {
                base.MessengerInstance.Send<ScreenLock, URLSelectorDropboxPage>(ScreenLock.Simple(NewDownload.LoadingLabel));
                this.targetFolder = folder;
                try
                {
                    this.dropNetClient.GetMetaDataAsync(folder.URL, OnMetaDataCompleted, OnMetaDaError);
                }
                catch (Exception)
                {
                    OnMetaDaError(null);
                }
            });
        }

        public void Login()
        {
            base.MessengerInstance.Send<SessionState, URLSelectorDropboxPage>(new SessionState() { State = ESessionState.Signingin });
            base.MessengerInstance.Send<ScreenLock, URLSelectorDropboxPage>(ScreenLock.Backable(NewDownload.LoadingLabel));
            this.dropNetClient = new DropNetClient("tdjfq9pdclgngyt", "k6okn03xb6v5lza");
            this.dropNetClient.GetTokenAsync((userLogin) =>
            {
                try
                {
                    if (this.dropNetClient != null && userLogin != null)
                    {
                        var url = this.dropNetClient.BuildAuthorizeUrl(DropboxViewModel.ValidUrl);
                        base.MessengerInstance.Send<SessionState, URLSelectorDropboxPage>(new SessionState() { State = ESessionState.Signingin, SigningInURL = url });
                    }
                }
                catch (Exception)
                {
                    this.DialogViewService.MessageBoxOk(NewDownload.Error, NewDownload.DropboxLoginFailed);
                    this.Logout();
                }
            },
            OnLoginError);
        }

        public void LoginStep3()
        {
            base.MessengerInstance.Send<ScreenLock, URLSelectorDropboxPage>(ScreenLock.Backable(NewDownload.LoadingLabel));
            this.dropNetClient.GetAccessTokenAsync((accessToken) =>
            {
                try
                {
                    if (this.dropNetClient != null && accessToken != null)
                    {
                        //Store this token for "remember me" function
                        base.MessengerInstance.Send<SessionState, URLSelectorDropboxPage>(new SessionState() { State = ESessionState.Connected });
                        if (this.dropNetClient != null)
                        {
                            this.dropNetClient.GetMetaDataAsync("/", OnMetaDataCompleted, OnMetaDaError);
                        }
                    }
                }
                catch (Exception)
                {
                    this.DialogViewService.MessageBoxOk(NewDownload.Error, NewDownload.DropboxLoginFailed);
                    this.Logout();
                }
            },
            OnLoginError);
        }

        public void Logout()
        {
            if (this.dropNetClient != null)
            {
                this.dropNetClient = null;
            }
            this.Data = new ObservableCollection<BaseFile>();
            base.MessengerInstance.Send<ScreenLock, URLSelectorDropboxPage>(ScreenLock.Release());
            base.MessengerInstance.Send<SessionState, URLSelectorDropboxPage>(new SessionState() { State = ESessionState.NotConnected });
        }

        public void Back()
        {
            if (this.history.Count > 0)
            {
                base.MessengerInstance.Send<ScreenLock, URLSelectorDropboxPage>(ScreenLock.Simple(NewDownload.LoadingLabel));
                this.isBack = true;
                this.targetFolder = this.history.Peek();
                string path = this.targetFolder == null ? "/" : this.targetFolder.URL;
                this.dropNetClient.GetMetaDataAsync(path, OnMetaDataCompleted, OnMetaDaError);
            }
        }

        private void OnMetaDataCompleted(MetaData metaData)
        {
            if (metaData != null)
            {
                this.HandleHistory();

                this.Root = this.targetFolder == null ? NewDownload.RootDirectoryLabel : this.targetFolder.Title;
                this.Data = new ObservableCollection<BaseFile>();
                foreach (var item in metaData.Contents)
                {
                    if (item != null)
                    {
                        var mediaType = this.MediaSupportService.GetMediaType(item.Name);
                        switch (mediaType)
                        {
                            case MediaType.Unknown:
                                if (item.Is_Dir)
                                {
                                    this.Data.Add(new Folder()
                                    {
                                        Title = item.Name,
                                        URL = item.Path,
                                    });
                                }
                                break;
                            case MediaType.Video:
                                this.Data.Add(new VideoFile()
                                {
                                    Title = item.Name,
                                    URL = item.Path,
                                    Size = FileSizeConverter.Convert(item.Bytes)
                                });
                                break;
                            case MediaType.Audio:
                                this.Data.Add(new AudioFile()
                                {
                                    Title = item.Name,
                                    URL = item.Path,
                                    Size = FileSizeConverter.Convert(item.Bytes)
                                });
                                break;
                            default:
                                break;
                        }
                    }
                }
                this.Data = new ObservableCollection<BaseFile>(this.Data.OrderBy(d => d, new FileTypeComparer()).ThenBy(d => d.Title));
                this.EmptyFolder = !this.Data.Any();
            }
            base.MessengerInstance.Send<ScreenLock, URLSelectorDropboxPage>(ScreenLock.Release());
        }

        private void OnLoginError(DropboxException error)
        {
            this.DialogViewService.MessageBoxOkInUIThead(NewDownload.Error, NewDownload.DropboxLoginFailed);
            this.Logout();
        }

        private void OnMetaDaError(DropboxException error)
        {
            this.DialogViewService.MessageBoxOkInUIThead(NewDownload.Error, NewDownload.BrowseFolderFailedLabel);
            this.Logout();
        }

        private string BuildUrlFromPath(string path)
        {
            string result = "https://api-content.dropbox.com/1/files/dropbox" + Uri.EscapeUriString(path) + "?";

            var authenticator = new OAuthAuthenticator(result,
                                                       "tdjfq9pdclgngyt",
                                                       "k6okn03xb6v5lza",
                                                       this.dropNetClient.UserLogin.Token,
                                                       this.dropNetClient.UserLogin.Secret);
            var authenticatorRequest = new RestSharp.RestRequest();
            authenticator.Authenticate(null, authenticatorRequest);

            bool first = true;
            foreach (var parameter in authenticatorRequest.Parameters)
            {
                if (!first)
                {
                    result += "&";
                }
                else
                {
                    first = false;
                }
                result += parameter.Name.ToString() + "=" + HttpUtility.UrlEncode(parameter.Value.ToString());
            }
            return result;
        }
    }
}

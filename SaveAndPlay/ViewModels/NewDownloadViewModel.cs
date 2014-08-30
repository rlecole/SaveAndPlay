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

using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.BackgroundTransfer;
using Microsoft.Phone.Info;
using Microsoft.Phone.Net.NetworkInformation;
using SaveAndPlay.Helpers;
using SaveAndPlay.Resources;
using SaveAndPlay.Service;
using SaveAndPlay.UI.Pages;
using SaveAndPlay.ViewModels.Messages;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;

namespace SaveAndPlay.ViewModels
{
    public class NewDownloadViewModel : BaseViewModel
    {
        public class TransferRequirement
        {
            public string IsRequired { get; set; }
            public string Title { get; set; }
            public string IconLight { get; set; }
            public string IconDark { get; set; }
        }

        public class NewDownloadViewModelState
        {
            public string URL { get; set; }
            public string Title { get; set; }
            public string Extension { get; set; }
            public long ContentLength { get; set; }
            public bool TransferMode { get; set; }
        }

        private class ValidationThreadState
        {
            public bool IsAlive { get; set; }
        }

        private TransferRequirement ConnectionRequired = new TransferRequirement() { Title = "ConnectionMandatoryLabel", IconLight = "/Images/light/signal.png", IconDark = "/Images/dark/signal.png" };
        private TransferRequirement Cellular3GRequired = new TransferRequirement() { Title = "Cellular3GMandatoryLabel", IconLight = "/Images/light/signal.png", IconDark = "/Images/dark/signal.png" };
        private TransferRequirement WifiRequired = new TransferRequirement() { Title = "WifiMandatoryLabel", IconLight = "/Images/light/wifi.png", IconDark = "/Images/dark/wifi.png" };
        private TransferRequirement PowerRequired = new TransferRequirement() { Title = "ExternalPowerMandatoryLabel", IconLight = "/Images/light/power.png", IconDark = "/Images/dark/power.png" };
        private TransferRequirement ActiveAppRequired = new TransferRequirement() { Title = "ActiveAppMandatoryLabel", IconLight = "/Images/light/active.png", IconDark = "/Images/dark/active.png" };

        private const int MaxDownloads = 3;
        private const int CellularThreshold = 20971520;
        private const int ExternalPowerThreshold = 104857600;

        private object validationThreadsSyncRoot = new object();
        private List<ValidationThreadState> validationThreads = new List<ValidationThreadState>();

        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged("Title", title, value, true); }
        }

        private string url;/* = "http://media.ch9.ms/ch9/b480/5f6b86d9-d370-4b69-8d9f-1c2bded4b480/2-005.wmv";*/
        public string URL
        {
            get { return url; }
            set { url = value; base.RaisePropertyChanged("URL", url, value, true); }
        }

        private IList<TransferRequirement> requirements;
        public IList<TransferRequirement> Requirements
        {
            get { return requirements; }
            set { requirements = value; RaisePropertyChanged("Requirements"); }
        }

        private long contentLength;
        public long ContentLength
        {
            get { return contentLength; }
            set { contentLength = value; base.RaisePropertyChanged("ContentLength"); }
        }

        private bool transferMode;
        public bool TransferMode
        {
            get { return transferMode; }
            set { transferMode = value; base.RaisePropertyChanged("TransferMode", transferMode, transferMode, true); }
        }

        public string Extension { get; set; }

        public NewDownloadViewModel()
        {
            this.ResumeState();

            base.MessengerInstance.Register<PropertyChangedMessage<bool>>(this, (action) =>
            {
                if (action.PropertyName == "TransferMode")
                {
                    this.Requirements = this.EstablishRequirements(this.ContentLength);
                }
            });
        }

        public void AutoCompleteTitle()
        {
            this.SetInformationURL(this.URL);
        }

        public void Reset()
        {
            this.ResetCurrentState();
            this.URL = string.Empty;
            this.Title = string.Empty;
            this.Extension = string.Empty;
            this.ContentLength = 0;
            this.Requirements = new List<TransferRequirement>();
        }

        public void URLValidation(string url, string navigateToOnSuccess)
        {
            this.TransferMode = false;
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                this.DialogViewService.MessageBoxOk(NewDownload.DownloadRequirementsFailed, NewDownload.ConnectionRequiredLabel);
                return;
            }

            base.MessengerInstance.Send(ScreenLock.Backable(NewDownload.URLValidation));
            var state = new ValidationThreadState() { IsAlive = true };
            lock (this.validationThreadsSyncRoot)
            {
                this.validationThreads.Add(state);
            }
            ThreadPool.QueueUserWorkItem((s) =>
            {
                if (((ValidationThreadState)s).IsAlive)
                {
                    bool result = this.CheckAndUpdateURLInformation(url);
                    if (((ValidationThreadState)s).IsAlive)
                    {
                        if (result)
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                this.SetInformationURL(url);
                                base.MessengerInstance.Send<ScreenLock>(new ScreenLock() { IsActivated = false });
                                this.NavigationViewService.NavigateTo(navigateToOnSuccess);
                            });
                        }
                        else
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                this.DialogViewService.MessageBoxOk(NewDownload.DownloadRequirementsFailed, NewDownload.URLKO);
                                base.MessengerInstance.Send<ScreenLock>(ScreenLock.Release());
                            });
                        }
                    }
                    lock (this.validationThreadsSyncRoot)
                    {
                        this.validationThreads.Remove((ValidationThreadState)s);
                    }
                }
            }, state);
        }

        public void StopURLValidation()
        {
            lock (this.validationThreadsSyncRoot)
            {
                if (this.validationThreads.Count > 0)
                {
                    this.validationThreads.Last().IsAlive = false;
                }
            }
        }

        public void Download()
        {
            base.MessengerInstance.Send<ScreenLock, AddDownloadPage>(ScreenLock.Strict(Main.Loading));
            ThreadPool.QueueUserWorkItem((o) =>
            {
                try
                {
                    if (this.CheckDownloadRequirements())
                    {
                        if (!this.TransferMode)
                        {
                            this.DownloadBackground();
                        }
                        else
                        {
                            this.DownloadForeground();
                        }
                    }
                }
                catch (Exception)
                {
                    this.DialogViewService.MessageBoxOkInUIThead(NewDownload.DownloadRequirementsFailed, NewDownload.TransferAdditionFailed);
                }
                finally
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        base.MessengerInstance.Send<ScreenLock, AddDownloadPage>(ScreenLock.Release());
                    });
                }
            });
        }

        private void DownloadBackground()
        {
            IsolatedStorageFileHelper.CreateDirectoryIfNotExists("/shared/transfers");

            string downloadFile = this.Title + "." + this.Extension;
            BackgroundTransferRequest transferRequest = new BackgroundTransferRequest(new Uri(this.URL, UriKind.Absolute));
            transferRequest.Method = "GET";
            transferRequest.DownloadLocation = new Uri("shared/transfers/" + downloadFile, UriKind.RelativeOrAbsolute);
            transferRequest.Tag = downloadFile;
            transferRequest.TransferPreferences = TransferPreferences.AllowCellularAndBattery;

            if (this.ContentLength > CellularThreshold &&
                this.ContentLength <= ExternalPowerThreshold)
            {
                transferRequest.TransferPreferences = TransferPreferences.AllowBattery;
            }
            else if (this.ContentLength > ExternalPowerThreshold)
            {
                transferRequest.TransferPreferences = TransferPreferences.None;
            }

            if (!IsolatedStorageFileHelper.CheckIfFileAlreadyDownloading(downloadFile))
            {
                BackgroundTransferService.Add(transferRequest);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    base.MessengerInstance.Send<Update, DownloadListViewModel>(new Update() { Scope = UpdateScope.Background });
                    this.DialogViewService.MessageBoxOk(Main.Info, NewDownload.NewDownloadStarted);
                    this.NavigationViewService.GoBack();
                });
            }
            else
            {
                this.DialogViewService.MessageBoxOkInUIThead(NewDownload.DownloadRequirementsFailed, NewDownload.DownloadSameName);
            }
        }

        private void DownloadForeground()
        {
            IsolatedStorageFileHelper.CreateDirectoryIfNotExists("/shared/mtransfers");
            ForegroundTransferRequest transferRequest = new ForegroundTransferRequest(new Uri(this.URL, UriKind.Absolute));
            string downloadFile = this.Title + "." + this.Extension;
            transferRequest.DownloadLocation = "shared/mtransfers/" + downloadFile;
            transferRequest.Tag = downloadFile;

            if (!IsolatedStorageFileHelper.CheckIfFileAlreadyDownloading(downloadFile))
            {
                ForegroundTransferService.Add(transferRequest);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    base.MessengerInstance.Send<Update, DownloadListViewModel>(new Update() { Scope = UpdateScope.Foreground });
                    this.DialogViewService.MessageBoxOk(Main.Info, NewDownload.NewDownloadStarted);
                    this.NavigationViewService.GoBack();
                });
            }
            else
            {
                this.DialogViewService.MessageBoxOkInUIThead(NewDownload.DownloadRequirementsFailed, NewDownload.DownloadSameName);
            }
        }

        private bool CheckDownloadRequirements()
        {
            if (string.IsNullOrEmpty(this.Title))
            {
                this.DialogViewService.MessageBoxOkInUIThead(NewDownload.DownloadRequirementsFailed, NewDownload.NoTitleSpecified);
                return false;
            }

            if (BackgroundTransferService.Requests.Count() >= MaxDownloads)
            {
                this.DialogViewService.MessageBoxOkInUIThead(NewDownload.DownloadRequirementsFailed, NewDownload.MaximumDownloadReached);
                return false;
            }

            if (this.Requirements.Any(r => r == ConnectionRequired) &&
                !NetworkInterface.GetIsNetworkAvailable())
            {
                this.DialogViewService.MessageBoxOkInUIThead(NewDownload.DownloadRequirementsFailed, NewDownload.ConnectionMandatoryLabelLong);
                return false;
            }

            var networkInterface = NetworkInterface.NetworkInterfaceType;
            //if (this.Requirements.Any(r => r == Cellular3GRequired) &&
            //    networkInterface != NetworkInterfaceType.MobileBroadbandCdma)
            //{
            //    UIHelper.MessageBoxFromVM(NewDownload.DownloadRequirementsFailed, NewDownload.Cellular3GMandatoryLabelLong);
            //    return false;
            //}

            if (this.Requirements.Any(r => r == WifiRequired) &&
                networkInterface != NetworkInterfaceType.Wireless80211)
            {
                this.DialogViewService.MessageBoxOkInUIThead(NewDownload.DownloadRequirementsFailed, NewDownload.WifiMandatoryLabelLong);
                return false;
            }

            if (this.Requirements.Any(r => r == PowerRequired) &&
                DeviceStatus.PowerSource != PowerSource.External)
            {
                this.DialogViewService.MessageBoxOkInUIThead(NewDownload.DownloadRequirementsFailed, NewDownload.ExternalPowerMandatoryLabelLong);
                return false;
            }

            return true;
        }

        private void SetInformationURL(string url)
        {
            this.Extension = string.Empty;

            if (!string.IsNullOrEmpty(url))
            {
                url = HttpUtility.UrlDecode(url);
                int index = url.LastIndexOf('?');
                if (index > 0)
                {
                    url = url.Substring(0, index);
                }

                int begin = url.LastIndexOf("\\");
                begin = begin < 0 ? url.LastIndexOf("/") : begin;
                int end = url.LastIndexOf(".");
                if (begin >= 0)
                {
                    begin++;
                    end = end < begin ? url.Length : end++;
                    int length = end - begin;
                    this.Title = url.Substring(begin, (length <= 0) ? 0 : length);
                    if (end + 1 < url.Length)
                    {
                        this.Extension = url.Substring(end + 1);
                    }
                }
            }
        }

        private bool CheckAndUpdateURLInformation(string url)
        {
            try
            {
                Uri validatedUri;
                if (!string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out validatedUri))
                {
                    var type = this.MediaSupportService.GetMediaType(url);
                    if (type != MediaType.Unknown)
                    {
                        long tmp = this.GetContentLength(url);
                        if (tmp > 0)
                        {
                            var requirements = this.EstablishRequirements(tmp);
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                this.Requirements = requirements;
                                this.ContentLength = tmp;
                            });
                            return true;
                        }
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        private IList<TransferRequirement> EstablishRequirements(long contentLength)
        {
            var list = new List<TransferRequirement>();

            if (!this.TransferMode)
            {
                if (contentLength > CellularThreshold && contentLength <= ExternalPowerThreshold)
                {
                    list.Add(WifiRequired);
                }
                else if (contentLength > ExternalPowerThreshold)
                {
                    list.Add(WifiRequired);
                    list.Add(PowerRequired);
                }
                else
                {
                    list.Add(Cellular3GRequired);
                }
            }
            else
            {
                list.Add(ConnectionRequired);
                list.Add(ActiveAppRequired);
            }

            return list;
        }

        private long GetContentLength(string url)
        {
            long contentLength = 0;
            var sizeRequest = (HttpWebRequest)WebRequest.Create(url);
            sizeRequest.AllowReadStreamBuffering = false;
            sizeRequest.Method = "GET";
            var handle = new ManualResetEvent(false);
            sizeRequest.BeginGetResponse((result) =>
            {
                HttpWebRequest state = (HttpWebRequest)result.AsyncState;

                try
                {
                    HttpWebResponse response = (HttpWebResponse)state.EndGetResponse(result);
                    contentLength = response.ContentLength;
                    if (contentLength <= 0)
                    {
                        long tmp;
                        if (Int64.TryParse(response.Headers["Content-Length"], out tmp))
                        {
                            contentLength = tmp;
                        }
                    }
                    response.Close();
                }
                catch
                {
                }
                finally
                {
                    handle.Set();
                }
            }, sizeRequest);

            handle.WaitOne();
            sizeRequest.Abort();

            return contentLength;
        }

        public void SaveCurrentState()
        {
            var state = new NewDownloadViewModelState()
            {
                URL = this.URL,
                Title = this.Title,
                Extension = this.Extension,
                ContentLength = this.ContentLength,
                TransferMode = this.TransferMode,
            };

            if (IsolatedStorageSettings.ApplicationSettings.Contains("NewDownloadViewModelState"))
            {
                IsolatedStorageSettings.ApplicationSettings["NewDownloadViewModelState"] = state;
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings.Add("NewDownloadViewModelState", state);
            }
        }

        public void ResetCurrentState()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("NewDownloadViewModelState"))
            {
                IsolatedStorageSettings.ApplicationSettings["NewDownloadViewModelState"] = null;
            }
        }

        private void ResumeState()
        {
            NewDownloadViewModelState state = null;
            if (IsolatedStorageSettings.ApplicationSettings.TryGetValue<NewDownloadViewModelState>("NewDownloadViewModelState", out state))
            {
                if (state != null)
                {
                    this.URL = state.URL;
                    this.Title = state.Title;
                    this.Extension = state.Extension;
                    this.ContentLength = state.ContentLength;
                    this.TransferMode = state.TransferMode;
                    this.Requirements = this.EstablishRequirements(state.ContentLength);
                }
            }
        }
    }
}

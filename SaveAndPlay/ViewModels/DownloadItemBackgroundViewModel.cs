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

using Microsoft.Phone.BackgroundTransfer;
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.Helpers;
using SaveAndPlay.Resources;
using SaveAndPlay.Scheduler;
using SaveAndPlay.ViewModels.Messages;
using SaveAndPlay.ViewService;
using System;
using System.IO;

namespace SaveAndPlay.ViewModels
{
    public class DownloadItemBackgroundViewModel : BaseDownloadItemViewModel
    {
        private long bytesReceived = 0;

        private BackgroundTransferRequest BackgroundTransferRequest { get; set; }

        public override string RequestId
        {
            get
            {
                if (this.BackgroundTransferRequest != null)
                {
                    return this.BackgroundTransferRequest.RequestId;
                }
                return string.Empty;
            }
        }

        protected override long BytesReceived
        {
            get { return this.bytesReceived; }
        }

        public override bool IsBound
        {
            get { return false; }
        }

        public override string Title
        {
            get
            {
                if (this.BackgroundTransferRequest != null)
                {
                    return this.BackgroundTransferRequest.Tag;
                }
                return string.Empty;
            }
        }

        public override string Host
        {
            get
            {
                if (this.BackgroundTransferRequest != null &&
                    this.BackgroundTransferRequest.RequestUri != null)
                {
                    return this.BackgroundTransferRequest.RequestUri.Host;
                }
                return string.Empty;
            }
        }
        
        public DownloadItemBackgroundViewModel(BackgroundTransferRequest backgroundTransferRequest)
        {
            this.BackgroundTransferRequest = backgroundTransferRequest;
            this.ProcessTransfer(backgroundTransferRequest);
            this.WatchTransfer();
            this.ProcessTransfer(backgroundTransferRequest);
        }

        protected override void Cancel(string id)
        {
            this.RemoveTransferRequest(id);
        }

        public override void Clean()
        {
            if (this.BackgroundTransferRequest != null)
            {
                this.BackgroundTransferRequest.Dispose();
            }
        }

        private void ProcessTransfer(BackgroundTransferRequest transfer)
        {
            if (this.Status != DownloadList.Enum_TransferStatus_Cancelled)
            {
                this.Status = UIHelper.GetEnumLocalized<TransferStatus, DownloadList>(transfer.TransferStatus);
            }
            switch (transfer.TransferStatus)
            {
                case TransferStatus.Completed:
                    RemoveTransferRequest(transfer.RequestId);
                    // If the status code of a completed transfer is 200 or 206, the transfer was successful
                    if (transfer.StatusCode == 200 || transfer.StatusCode == 206)
                    {
                        var type = MediaSupportService.GetMediaType(transfer.Tag);
                        string filename = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(transfer.Tag);
                        string filepath = Path.Combine(MediaSupportService.GetStorageDirectory(), filename);
                        if (IsolatedStorageFileHelper.MoveFile(transfer.DownloadLocation.OriginalString, filepath))
                        {
                            if (type == Service.MediaType.Audio)
                            {
                                App.DataCacheServiceInstance.AddAudioMedia(System.IO.Path.GetFileNameWithoutExtension(transfer.Tag), string.Empty, string.Empty, filename, false, TaskParam.Async());
                            }
                            else
                            {
                                App.DataCacheServiceInstance.AddVideoMedia(System.IO.Path.GetFileNameWithoutExtension(transfer.Tag), filename, false, TaskParam.Async());
                            }
                        }
                    }
                    else
                    {
                        IsolatedStorageFileHelper.DeleteFileIfExists(transfer.DownloadLocation.OriginalString);
                        this.Status = DownloadList.Enum_TransferStatus_Cancelled;
                        this.Progress = 0;
                    }
                    this.TransferRate = null;
                    break;

                case TransferStatus.WaitingForExternalPower:
                    this.DialogViewService.MessageBoxOkInUIThead(NewDownload.DownloadWarning, DownloadList.WaitingForExternalPower);
                    break;

                case TransferStatus.WaitingForExternalPowerDueToBatterySaverMode:
                    this.DialogViewService.MessageBoxOkInUIThead(NewDownload.DownloadWarning, DownloadList.WaitingForExternalPowerDueToBatterySaverMode);
                    break;

                case TransferStatus.WaitingForNonVoiceBlockingNetwork:
                    this.DialogViewService.MessageBoxOkInUIThead(NewDownload.DownloadWarning, DownloadList.WaitingForNonVoiceBlockingNetwork);
                    break;

                case TransferStatus.WaitingForWiFi:
                    this.DialogViewService.MessageBoxOkInUIThead(NewDownload.DownloadWarning, DownloadList.WaitingForWiFi);
                    break;
            }
        }

        private void WatchTransfer()
        {
            this.BackgroundTransferRequest.TransferStatusChanged += (sender, e) =>
            {
                this.ProcessTransfer(e.Request);
                base.ComputeTransferRate();
            };
            this.BackgroundTransferRequest.TransferProgressChanged += (sender, e) =>
            {
                this.bytesReceived = e.Request.BytesReceived;
                if (this.BackgroundTransferRequest != null && this.BackgroundTransferRequest.BytesReceived > 0)
                {
                    this.Progress = (int)((double)this.BackgroundTransferRequest.BytesReceived /
                                           this.BackgroundTransferRequest.TotalBytesToReceive * 100);
                }
                base.ComputeTransferRate();
            };
        }

        private void RemoveTransferRequest(string transferID)
        {
            BackgroundTransferRequest transferToRemove = BackgroundTransferService.Find(transferID);

            if (transferToRemove != null)
            {
                try
                {
                    BackgroundTransferService.Remove(transferToRemove);
                }
                catch
                {
                    //TODO
                }
            }
        }
    }
}

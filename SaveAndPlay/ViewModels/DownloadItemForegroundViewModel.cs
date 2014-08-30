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
using SaveAndPlay.Helpers;
using SaveAndPlay.Resources;
using SaveAndPlay.Scheduler;
using SaveAndPlay.Service;
using SaveAndPlay.ViewModels.Messages;
using System;
using System.IO;

namespace SaveAndPlay.ViewModels
{
    public class DownloadItemForegroundViewModel : BaseDownloadItemViewModel
    {
        private long bytesReceived = 0;
        private long totalBytesToReceive = 0;

        public ForegroundTransferRequest ForegroundTransferRequest { get; set; }

        public override string RequestId
        {
            get
            {
                if (this.ForegroundTransferRequest != null)
                {
                    return this.ForegroundTransferRequest.RequestId;
                }
                return string.Empty;
            }
        }

        public override string Host
        {
            get
            {
                if (this.ForegroundTransferRequest != null)
                {
                    return this.ForegroundTransferRequest.Uri.Host;
                }
                return string.Empty;
            }
        }

        public override bool IsBound
        {
            get { return true; }
        }

        protected override long BytesReceived
        {
            get { return this.bytesReceived; }
        }

        public DownloadItemForegroundViewModel(ForegroundTransferRequest request)
            : base()
        {
            this.ForegroundTransferRequest = request;
            this.WatchTransfer();
        }

        protected override void Cancel(string id)
        {
            if (this.ForegroundTransferRequest != null)
            {
                this.ForegroundTransferRequest.CancelTransfer();
            }
        }

        public override void Clean()
        {
        }

        public override string Title
        {
            get
            {
                if (this.ForegroundTransferRequest != null)
                {
                    return this.ForegroundTransferRequest.Tag;
                }
                return string.Empty;
            }
        }

        private void ProcessTransfer(ForegroundTransferEventArgs transfer)
        {
            if (transfer.Error != null ||
                this.Status == DownloadList.Enum_TransferStatus_Cancelled ||
                this.Status == DownloadList.Enum_TransferStatus_Paused)
            {
                IsolatedStorageFileHelper.DeleteFileIfExists(this.ForegroundTransferRequest.DownloadLocation);
                this.UpdateStatus(transfer.Status == TransferStatus.Paused ? DownloadList.Enum_TransferStatus_CancelledByQuit : DownloadList.Enum_TransferStatus_Cancelled);
                this.UpdateProgress(0);
                this.UpdateTransferRate(null);
            }
            else if (transfer.Status == TransferStatus.Completed)
            {
                this.UpdateProgress(100);
                this.UpdateStatus(UIHelper.GetEnumLocalized<TransferStatus, DownloadList>(TransferStatus.Completed));
                var type = MediaSupportService.GetMediaType(this.ForegroundTransferRequest.Tag);
                string filename = Guid.NewGuid().ToString() + Path.GetExtension(this.ForegroundTransferRequest.Tag);
                string filepath = Path.Combine(MediaSupportService.GetStorageDirectory(), filename);
                if (IsolatedStorageFileHelper.MoveFile(this.ForegroundTransferRequest.DownloadLocation, filepath))
                {
                    if (type == MediaType.Audio)
                    {
                        App.DataCacheServiceInstance.AddAudioMedia(Path.GetFileNameWithoutExtension(this.ForegroundTransferRequest.Tag), string.Empty, string.Empty, filename, false, TaskParam.Async());
                    }
                    else
                    {
                        App.DataCacheServiceInstance.AddVideoMedia(Path.GetFileNameWithoutExtension(this.ForegroundTransferRequest.Tag), filename, false, TaskParam.Async());
                    }
                }
                this.UpdateTransferRate(null);
            }
            else
            {
                this.UpdateStatus(UIHelper.GetEnumLocalized<TransferStatus, DownloadList>(TransferStatus.Transferring));
            }
        }

        private void WatchTransfer()
        {
            this.ForegroundTransferRequest.TransferStatusChanged += (sender, e) =>
            {
                this.ProcessTransfer(e);
            };

            this.ForegroundTransferRequest.TransferProgressChanged += (sender, e) =>
            {
                this.bytesReceived = e.BytesReceived;
                this.totalBytesToReceive = e.TotalBytesToReceive;
                base.ComputeTransferRate();
                if (this.BytesReceived > 0)
                {
                    this.UpdateProgress((int)((double)this.bytesReceived / this.totalBytesToReceive * 100));
                }
            };
        }
    }
}

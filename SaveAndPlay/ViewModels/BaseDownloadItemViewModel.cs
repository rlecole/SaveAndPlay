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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.Service;
using System.Windows;

namespace SaveAndPlay.ViewModels
{
    public abstract class BaseDownloadItemViewModel : BaseViewModel
    {
        private class LastUpdate
        {
            public DateTime DateTime { get; set; }
            public long Bytes { get; set; }
        }

        private LastUpdate[] lastUpdates = new LastUpdate[2];

        protected abstract long BytesReceived { get; }
        public abstract string RequestId { get; }
        public abstract string Host { get; }
        public abstract string Title { get; }
        public abstract bool IsBound { get; }

        protected abstract void Cancel(string id);
        public abstract void Clean();

        private int progress = 0;
        public int Progress
        {
            get { return progress; }
            set { progress = value; RaisePropertyChanged("Progress"); }
        }

        private string status;
        public string Status
        {
            get { return status; }
            set { status = value; RaisePropertyChanged("Status"); }
        }

        private double? transferRate;
        public double? TransferRate
        {
            get { return transferRate; }
            set { transferRate = value; RaisePropertyChanged("TransferRate"); }
        }

        public RelayCommand<string> CancelCommand { get; private set; }

        public BaseDownloadItemViewModel()
        {
            this.CancelCommand = new RelayCommand<string>((id) =>
            {
                this.Cancel(id);
            });
        }

        protected void ComputeTransferRate()
        {
            this.lastUpdates[0] = this.lastUpdates[1];
            this.lastUpdates[1] = new LastUpdate()
            {
                DateTime = DateTime.Now,
                Bytes = this.BytesReceived
            };
            if (this.lastUpdates[0] != null &&
                this.lastUpdates[1] != null)
            {
                var dateDiff = this.lastUpdates[1].DateTime - this.lastUpdates[0].DateTime;
                long bytesDiff = this.lastUpdates[1].Bytes - this.lastUpdates[0].Bytes;
                if (bytesDiff > 0)
                {
                    this.TransferRate = bytesDiff / dateDiff.TotalSeconds;
                }
            }
        }

        protected void UpdateStatus(string status)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                this.Status = status;
            });
        }

        protected void UpdateTransferRate(double? transferRate)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                this.TransferRate = transferRate;
            });
        }

        protected void UpdateProgress(int progress)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                this.Progress = progress;
            });
        }
    }
}

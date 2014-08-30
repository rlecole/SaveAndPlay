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

using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using Microsoft.Phone.BackgroundTransfer;
using SaveAndPlay.ViewModels.Messages;
using SaveAndPlay.Service;
using System.Collections.Generic;
using System;
using System.Collections;

namespace SaveAndPlay.ViewModels
{
    public class DownloadListViewModel : ViewModelBase
    {
        private ObservableCollection<BaseDownloadItemViewModel> requests;
        public ObservableCollection<BaseDownloadItemViewModel> Requests
        {
            get { return requests; }
            set { requests = value; base.RaisePropertyChanged("Requests"); }
        }

        private bool noRequest;
        public bool NoRequest
        {
            get { return noRequest; }
            set { noRequest = value; base.RaisePropertyChanged("NoRequest"); }
        }

        public DownloadListViewModel()
        {
            base.MessengerInstance.Register<Update>(this, (o) =>
            {
                this.Update(o.Scope);
            });
        }

        public void Update(UpdateScope scope)
        {
            IEnumerable<BackgroundTransferRequest> newBGRequests = null;
            IEnumerable<ForegroundTransferRequest> newFGRequests = null;

            if (scope == UpdateScope.All || scope == UpdateScope.Background)
            {
                newBGRequests = BackgroundTransferService.Requests.ToList();
            }
            if (scope == UpdateScope.All || scope == UpdateScope.Foreground)
            {
                newFGRequests = ForegroundTransferService.Requests;
            }

            if (this.Requests != null)
            {
                if (newBGRequests != null)
                {
                    foreach (var newBGRequest in newBGRequests)
                    {
                        if (!this.Requests.Any(r => r.RequestId == newBGRequest.RequestId))
                        {
                            this.Requests.Add(new DownloadItemBackgroundViewModel(newBGRequest));
                        }
                        else
                        {
                            // The Requests property returns new references, so make sure that you dispose of the old references to avoid memory leaks.
                            newBGRequest.Dispose();
                        }
                    }
                }
                if (newFGRequests != null)
                {
                    foreach (var newFGRequest in newFGRequests)
                    {
                        if (!this.Requests.Any(r => r.RequestId == newFGRequest.RequestId))
                        {
                            this.Requests.Add(new DownloadItemForegroundViewModel(newFGRequest));
                        }
                    }
                }
            }
            else
            {
                this.Requests = new ObservableCollection<BaseDownloadItemViewModel>();
                if (newBGRequests != null)
                {
                    newBGRequests.Aggregate(this.Requests, (list, r) =>
                    {
                        this.Requests.Add(new DownloadItemBackgroundViewModel(r));
                        return list;
                    });
                }
                if (newFGRequests != null)
                {
                    newFGRequests.Aggregate(this.Requests, (list, r) =>
                    {
                        this.Requests.Add(new DownloadItemForegroundViewModel(r));
                        return list;
                    });
                }
            }
            this.CheckEmptyness();
        }

        public void Clear()
        {
            var newBGRequests = BackgroundTransferService.Requests.ToList();
            var newFGRequests = ForegroundTransferService.Requests;

            if (this.Requests != null)
            {
                foreach (var request in this.Requests.Reverse())
                {
                    if (!newBGRequests.Any(r => r.RequestId == request.RequestId) &&
                        !newFGRequests.Any(r => r.RequestId == request.RequestId))
                    {
                        request.Clean();
                        this.Requests.Remove(request);
                    }
                }
            }
            // The Requests property returns new references, so make sure that you dispose of the old references to avoid memory leaks.
            foreach (var newBGRequest in newBGRequests)
            {
                newBGRequest.Dispose();
            }
            this.CheckEmptyness();
        }

        private void CheckEmptyness()
        {
            this.NoRequest = this.Requests == null || this.Requests.Count() <= 0;
        }
    }
}

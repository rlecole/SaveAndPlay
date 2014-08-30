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
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using Microsoft.Phone.BackgroundTransfer;
using SaveAndPlay.Helpers;
using System.Windows;
using System.Threading;
using SaveAndPlay.ViewService;
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.Resources;

namespace SaveAndPlay.Service
{
    public class ForegroundTransferEventArgs : EventArgs
    {
        public long BytesReceived { get; set; }
        public long TotalBytesToReceive { get; set; }
        public TransferStatus Status { get; set; }
        public Exception Error { get; set; }
    }

    public class ForegroundTransferRequest
    {
        private static TimeSpan UpdateProgressPeriod = new TimeSpan(0, 0, 1);

        private bool cancelledByQuit = false;
        private HttpWebRequest request = null;
        private HttpWebResponse response = null;
        private DateTime? lastUpdateProgress = null;

        public Uri Uri { get; set; }
        public string DownloadLocation { get; set; }
        public string RequestId { get; set; }
        public string Tag { get; set; }

        public event EventHandler<ForegroundTransferEventArgs> TransferProgressChanged;
        public event EventHandler<ForegroundTransferEventArgs> TransferStatusChanged;

        public IDialogViewService DialogViewService
        {
            get { return ServiceLocator.Current.GetInstance<IDialogViewService>(); }
        }

        public ForegroundTransferRequest(Uri uri)
        {
            this.Uri = uri;
            this.RequestId = Guid.NewGuid().ToString();
        }

        private void GetData(IAsyncResult result)
        {
            IsolatedStorageFileStream streamToWriteTo = null;
            Stream responseStream = null;

            try
            {
                this.request = (HttpWebRequest)result.AsyncState;
                response = (HttpWebResponse)request.EndGetResponse(result);

                responseStream = response.GetResponseStream();
                int read;
                byte[] data = new byte[16 * 1024];
                long totalValue = 0;
                long bytesReceived = 0;
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    totalValue = response.ContentLength;
                    
                    if (this.CheckIfEnoughSpaceLeft(store, totalValue))
                    {
                        streamToWriteTo = store.CreateFile(this.DownloadLocation);

                        this.UpdateStatus(new ForegroundTransferEventArgs() { Status = TransferStatus.Transferring });
                        while (responseStream.CanRead && (read = responseStream.Read(data, 0, data.Length)) > 0)
                        {
                            bytesReceived += read;
                            this.UpdateProgress(new ForegroundTransferEventArgs()
                            {
                                BytesReceived = bytesReceived,
                                TotalBytesToReceive = totalValue
                            });
                            streamToWriteTo.Write(data, 0, read);
                        }
                        this.UpdateProgress(new ForegroundTransferEventArgs()
                        {
                            BytesReceived = bytesReceived,
                            TotalBytesToReceive = totalValue
                        });
                    }
                }
                if (streamToWriteTo != null)
                {
                    streamToWriteTo.Close();
                    streamToWriteTo = null;
                }

                if (totalValue > 0 &&
                    bytesReceived >= totalValue)
                {
                    this.UpdateStatus(new ForegroundTransferEventArgs() { Status = TransferStatus.Completed });
                }
                else
                {
                    this.UpdateStatus(new ForegroundTransferEventArgs()
                    {
                        Status = cancelledByQuit ? TransferStatus.Paused : TransferStatus.Unknown,
                        Error = new Exception()
                    });
                }
            }
            catch (Exception e)
            {
                if (responseStream != null)
                {
                    responseStream.Close();
                    responseStream = null;
                }
                if (this.response != null)
                {
                    this.response.Close();
                    this.response = null;
                }
                if (streamToWriteTo != null)
                {
                    streamToWriteTo.Close();
                    streamToWriteTo = null;
                }
                this.UpdateStatus(new ForegroundTransferEventArgs()
                {
                    Status = cancelledByQuit ? TransferStatus.Paused : TransferStatus.Unknown,
                    Error = e
                });
            }
            finally
            {
                if (responseStream != null)
                {
                    responseStream.Close();
                }
                if (this.response != null)
                {
                    this.response.Close();
                    this.response = null;
                }
                this.request = null;
                ForegroundTransferService.Remove(this);
            }
        }

        public void StartTransfer()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Uri);
            request.AllowReadStreamBuffering = false;
            var async = request.BeginGetResponse(new AsyncCallback(this.GetData), request);
        }

        public void CancelTransfer(bool cancelledByQuit = false)
        {
            if (this.request != null)
            {
                this.cancelledByQuit = cancelledByQuit;
                this.request.Abort();
                response.Close();
            }
        }

        public bool IsBusy()
        {
            return this.request != null;
        }

        private void UpdateProgress(ForegroundTransferEventArgs e)
        {
            DateTime now = DateTime.Now;
            if (!this.lastUpdateProgress.HasValue ||
                (now - this.lastUpdateProgress) > UpdateProgressPeriod)
            {
                this.lastUpdateProgress = now;

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (this.TransferProgressChanged != null)
                    {
                        this.TransferProgressChanged(this, e);
                    }
                });
            }
        }

        private void UpdateStatus(ForegroundTransferEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (this.TransferStatusChanged != null)
                {
                    this.TransferStatusChanged(this, e);
                }
            });
        }

        private bool CheckIfEnoughSpaceLeft(IsolatedStorageFile store, long size)
        {
            bool enoughSpaceLeft = false;

            var handle = new ManualResetEvent(false);
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    // Request size.
                    Int64 spaceRequired = size;
                    Int64 available = store.AvailableFreeSpace;

                    // If available space is less than
                    // what is requested, try to increase.
                    if (available < spaceRequired)
                    {
                        spaceRequired = spaceRequired - available;
                        // Request more quota space.
                        if (!store.IncreaseQuotaTo(store.Quota + spaceRequired + 1))
                        {
                            // The user clicked NO to the
                            // host's prompt to approve the quota increase.
                            enoughSpaceLeft = false;
                        }
                        else
                        {
                            // The user clicked YES to the
                            // host's prompt to approve the quota increase.
                            enoughSpaceLeft = true;
                        }
                    }
                    enoughSpaceLeft = true;
                }
                catch (Exception)
                {
                    // Handle that store could not be accessed.
                    enoughSpaceLeft = false;
                }
                if (!enoughSpaceLeft)
                {
                    this.DialogViewService.MessageBoxOk(NewDownload.DownloadRequirementsFailed, string.Format(NewDownload.NotEnoughFreeSpace, this.Tag));
                }
                handle.Set();
            });
            handle.WaitOne();

            return enoughSpaceLeft;
        }
    }

    public static class ForegroundTransferService
    {
        private static object requestSyncRoot = new object();
        private static IList<ForegroundTransferRequest> requests = new List<ForegroundTransferRequest>();

        static ForegroundTransferService()
        {
            IsolatedStorageFileHelper.RemoveDirectoryContent("/shared/mtransfers");
        }

        public static bool AnyRequestActive
        {
            get
            {
                lock (requestSyncRoot)
                {
                    return requests.Any(r => r.IsBusy());
                }
            }
        }

        public static IList<ForegroundTransferRequest> Requests
        {
            get
            {
                lock (requestSyncRoot)
                {
                    return requests.ToList();
                }
            }
        }

        public static void Add(ForegroundTransferRequest request)
        {
            lock (requestSyncRoot)
            {
                requests.Add(request);
            }
            request.StartTransfer();
        }

        public static void Remove(ForegroundTransferRequest request, bool cancelledByQuit = false)
        {
            lock (requestSyncRoot)
            {
                if (requests.Contains(request))
                {
                    request.CancelTransfer(cancelledByQuit);
                    requests.Remove(request);
                }
            }
        }

        public static void RemoveAll(bool cancelledByQuit = false)
        {
            lock (requestSyncRoot)
            {
                foreach (var request in requests.Reverse())
                {
                    Remove(request, cancelledByQuit);
                }
            }
        }
    }
}

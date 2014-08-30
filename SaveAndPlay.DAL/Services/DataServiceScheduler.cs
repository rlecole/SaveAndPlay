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

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;

//namespace SaveAndPlay.DAL.Services
//{
//    public interface IDataServiceSchedulerClient
//    {
//        void TaskRunning(string current, List<string> toCome);
//        void TaskCompleted(DataAsyncResult result, string current, List<string> toCome);
//    }

//    public class DataAsyncResult : IAsyncResult
//    {
//        private object state;
//        private bool isCompleted;
//        private ManualResetEvent manualResetEvent;

//        public DataAsyncResult(object state, bool isCompleted)
//        {
//            this.state = state;
//            this.isCompleted = isCompleted;
//            this.manualResetEvent = new ManualResetEvent(false);
//        }

//        public ManualResetEvent ManualResetEvent
//        {
//            get { return manualResetEvent; }
//        }

//        public object AsyncState
//        {
//            get { return this.state; }
//            set { this.state = value; }
//        }

//        public WaitHandle AsyncWaitHandle
//        {
//            get { return null; }
//        }

//        public bool CompletedSynchronously
//        {
//            get { return false; }
//        }

//        public bool IsCompleted
//        {
//            get { return this.isCompleted; }
//            set { this.isCompleted = value; }
//        }
//    }

//    public class Task
//    {
//        public Func<object> Operation { get; set; }
//        public DataAsyncResult AsyncResult { get; set; }
//        public TaskParam Parameters { get; set; }
//    }

//    public enum TaskRunType
//    {
//        Sync,
//        AsyncWithActionCallback,
//        AsyncWithContractCallback
//    }

//    public class TaskParam
//    {
//        public TaskRunType RunType { get; set; }
//        public AsyncCallback Callback { get; set; }
//        public string Id { get; set; }

//        public static TaskParam Sync(string id)
//        {
//            return new TaskParam() { RunType = TaskRunType.Sync, Id = id };
//        }

//        public static TaskParam AsyncAction(string id, AsyncCallback callback)
//        {
//            return new TaskParam() { RunType = TaskRunType.AsyncWithActionCallback, Callback = callback, Id = id };
//        }

//        public static TaskParam AsyncContract(string id)
//        {
//            return new TaskParam() { RunType = TaskRunType.AsyncWithContractCallback, Id = id };
//        }

//        public static TaskParam Async()
//        {
//            return new TaskParam() { RunType = TaskRunType.AsyncWithActionCallback, Callback = (o) => {} };
//        }
//    }

//    public class DataServiceScheduler
//    {
//        private const int sleep = 50;
//        private bool running = true;
//        private object syncRoot = new object();
//        private Queue<Task> tasks = new Queue<Task>();
//        private Task currentTask = null;

//        private List<IDataServiceSchedulerClient> clients = new List<IDataServiceSchedulerClient>();
//        private object clientSyncRoot = new object();

//        public DataServiceScheduler()
//        {
//            this.Start();
//        }

//        public void Register(IDataServiceSchedulerClient client)
//        {
//            lock (this.clientSyncRoot)
//            {
//                if (!this.clients.Contains(client))
//                {
//                    this.clients.Add(client);

//                    string currentTaskId = null;
//                    List<string> toComeTaskIds = null;
//                    lock (this.syncRoot)
//                    {
//                        if (this.currentTask != null)
//                        {
//                            currentTaskId = this.currentTask.Parameters.Id;
//                        }
//                        if (this.tasks != null && this.tasks.Count > 0)
//                        {
//                            toComeTaskIds = this.tasks.Select(x => x.Parameters.Id).ToList();
//                        }
//                    }
//                    client.TaskRunning(currentTaskId, toComeTaskIds);
//                }
//            }
//        }

//        public void Unregister(IDataServiceSchedulerClient client)
//        {
//            lock (this.clientSyncRoot)
//            {
//                if (this.clients.Contains(client))
//                {
//                    this.clients.Remove(client);
//                }
//            }
//        }

//        private void Notify(Action<IDataServiceSchedulerClient> action)
//        {
//            List<IDataServiceSchedulerClient> toNotify;
//            lock (this.clientSyncRoot)
//            {
//                toNotify = new List<IDataServiceSchedulerClient>(this.clients);
//            }

//            if (toNotify != null && toNotify.Count > 0)
//            {
//                foreach (var client in toNotify)
//                {
//                    action(client);
//                }
//            }
//        }


//        public void Start()
//        {
//            ThreadPool.QueueUserWorkItem((o) =>
//            {
//                this.currentTask = null;
//                while (running)
//                {
//                    if (this.currentTask != null)
//                    {
//                        // launch data service request
//                        try
//                        {
//                            this.currentTask.AsyncResult.AsyncState = this.currentTask.Operation();
//                            this.currentTask.AsyncResult.IsCompleted = true;
//                        }
//                        catch (Exception e)
//                        {
//                            this.currentTask.AsyncResult.IsCompleted = false;
//                        }

//                        // notify the end of the request
//                        try
//                        {
//                            switch (this.currentTask.Parameters.RunType)
//                            {
//                                case TaskRunType.AsyncWithActionCallback:
//                                    this.currentTask.AsyncResult.ManualResetEvent.Set();
//                                    if (this.currentTask.Parameters.Callback != null)
//                                    {
//                                        this.currentTask.Parameters.Callback(this.currentTask.AsyncResult);
//                                    }
//                                    this.NotifyCompleted();
//                                    break;
//                                case TaskRunType.Sync:
//                                case TaskRunType.AsyncWithContractCallback:
//                                    this.currentTask.AsyncResult.ManualResetEvent.Set();
//                                    this.NotifyCompleted();
//                                    break;
//                                default:
//                                    break;
//                            }
//                        }
//                        catch
//                        {
//                            // catch everything just to avoid a crash that would stop the data service dedicated thread...
//                        }

//                        // reset current processed request
//                        this.currentTask = null;
//                    }

//                    // get the next request
//                    lock (this.syncRoot)
//                    {
//                        if (this.tasks.Count > 0 && running)
//                        {
//                            this.currentTask = tasks.Dequeue();
//                        }
//                    }

//                    // sleep if no request has been pushed
//                    if (this.currentTask == null && running)
//                    {
//                        Thread.Sleep(sleep);
//                    }
//                }
//            });
//        }

//        public void Stop()
//        {
//            this.running = false;
//        }

//        public Task GetCurrentRequestAndStopScheduler()
//        {
//            lock (this.syncRoot)
//            {
//                this.Stop();
//                return this.currentTask;
//            }
//        }

//        public DataAsyncResult EnqueueRequest(Func<object> operation, TaskParam parameters)
//        {
//            DataAsyncResult result = new DataAsyncResult(null, false);
//            var newRequest = new Task()
//            {
//                Operation = operation,
//                AsyncResult = result,
//                Parameters = parameters
//            };

//            lock (syncRoot)
//            {
//                this.tasks.Enqueue(newRequest);
//            }
//            return result;
//        }

//        private void NotifyCompleted()
//        {
//            List<string> toCome = null;
//            lock (this.syncRoot)
//            {
//                if (this.tasks != null && this.tasks.Count > 0)
//                {
//                    toCome = this.tasks.Select(x => x.Parameters.Id).ToList();
//                }
//            }
//            this.Notify(x => x.TaskCompleted(this.currentTask.AsyncResult, this.currentTask.Parameters.Id, toCome));
//        }
//    }
//}

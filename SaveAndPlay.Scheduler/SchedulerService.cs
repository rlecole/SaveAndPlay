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
using System.Linq;
using System.Text;
using System.Threading;

namespace SaveAndPlay.Scheduler
{
    public class SchedulerService
    {
        private const int sleep = 250;
        private bool running;
        private Queue<Task> tasks = new Queue<Task>();
        private Task currentTask = null;

        private List<IDataServiceSchedulerClient> clients = new List<IDataServiceSchedulerClient>();
        private object clientSyncRoot = new object();
        private object taskSyncRoot = new object();

        public SchedulerService()
        {
            this.Start();
        }

        public void Register(IDataServiceSchedulerClient client)
        {
            lock (this.clientSyncRoot)
            {
                if (!this.clients.Contains(client))
                {
                    this.clients.Add(client);

                    string currentTaskId = null;
                    List<string> toComeTaskIds = null;
                    lock (this.taskSyncRoot)
                    {
                        if (this.currentTask != null)
                        {
                            currentTaskId = this.currentTask.Parameters.Id;
                        }
                        if (this.tasks != null && this.tasks.Count > 0)
                        {
                            toComeTaskIds = this.tasks.Select(x => x.Parameters.Id).ToList();
                        }
                    }
                    client.TaskRunning(currentTaskId, toComeTaskIds);
                }
            }
        }

        public void Unregister(IDataServiceSchedulerClient client)
        {
            lock (this.clientSyncRoot)
            {
                if (this.clients.Contains(client))
                {
                    this.clients.Remove(client);
                }
            }
        }

        private void Notify(Action<IDataServiceSchedulerClient> action)
        {
            List<IDataServiceSchedulerClient> toNotify;
            toNotify = new List<IDataServiceSchedulerClient>(this.clients);

            if (toNotify != null && toNotify.Count > 0)
            {
                foreach (var client in toNotify)
                {
                    action(client);
                }
            }
        }

        public void Start()
        {
            if (!running)
            {
                Task task = null;
                running = true;
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    while (running)
                    {
                        if (task != null)
                        {
                            try
                            {
                                // launch data service task
                                task.AsyncResult.AsyncState = task.Operation();
                                task.AsyncResult.IsCompleted = true;
                            }
                            catch
                            {
                                // catch everything just to avoid a crash that would stop the data service dedicated thread...
                                task.AsyncResult.IsCompleted = false;
                            }

                            lock (this.clientSyncRoot)
                            {
                                // notify the end of the request
                                try
                                {
                                    switch (task.Parameters.RunType)
                                    {
                                        case TaskRunType.AsyncWithActionCallback:
                                            task.AsyncResult.ManualResetEvent.Set();
                                            if (task.Parameters.Callback != null)
                                            {
                                                task.Parameters.Callback(task.AsyncResult);
                                            }
                                            this.NotifyCompleted();
                                            break;
                                        case TaskRunType.Sync:
                                        case TaskRunType.AsyncWithContractCallback:
                                            task.AsyncResult.ManualResetEvent.Set();
                                            this.NotifyCompleted();
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                catch
                                {
                                    // catch everything just to avoid a crash that would stop the data service dedicated thread...
                                }

                                // reset current processed request
                                lock (this.taskSyncRoot)
                                {
                                    this.currentTask = null;
                                    task = this.currentTask;
                                }
                            }
                        }

                        // get the next request
                        lock (this.taskSyncRoot)
                        {
                            if (this.tasks.Count > 0 && running)
                            {
                                this.currentTask = tasks.Dequeue();
                                task = this.currentTask;
                            }
                        }

                        // sleep if no request has been pushed
                        if (task == null && running)
                        {
                            using (EventWaitHandle tmpEvent = new ManualResetEvent(false))
                            {
                                tmpEvent.WaitOne(TimeSpan.FromMilliseconds(sleep));
                            }
                        }
                    }
                });
            }
        }

        public void Stop()
        {
            this.running = false;
        }

        public Task GetCurrentRequestAndStopScheduler()
        {
            lock (this.taskSyncRoot)
            {
                this.Stop();
                return this.currentTask;
            }
        }

        public DataAsyncResult EnqueueRequest(Func<object> operation, TaskParam parameters)
        {
            DataAsyncResult result = new DataAsyncResult(null, false);
            var newRequest = new Task()
            {
                Operation = operation,
                AsyncResult = result,
                Parameters = parameters
            };

            lock (this.taskSyncRoot)
            {
                this.tasks.Enqueue(newRequest);
            }
            return result;
        }

        private void NotifyCompleted()
        {
            DataAsyncResult result = null;
            string id = null;
            List<string> toCome = null;
            lock (this.taskSyncRoot)
            {
                result = this.currentTask.AsyncResult;
                id = this.currentTask.Parameters.Id;
                if (this.tasks != null && this.tasks.Count > 0)
                {
                    toCome = this.tasks.Select(x => x.Parameters.Id).ToList();
                }
            }
            this.Notify(x => x.TaskCompleted(result, id, toCome));
        }
    }
}

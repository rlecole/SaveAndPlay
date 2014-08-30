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

namespace SaveAndPlay.Scheduler
{
    public class TaskParam
    {
        public TaskRunType RunType { get; set; }
        public AsyncCallback Callback { get; set; }
        public string Id { get; set; }

        public static TaskParam Sync(string id)
        {
            return new TaskParam() { RunType = TaskRunType.Sync, Id = id };
        }

        public static TaskParam AsyncAction(string id, AsyncCallback callback)
        {
            return new TaskParam() { RunType = TaskRunType.AsyncWithActionCallback, Callback = callback, Id = id };
        }

        public static TaskParam AsyncContract(string id)
        {
            return new TaskParam() { RunType = TaskRunType.AsyncWithContractCallback, Id = id };
        }

        public static TaskParam Async()
        {
            return new TaskParam() { RunType = TaskRunType.AsyncWithActionCallback, Callback = (o) => { } };
        }
    }
}

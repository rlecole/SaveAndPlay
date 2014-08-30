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

using SaveAndPlay.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace SaveAndPlay.DAL.Services
{
    public class AudioMediaDataService : BaseRequestDataService<AudioMedia>
    {
        public void SelectAll(AsyncCallback callback)
        {
            List<AudioMedia> result = null;
            using (var context = new SPDataContext(SPDataContext.DBConnectionString))
            {
                result = context.Songs.ToList();
            }
        }

        public override void Update(AudioMedia entity)
        {
            using (var context = new SPDataContext(SPDataContext.DBConnectionString, true))
            {
                context.DeferredLoadingEnabled = false;
                var original = context.Songs.FirstOrDefault(x => x.Id == entity.Id);
                original.Title = entity.Title;
                original.AlbumId = entity.AlbumId;
                original.IsProtected = entity.IsProtected;
                original.FilesystemId = entity.FilesystemId;
                context.SubmitChanges();
            }
        }

        public override void Delete(AudioMedia entity)
        {
            using (var context = new SPDataContext(SPDataContext.DBConnectionString, true))
            {
                context.DeferredLoadingEnabled = false;
                var original = context.Songs.FirstOrDefault(x => x.Id == entity.Id);
                context.Songs.DeleteOnSubmit(original);
                context.SubmitChanges();
            }
        }
    }
}

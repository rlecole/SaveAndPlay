﻿#region Apache License, Version 2.0 
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
using System.Linq;
using System.Text;

namespace SaveAndPlay.DAL.Services
{
    public class AlbumDataService : BaseRequestDataService<Album>
    {
        public List<Album> SelectAll(AsyncCallback callback)
        {
            List<Album> result = null;
            using (var context = new SPDataContext(SPDataContext.DBConnectionString))
            {
                result = context.Albums.ToList();
            }
            return result;
        }

        public override void Update(Album entity)
        {
            using (var context = new SPDataContext(SPDataContext.DBConnectionString, true))
            {
                context.DeferredLoadingEnabled = false;
                var original = context.Albums.FirstOrDefault(x => x.Id == entity.Id);
                original.Title = entity.Title;
                context.SubmitChanges();
            }
        }

        public override void Delete(Album entity)
        {
            using (var context = new SPDataContext(SPDataContext.DBConnectionString, true))
            {
                context.DeferredLoadingEnabled = false;
                var original = context.Albums.FirstOrDefault(x => x.Id == entity.Id);
                context.Albums.DeleteOnSubmit(original);
                context.SubmitChanges();
            }
        }
    }
}

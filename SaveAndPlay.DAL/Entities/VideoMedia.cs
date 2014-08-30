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
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;

namespace SaveAndPlay.DAL.Entities
{
    [Table]
    public class VideoMedia : BaseEntity
    {
        public VideoMedia()
        {
            this.Visible = true;
        }

        private int id;
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id
        {
            get { return id; }
            set
            {
                if (id != value)
                {
                    OnNotifyPropertyChanging("Id");
                    id = value;
                }
            }
        }

        private string title;
        [Column(CanBeNull = false)]
        public string Title
        {
            get { return title; }
            set
            {
                if (title != value)
                {
                    OnNotifyPropertyChanging("Title");
                    title = value;
                }
            }
        }

        private string filesystemId;
        [Column(CanBeNull = false)]
        public string FilesystemId
        {
            get { return filesystemId; }
            set
            {
                if (filesystemId != value)
                {
                    OnNotifyPropertyChanging("FilesystemId");
                    filesystemId = value;
                }
            }
        }
        
        private bool isProtected;
        [Column(CanBeNull = false)]
        public bool IsProtected
        {
            get { return isProtected; }
            set
            {
                if (isProtected != value)
                {
                    OnNotifyPropertyChanging("IsProtected");
                    isProtected = value;
                }
            }
        }

        [Column(IsVersion = true)]
        private Binary _version;

        private bool visible;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        public override string ToString()
        {
            return this.title;
        }
    }
}

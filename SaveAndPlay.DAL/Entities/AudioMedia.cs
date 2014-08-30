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
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace SaveAndPlay.DAL.Entities
{
    [Table]
    public class AudioMedia : BaseEntity
    {
        public AudioMedia()
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

        private int albumId;
        [Column(CanBeNull = true)]
        public int AlbumId
        {
            get { return albumId; }
            set
            {
                if (albumId != value)
                {
                    OnNotifyPropertyChanging("AlbumId");
                    albumId = value;
                }
            }
        }

        private EntityRef<Album> albumRef = new EntityRef<Album>();
        [Association(Name = "FK_Album_AudioMedia", Storage = "albumRef", ThisKey = "AlbumId", OtherKey = "Id", IsForeignKey = true)]
        public Album Album
        {
            get { return this.albumRef.Entity; }
            set
            {
                Album previousValue = this.albumRef.Entity;

                if (previousValue != value || this.albumRef.HasLoadedOrAssignedValue == false)
                {
                    OnNotifyPropertyChanging("Album");
                    if (previousValue != null)
                    {
                        this.albumRef.Entity = null;
                        previousValue.Songs.Remove(this);
                    }

                    this.albumRef.Entity = (Album)value;

                    if (value != null)
                    {
                        value.Songs.Add(this);
                        this.AlbumId = value.Id;
                    }
                    else
                    {
                        this.AlbumId = default(int);
                    }
                }
            }
        }

        [Column(IsVersion = true)]
        private Binary _version;

        public string Image { get; set; }

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



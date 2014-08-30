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

namespace SaveAndPlay.DAL.Entities
{
    [Table]
    public class Album : BaseEntity
    {
        public Album()
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

        private int artistId;
        [Column(CanBeNull=true)]
        public int ArtistId
        {
            get { return artistId; }
            set
            {
                if (artistId != value)
                {
                    OnNotifyPropertyChanging("ArtistId");
                    artistId = value;
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

        private EntityRef<Artist> artistRef = new EntityRef<Artist>();
        [Association(Name = "FK_Artist_Album", Storage = "artistRef", ThisKey = "ArtistId", OtherKey = "Id", IsForeignKey = true)]
        public Artist Artist
        {
            get { return this.artistRef.Entity; }
            set
            {
                Artist previousValue = this.artistRef.Entity;

                if (previousValue != value || this.artistRef.HasLoadedOrAssignedValue == false)
                {
                    OnNotifyPropertyChanging("Artist");
                    if (previousValue != null)
                    {
                        this.artistRef.Entity = null;
                        previousValue.Albums.Remove(this);
                    }

                    this.artistRef.Entity = (Artist)value;

                    if (value != null)
                    {
                        value.Albums.Add(this);
                        this.ArtistId = value.Id;
                    }
                    else
                    {
                        this.ArtistId = default(int);
                    }
                }
            }
        }

        private readonly EntitySet<AudioMedia> audioMediasRef = new EntitySet<AudioMedia>();
        [Association(Name = "FK_Album_AudioMedia", Storage = "audioMediasRef", ThisKey = "Id", OtherKey = "AlbumId")]
        public IList<AudioMedia> Songs
        {
            get { return this.audioMediasRef; }
            set { this.audioMediasRef.Assign(value); }
        }

        public List<AudioMedia> SongsVisible
        {
            get
            {
                return this.audioMediasRef.Where(x => x.Visible).ToList();
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
            if (string.IsNullOrEmpty(title))
            {
                return "Undefined";
            }
            return this.title;
        }
    }
}

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
    public class Artist : BaseEntity
    {
        public Artist()
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

        private string name;
        [Column(CanBeNull = false)]
        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    OnNotifyPropertyChanging("Name");
                    name = value;
                }
            }
        }

        private readonly EntitySet<Album> albumsRef = new EntitySet<Album>();
        [Association(Name = "FK_Artist_Album", Storage = "albumsRef", ThisKey = "Id", OtherKey = "ArtistId")]
        public IList<Album> Albums
        {
            get { return this.albumsRef; }
            set { this.albumsRef.Assign(value); }
        }

        public List<Album> AlbumsVisible
        { 
            get
            { 
                return this.albumsRef.Where(x => x.Visible).ToList();
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
            if (string.IsNullOrEmpty(name))
            {
                return "Undefined";
            }
            return this.name;
        }
    }
}

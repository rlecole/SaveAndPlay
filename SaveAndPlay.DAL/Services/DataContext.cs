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

namespace SaveAndPlay.DAL.Services
{
    public class SPDataContext : DataContext
    {
        public static string DBConnectionString = "Data Source=isostore:/database.sdf";

        public SPDataContext(string connectionString, bool objectTrackingEnabled = false)
            : base(connectionString)
        {
            this.ObjectTrackingEnabled = objectTrackingEnabled;
            this.Songs = GetTable<AudioMedia>();
            this.Videos = GetTable<VideoMedia>();
            this.Albums = GetTable<Album>();
            this.Artists = GetTable<Artist>();
            this.Playlists = GetTable<Playlist>();
            this.PlaylistsAudioMedia = GetTable<PlaylistAudioMedia>();
        }

        public Table<AudioMedia> Songs { get; set; }
        public Table<VideoMedia> Videos { get; set; }
        public Table<Album> Albums { get; set; }
        public Table<Artist> Artists { get; set; }
        public Table<Playlist> Playlists { get; set; }
        public Table<PlaylistAudioMedia> PlaylistsAudioMedia { get; set; }
    }
}

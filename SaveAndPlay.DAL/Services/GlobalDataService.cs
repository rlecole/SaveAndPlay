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
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;

namespace SaveAndPlay.DAL.Services
{
    public class GlobalDataService
    {
        public class Entities
        {
            public List<VideoMedia> Videos { get; set; }
            public List<AudioMedia> Songs { get; set; }
            public List<Album> Albums { get; set; }
            public List<Artist> Artists { get; set; }
            public List<Playlist> Playlists { get; set; }
            public List<PlaylistAudioMedia> PlaylistsAudioMedia { get; set; }
        }

        public Entities SelectAll()
        {
            Entities result = new Entities();
            using (var context = new SPDataContext(SPDataContext.DBConnectionString))
            {
                result.Videos = context.Videos.Cast<VideoMedia>().ToList();
                result.Songs = context.Songs.Cast<AudioMedia>().ToList();
                result.Albums = context.Albums.Cast<Album>().ToList();
                result.Artists = context.Artists.Cast<Artist>().ToList();
                result.Playlists = context.Playlists.Cast<Playlist>().ToList();
                result.PlaylistsAudioMedia = context.PlaylistsAudioMedia.Cast<PlaylistAudioMedia>().ToList();
            }
            return result;
        }
    }
}

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

using Microsoft.Phone.BackgroundAudio;
using SaveAndPlay.DAL.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SaveAndPlay.DAL.Services
{
    public class PlaybackAgentDataService
    {
        public class PlaybackMedia
        {
            public string Title { get; set; }
            public string Album { get; set; }
            public string Artist { get; set; }
            public string FilePath { get; set; }
        }

        private const string PlaylistPath = "playlist.dat";

        public void BuildPlaylist(IEnumerable<AudioMedia> tracks, string undefinedLabel)
        {
            using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isolatedStorageFile.FileExists(PlaylistPath))
                {
                    isolatedStorageFile.DeleteFile(PlaylistPath);
                }
                using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(PlaylistPath, System.IO.FileMode.Create, isolatedStorageFile))
                {
                    var newTracks = tracks.Select(x => new PlaybackMedia()
                    {
                        Title = x.Title,
                        Artist = string.IsNullOrEmpty(x.Album.Artist.Name) ? undefinedLabel : x.Album.Artist.Name,
                        Album = string.IsNullOrEmpty(x.Album.Title) ? undefinedLabel : x.Album.Title,
                        FilePath = Path.Combine("medias", x.FilesystemId),
                    }).ToList();

                    DataContractSerializer serializer = new DataContractSerializer(typeof(List<PlaybackMedia>));
                    serializer.WriteObject(stream, newTracks);
                }
            }
        }

        public List<AudioTrack> GetPlaylist()
        {
            var songs = new List<PlaybackMedia>();

            using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isolatedStorageFile.FileExists(PlaylistPath))
                {
                    using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(PlaylistPath, System.IO.FileMode.Open, isolatedStorageFile))
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(List<PlaybackMedia>));
                        songs = (List<PlaybackMedia>)serializer.ReadObject(stream);
                    }
                }
            }
            return songs.Select(x => new AudioTrack(
                new Uri(x.FilePath, UriKind.RelativeOrAbsolute),
                x.Title,
                x.Artist,
                x.Album,
                new Uri("shared/media/application.png", UriKind.Relative),
                null,
                EnabledPlayerControls.All)).ToList();
        }
    }
}

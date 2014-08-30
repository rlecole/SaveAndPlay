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

using SaveAndPlay.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaveAndPlay.DAL.Services
{
    public abstract class BaseDataServiceScheduler
    {
        private static SchedulerService scheduler = new SchedulerService();
        public SchedulerService Scheduler
        {
            get { return scheduler; }
        }

        protected AudioMediaDataService audioMediaDataService;
        protected AudioMediaDataService AudioMediaDataService
        {
            get
            {
                if (audioMediaDataService == null)
                {
                    audioMediaDataService = new AudioMediaDataService();
                }
                return audioMediaDataService;
            }
        }

        protected AlbumDataService albumDataService;
        protected AlbumDataService AlbumDataService
        {
            get
            {
                if (albumDataService == null)
                {
                    albumDataService = new AlbumDataService();
                }
                return albumDataService;
            }
        }

        protected ArtistDataService artistDataService;
        protected ArtistDataService ArtistDataService
        {
            get
            {
                if (artistDataService == null)
                {
                    artistDataService = new ArtistDataService();
                }
                return artistDataService;
            }
        }

        protected PlaylistDataService playlistDataService;
        protected PlaylistDataService PlaylistDataService
        {
            get
            {
                if (playlistDataService == null)
                {
                    playlistDataService = new PlaylistDataService();
                }
                return playlistDataService;
            }
        }

        protected PlaylistAudioMediaDataService playlistAudioMediaDataService;
        protected PlaylistAudioMediaDataService PlaylistAudioMediaDataService
        {
            get
            {
                if (playlistAudioMediaDataService == null)
                {
                    playlistAudioMediaDataService = new PlaylistAudioMediaDataService();
                }
                return playlistAudioMediaDataService;
            }
        }

        protected VideoMediaDataService videoMediaDataService;
        protected VideoMediaDataService VideoMediaDataService
        {
            get
            {
                if (videoMediaDataService == null)
                {
                    videoMediaDataService = new VideoMediaDataService();
                }
                return videoMediaDataService;
            }
        }

        protected GlobalDataService globalDataService;
        protected GlobalDataService GlobalDataService
        {
            get
            {
                if (globalDataService == null)
                {
                    globalDataService = new GlobalDataService();
                }
                return globalDataService;
            }
        }

        public DataAsyncResult EnqueueRequest(TaskParam parameters, Func<object> operation)
        {
            return scheduler.EnqueueRequest(operation, parameters);
        }

        public void Register(IDataServiceSchedulerClient client)
        {
            scheduler.Register(client);
        }

        public void Unregister(IDataServiceSchedulerClient client)
        {
            scheduler.Unregister(client);
        }
    }
}

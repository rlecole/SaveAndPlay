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
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Runtime.Serialization;
using System.IO.IsolatedStorage;
using SaveAndPlay.Resources;

namespace SaveAndPlay.ViewService
{
    public class PlaybackAgentService : IPlaybackAgentService
    {
        private INavigationViewService navigationViewService;
        public INavigationViewService NavigationViewService
        {
            get
            {
                if (this.navigationViewService == null)
                {
                    this.navigationViewService = ServiceLocator.Current.GetInstance<INavigationViewService>();
                }
                return navigationViewService; 
            }
        }

        public void Play(AudioMedia audioMedia)
        {
            this.PlayInternal(new List<AudioMedia>() { audioMedia });
        }

        public void Play(Album album)
        {
            this.PlayInternal(album.Songs);
        }

        public void Play(Artist artist)
        {
            this.PlayInternal(artist.AlbumsVisible.SelectMany(x => x.SongsVisible));
        }

        public void Play(Playlist playlist)
        {
            this.PlayInternal(playlist.SongsVisible);
        }

        public void PlayRandom(List<AudioMedia> list)
        {
            this.PlayInternal(this.Shuffle(list));
        }

        public void Pause()
        {
            if (BackgroundAudioPlayer.Instance != null && BackgroundAudioPlayer.Instance.CanPause)
            {
                BackgroundAudioPlayer.Instance.Pause();
            }
        }

        public void Resume()
        {
            BackgroundAudioPlayer.Instance.Play();
        }

        public void Previous()
        {
            BackgroundAudioPlayer.Instance.SkipPrevious();
        }

        public void Next()
        {
            BackgroundAudioPlayer.Instance.SkipNext();
        }

        public void Clear()
        {
            if (BackgroundAudioPlayer.Instance != null)
            {
                if (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing ||
                    BackgroundAudioPlayer.Instance.PlayerState == PlayState.Paused)
                {
                    BackgroundAudioPlayer.Instance.Stop();
                }
                BackgroundAudioPlayer.Instance.Track = null;
                BackgroundAudioPlayer.Instance.Close();
            }
        }

        private void PlayInternal(IEnumerable<AudioMedia> tracks)
        {
            if (tracks.Count() > 0)
            {
                this.Clear();
                new SaveAndPlay.DAL.Services.PlaybackAgentDataService().BuildPlaylist(tracks, MusicList.UndefinedLabel);
                BackgroundAudioPlayer.Instance.Play();
                this.NavigationViewService.NavigateTo("/UI/Pages/AudioPlayerPage.xaml");
            }
        }


        public void SetPosition(TimeSpan timeSpan)
        {
            if (BackgroundAudioPlayer.Instance != null && BackgroundAudioPlayer.Instance.CanSeek)
            {
                BackgroundAudioPlayer.Instance.Position = timeSpan;
            }
        }

        public IList<AudioMedia> Shuffle(IList<AudioMedia> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                AudioMedia value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }
}

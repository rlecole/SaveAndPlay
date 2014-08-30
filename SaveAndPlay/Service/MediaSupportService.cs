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
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;

namespace SaveAndPlay.Service
{
    public class MediaSupportService : IMediaSupportService
    {
        private Media videoMedia = new Media(MediaType.Video, new string[] { "wmv", "asf", "mp4", "m4v" });
        private Media musicMedia = new Media(MediaType.Audio, new string[] { "mp3", "3gp", "3g2", "wma", "wav", "m4a" });

        public MediaType GetMediaType(string file)
        {
            MediaType type = MediaType.Unknown;

            string tmp = HttpUtility.UrlDecode(file);
            int index = tmp.LastIndexOf('?');
            if (index > 0)
            {
                tmp = tmp.Substring(0, index);
            }

            type = this.IsSupportedMedia(videoMedia, tmp);
            if (type == MediaType.Unknown)
            {
                type = this.IsSupportedMedia(musicMedia, tmp);
            }
            return type;
        }

        public string GetIcon(MediaType type)
        {
            string icon = string.Empty;
            switch (type)
            {
                case MediaType.Video:
                    icon = "/Images/video.png";
                    break;
                case MediaType.Audio:
                    icon = "/Images/music.png";
                    break;
            }
            return icon;
        }

        public string GetStorageDirectory()
        {
            return "medias";
        }

        private MediaType IsSupportedMedia(Media media, string file)
        {
            if (media.Extensions.Any(e => file.EndsWith(e)))
            {
                return media.Type;
            }
            return MediaType.Unknown;
        }
    }
}

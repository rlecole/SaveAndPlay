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
using Microsoft.Phone.Shell;

namespace SaveAndPlay.Resources
{
    public class Localisation
    {
        private static Main main = new Main();
        private static DownloadList downloadList = new DownloadList();
        private static MusicList musicList = new MusicList();
        private static VideoList videoList = new VideoList();
        private static NewDownload newDownload = new NewDownload();
        private static ProtectedMode protectedMode = new ProtectedMode();
        private static MediaSource mediaSource = new MediaSource();
        private static About about = new About();
        private static Login login = new Login();

        public static Main Main { get { return main; } }
        public static DownloadList DownloadList { get { return downloadList; } }
        public static MusicList MusicList { get { return musicList; } }
        public static VideoList VideoList { get { return videoList; } }
        public static NewDownload NewDownload { get { return newDownload; } }
        public static ProtectedMode ProtectedMode { get { return protectedMode; } }
        public static MediaSource MediaSource { get { return mediaSource; } }
        public static About About { get { return about; } }
        public static Login Login { get { return login; } }

        public static void ApplyLocalizationToApplicationBar<T>(T element) where T : IApplicationBarMenuItem
        {
            if (element != null && !string.IsNullOrEmpty(element.Text))
            {
                string tmp = element.Text;
                try
                {
                    if (tmp.StartsWith("AppBar_"))
                    {
                        element.Text = ApplicationBarMenus.ResourceManager.GetString(tmp);
                    }
                }
                catch
                {
                    element.Text = tmp;
                }
            }
        }
    }
}

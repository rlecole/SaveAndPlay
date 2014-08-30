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
using System.Collections.Generic;
using System.IO.IsolatedStorage;

namespace SaveAndPlay.Service
{
    public class ApplicationSettingsService : IApplicationSettingsService
    {
        public void ResetHosts()
        {
            IsolatedStorageSettings.ApplicationSettings["Hosts"] = null;
        }

        public VideoPosition GetVideoPosition()
        {
            VideoPosition position = null;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<VideoPosition>("VideoPosition", out position);
            return position;
        }

        public void SaveVideoPosition(VideoPosition position)
        {
            IsolatedStorageSettings.ApplicationSettings["VideoPosition"] = position;
        }

        public int GetDatabaseVersion()
        {
            int version = 0;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<int>("DatabaseVersion", out version);
            return version;
        }

        public void SetDatabaseVersion(int version)
        {
            IsolatedStorageSettings.ApplicationSettings["DatabaseVersion"] = version;
        }

        public bool GetIsOldMediaImported()
        {
            bool isOldMediaImported = false;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<bool>("IsOldMediaImported", out isOldMediaImported);
            return isOldMediaImported;
        }

        public void SetIsOldMediaImported(bool imported)
        {
            IsolatedStorageSettings.ApplicationSettings["IsOldMediaImported"] = imported;
        }

        public void AddFavorite(Favorite favorite)
        {
            List<Favorite> favorites = null;
            if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue<List<Favorite>>("Favorites", out favorites))
            {
                favorites = new List<Favorite>();
            }
            favorites.Add(favorite);
            IsolatedStorageSettings.ApplicationSettings["Favorites"] = favorites;
        }

        public void RemoveFavorite(Favorite favorite)
        {
            List<Favorite> favorites = null;
            if (IsolatedStorageSettings.ApplicationSettings.TryGetValue<List<Favorite>>("Favorites", out favorites))
            {
                favorites.Remove(favorite);
            }
        }

        public List<Favorite> GetFavorites()
        {
            List<Favorite> favorites = null;
            if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue<List<Favorite>>("Favorites", out favorites))
            {
                favorites = new List<Favorite>();
            }
            return favorites;
        }

        public void SetPassword(string password)
        {
            System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings["Password"] = password;
        }

        public bool ValidatePassword(string password)
        {
            string tmp = string.Empty;
            System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>("Password", out tmp);
            return password == tmp;
        }

        public bool IsDefinedPassword()
        {
            string password = string.Empty;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>("Password", out password);
            return !string.IsNullOrEmpty(password);
        }
    }
}

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
using System.IO.IsolatedStorage;

namespace SaveAndPlay.Service
{
    public class Favorite
    {
        public string Url { get; set; }
        public string Title { get; set; }
    }

    public class VideoPosition
    {
        public int Id { get; set; }
        public TimeSpan Position { get; set; }
    }

    public class ApplicationSettingsLoginHelper
    {
        private string key;

        public ApplicationSettingsLoginHelper(string key)
        {
            this.key = key;
        }

        public void SetLogin(string login)
        {
            System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings[key + "Login"] = login;
        }

        public string GetLogin()
        {
            string tmp = string.Empty;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>(key + "Login", out tmp);
            return tmp;
        }

        public void SetPassword(string password)
        {
            System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings[key + "Password"] = password;
        }

        public string GetPassword()
        {
            string tmp = string.Empty;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>(key + "Password", out tmp);
            return tmp;
        }

        public void ClearLoginInformation()
        {
            System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings[key + "Login"] = string.Empty;
            System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings[key + "Password"] = string.Empty;
        }
    }

    public interface IApplicationSettingsService
    {
        bool GetIsOldMediaImported();

        void SetIsOldMediaImported(bool imported);

        VideoPosition GetVideoPosition();

        void SaveVideoPosition(VideoPosition position);

        int GetDatabaseVersion();

        void SetDatabaseVersion(int version);

        void ResetHosts();

        void AddFavorite(Favorite favorite);

        void RemoveFavorite(Favorite favorite);

        List<Favorite> GetFavorites();

        void SetPassword(string password);

        bool ValidatePassword(string password);

        bool IsDefinedPassword();
    }
}

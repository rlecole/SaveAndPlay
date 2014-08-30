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

using GalaSoft.MvvmLight.Command;
using SaveAndPlay.Service;
using SaveAndPlay.UI.Pages;
using SaveAndPlay.ViewModels.Messages;
using System.Collections.ObjectModel;

namespace SaveAndPlay.ViewModels
{
    public class FavoritesViewModel : BaseViewModel
    {
        private ObservableCollection<Favorite> favorites;
        public ObservableCollection<Favorite> Favorites
        {
            get { return favorites; }
            set { favorites = value; base.RaisePropertyChanged("Favorites"); }
        }

        public bool NavigateToFavoriteInProgress { get; set; }

        public RelayCommand<Favorite> NavigateToFavorite { get; private set; }
        public RelayCommand<Favorite> RemoveFavorite { get; private set; }

        public FavoritesViewModel()
        {
            this.Update();
            
            this.NavigateToFavorite = new RelayCommand<Favorite>((favorite) =>
            {
                if (!this.NavigateToFavoriteInProgress)
                {
                    this.NavigateToFavoriteInProgress = true;
                    this.NavigationViewService.GoBack();
                    base.MessengerInstance.Send<Favorite, URLSelectorWebBrowserPage>(favorite);
                }
            });

            this.RemoveFavorite = new RelayCommand<Favorite>((favorite) =>
            {
                this.ApplicationSettingsService.RemoveFavorite(favorite);
                Update();
            });

            base.MessengerInstance.Register<Update>(this, (update) => { Update();});
        }

        private void Update()
        {
            this.Favorites = new ObservableCollection<Favorite>(this.ApplicationSettingsService.GetFavorites());
        }
    }
}

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

/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:SaveAndPlay"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.Service;
using GalaSoft.MvvmLight;
using System;

namespace SaveAndPlay.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                // Create design time view services and models
                if (!SimpleIoc.Default.IsRegistered<DownloadListViewModel>())
                {
                    SimpleIoc.Default.Register<DownloadListViewModel>(() =>
                    {
                        return new DownloadListViewModel()
                        {
                            NoRequest = false,
                            Requests = new System.Collections.ObjectModel.ObservableCollection<BaseDownloadItemViewModel>()
                            {
                                new DownloadItemForegroundViewModel(new ForegroundTransferRequest(new Uri("http://www.google.fr/testtesttesttest.wmv"))
                                {
                                    Tag = "testtesttesttesttesttesttesttest.wmv",
                                })
                                {
                                    TransferRate = 120d,
                                    Status = "Mode eco activé, en attente d'une connexion data 3G ou plus rapide",
                                    Progress = 86,
                                },
                            },
                        };
                    });
                }
            }
            else
            {
                // Create run time view services and models
                if (!SimpleIoc.Default.IsRegistered<MainViewModel>()) SimpleIoc.Default.Register<MainViewModel>();
                if (!SimpleIoc.Default.IsRegistered<DownloadListViewModel>()) SimpleIoc.Default.Register<DownloadListViewModel>();
                if (!SimpleIoc.Default.IsRegistered<FavoritesViewModel>()) SimpleIoc.Default.Register<FavoritesViewModel>();
                if (!SimpleIoc.Default.IsRegistered<NewDownloadViewModel>()) SimpleIoc.Default.Register<NewDownloadViewModel>();
                if (!SimpleIoc.Default.IsRegistered<SkydriveViewModel>()) SimpleIoc.Default.Register<SkydriveViewModel>();
                if (!SimpleIoc.Default.IsRegistered<DropboxViewModel>()) SimpleIoc.Default.Register<DropboxViewModel>();
                if (!SimpleIoc.Default.IsRegistered<AlbumViewModel>()) SimpleIoc.Default.Register<AlbumViewModel>();
                if (!SimpleIoc.Default.IsRegistered<ArtistViewModel>()) SimpleIoc.Default.Register<ArtistViewModel>();
                if (!SimpleIoc.Default.IsRegistered<AudioMediaViewModel>()) SimpleIoc.Default.Register<AudioMediaViewModel>();
                if (!SimpleIoc.Default.IsRegistered<VideoMediaViewModel>()) SimpleIoc.Default.Register<VideoMediaViewModel>();
                if (!SimpleIoc.Default.IsRegistered<PlaylistViewModel>()) SimpleIoc.Default.Register<PlaylistViewModel>();
                if (!SimpleIoc.Default.IsRegistered<MusicListViewModel>()) SimpleIoc.Default.Register<MusicListViewModel>();
                if (!SimpleIoc.Default.IsRegistered<VideoMediaViewModel>()) SimpleIoc.Default.Register<VideoMediaViewModel>();
                if (!SimpleIoc.Default.IsRegistered<VideoListViewModel>()) SimpleIoc.Default.Register<VideoListViewModel>();
                if (!SimpleIoc.Default.IsRegistered<ProtectedModeViewModel>()) SimpleIoc.Default.Register<ProtectedModeViewModel>();
                if (!SimpleIoc.Default.IsRegistered<ProtectedModeNewPasswordViewModel>()) SimpleIoc.Default.Register<ProtectedModeNewPasswordViewModel>();
            }
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public DownloadListViewModel Download
        {
            get
            {
                return ServiceLocator.Current.GetInstance<DownloadListViewModel>();
            }
        }

        public VideoListViewModel VideoList
        {
            get
            {
                return ServiceLocator.Current.GetInstance<VideoListViewModel>();
            }
        }

        public MusicListViewModel MusicList
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MusicListViewModel>();
            }
        }

        public FavoritesViewModel Favorites
        {
            get
            {
                return ServiceLocator.Current.GetInstance<FavoritesViewModel>();
            }
        }

        public NewDownloadViewModel NewDownload
        {
            get
            {
                return ServiceLocator.Current.GetInstance<NewDownloadViewModel>();
            }
        }

        public SkydriveViewModel Skydrive
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SkydriveViewModel>();
            }
        }

        public DropboxViewModel Dropbox
        {
            get
            {
                return ServiceLocator.Current.GetInstance<DropboxViewModel>();
            }
        }

        public AlbumViewModel Album
        {
            get
            {
                return ServiceLocator.Current.GetInstance<AlbumViewModel>();
            }
        }

        public ArtistViewModel Artist
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ArtistViewModel>();
            }
        }

        public AudioMediaViewModel AudioMedia
        {
            get
            {
                return ServiceLocator.Current.GetInstance<AudioMediaViewModel>();
            }
        }

        public VideoMediaViewModel VideoMedia
        {
            get
            {
                return ServiceLocator.Current.GetInstance<VideoMediaViewModel>();
            }
        }

        public PlaylistViewModel Playlist
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PlaylistViewModel>();
            }
        }

        public ProtectedModeViewModel ProtectedMode
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ProtectedModeViewModel>();
            }
        }

        public ProtectedModeNewPasswordViewModel ProtectedModeNewPassword
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ProtectedModeNewPasswordViewModel>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
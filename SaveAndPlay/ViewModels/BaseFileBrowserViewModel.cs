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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.Service;
using SaveAndPlay.UI.Controls.FileBrowserControlEntities;

namespace SaveAndPlay.ViewModels
{
    public abstract class BaseFileBrowserViewModel :  BaseViewModel
    {
        protected Stack<Folder> history = new Stack<Folder>();
        protected Folder targetFolder;
        protected Folder currentFolder;
        protected bool isBack = false;

        public RelayCommand<AudioFile> SelectAudio { get; set; }
        public RelayCommand<VideoFile> SelectVideo { get; set; }
        public RelayCommand<Folder> SelectFolder { get; set; }
        public abstract bool RunningSession { get; }

        public bool ChangingDirectory
        {
            get
            {
                return this.currentFolder != this.targetFolder;
            }
        }

        protected bool emptyFolder;
        public bool EmptyFolder
        {
            get { return emptyFolder; }
            set { emptyFolder = value; RaisePropertyChanged("EmptyFolder"); }
        }

        protected string root;
        public string Root
        {
            get { return root; }
            set { root = value; RaisePropertyChanged("Root"); }
        }

        protected ObservableCollection<BaseFile> data;
        public ObservableCollection<BaseFile> Data
        {
            get { return data; }
            set { data = value; RaisePropertyChanged("Data"); }
        }

        protected void HandleHistory()
        {
            if (!this.isBack)
            {
                if (this.targetFolder != null)
                {
                    this.history.Push(this.currentFolder);
                }
            }
            else
            {
                this.history.Pop();
            }
            this.currentFolder = this.targetFolder;
            this.isBack = false;
        }
    }
}

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
using System.IO.IsolatedStorage;
using SaveAndPlay.Resources;
using System.Windows.Resources;
using System.IO;

namespace SaveAndPlay.Helpers
{
    public class IsolatedStorageFileHelper
    {
        public static bool CheckIfFileAlreadyDownloading(string downloadFile)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isoStore.FileExists("shared/mtransfers/" + downloadFile) ||
                    isoStore.FileExists("shared/transfers/" + downloadFile))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CreateDirectoryIfNotExists(string path)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.DirectoryExists(path))
                {
                    isoStore.CreateDirectory(path);
                    return true;
                }
            }
            return false;
        }

        public static bool DeleteFileIfExists(string path)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isoStore.FileExists(path))
                {
                    isoStore.DeleteFile(path);
                    return true;
                }
            }
            return false;
        }

        public static bool RemoveDirectoryContent(string path)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isoStore.DirectoryExists(path))
                {
                    var files = isoStore.GetFileNames(path + "\\*");
                    foreach (var file in files)
                    {
                        isoStore.DeleteFile(path + "/" + file);
                    }
                    return true;
                }
            }
            return false;
        }

        public static bool MoveFile(string source, string destination)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isoStore.FileExists(source))
                {
                    if (isoStore.FileExists(destination))
                    {
                        isoStore.DeleteFile(destination);
                    }
                    isoStore.MoveFile(source, destination);
                    return true;
                }
            }
            return false;
        }

        public static void CopyIconToIsolatedStorage(string filePath)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(filePath))
                {
                    StreamResourceInfo resource = Application.GetResourceStream(new Uri("application.png", UriKind.Relative));

                    using (IsolatedStorageFileStream file = store.CreateFile(filePath))
                    {
                        int chunkSize = 4096;
                        byte[] bytes = new byte[chunkSize];
                        int byteCount;

                        while ((byteCount = resource.Stream.Read(bytes, 0, chunkSize)) > 0)
                        {
                            file.Write(bytes, 0, byteCount);
                        }
                    }
                }
            }
        }

        public static bool AnyDirectoryExists(params string[] paths)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                foreach (var path in paths)
                {
                    if (store.DirectoryExists(path))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

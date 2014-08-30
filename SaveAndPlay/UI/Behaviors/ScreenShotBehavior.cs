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

//using System.IO;
//using System.IO.IsolatedStorage;
//using System.Windows;
//using System.Windows.Interactivity;
//using System.Windows.Media.Imaging;
//using Microsoft.Phone.Controls;
//using Microsoft.Phone.Tasks;
//using Microsoft.Xna.Framework.Media;
//using System;

//namespace SaveAndPlay.UI.Behaviors
//{
//    public class ScreenShotBehavior : Behavior<PhoneApplicationPage>
//    {
//#if DEBUG
//        protected override void OnAttached()
//        {
//            base.OnAttached();

//            AssociatedObject.DoubleTap += AssociatedObject_DoubleTap;

//        }

//        void AssociatedObject_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
//        {

//            if (sender is PhoneApplicationPage)
//            {
//                var page = sender as PhoneApplicationPage;
//                var bmp = new WriteableBitmap((int)page.ActualWidth, (int)page.ActualHeight);
//                bmp.Render(page, null);
//                bmp.Invalidate();

//                string tempJpeg = "Screenshot " + DateTime.Now.ToString("MM.dd.yy H-mm-ss") + ".jpg";

//                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
//                {
//                    if (isoStore.FileExists(tempJpeg))
//                    {
//                        isoStore.DeleteFile(tempJpeg);
//                    }

//                    using (var myFileStream = isoStore.CreateFile(tempJpeg))
//                    {
//                        bmp.SaveJpeg(myFileStream, bmp.PixelWidth, bmp.PixelHeight, 0, 85);
//                    }
//                }
//            }
//        }

//        protected override void OnDetaching()
//        {
//            base.OnDetaching();
//            AssociatedObject.DoubleTap -= AssociatedObject_DoubleTap;
//        }
//#endif
//    }
//}
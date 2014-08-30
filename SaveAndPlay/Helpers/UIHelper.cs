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
using System.Threading;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Media;

namespace SaveAndPlay.Helpers
{
    public class UIHelper
    {
        public static void DelayUIInvocation(int delay, Action action)
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Thread.Sleep(delay);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    action();
                });
            });
        }

        public static string GetEnumLocalized<TEnum, TResources>(TEnum item) where TEnum : struct, IConvertible
        {
            string name = typeof(TEnum).Name;
            string enumStr = Enum.GetName(typeof(TEnum), item);

            if (!string.IsNullOrEmpty(enumStr))
            {
                var property = typeof(TResources).GetProperty("Enum_" + name + "_" + enumStr);
                if (property != null)
                {
                    return property.GetValue(null, null).ToString();
                }
            }

            return string.Empty;
        }

        public static T GetParent<T>(DependencyObject o) where T : FrameworkElement
        {
            var parent = VisualTreeHelper.GetParent(o);
            if (parent != null)
            {
                if (parent is T)
                {
                    return (T)parent;
                }
                else
                {
                    return GetParent<T>(parent);
                }
            }
            return null;
        }

        public static string FormatTimeSpan(TimeSpan timeSpan)
        {
            return string.Format("{0}:{1}", Math.Floor(timeSpan.TotalMinutes).ToString("00"),
                                                       timeSpan.Seconds.ToString("00"));
        }
    }
}

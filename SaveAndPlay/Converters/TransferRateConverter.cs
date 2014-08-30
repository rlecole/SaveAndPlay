﻿#region Apache License, Version 2.0 
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
using System.Windows.Data;
using System.Globalization;
using SaveAndPlay.Resources;

namespace SaveAndPlay.Converters
{
    public class TransferRateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;

            if (value != null && typeof(double) == value.GetType())
            {
                double? rate = (double?)value;
                if (rate.HasValue)
                {
                    if (rate.Value < 1024)
                    {
                        result = rate.Value.ToString("0.00") + " " + Main.BytesPerSecond;
                    }
                    else if (rate < 1048576)
                    {
                        result = (rate.Value / 1024).ToString("0.00") + " " + Main.KiloBytesPerSecond;
                    }
                    else
                    {
                        result = (rate.Value / 1048576).ToString("0.00") + " " + Main.MegaBytesPerSecond;
                    }
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

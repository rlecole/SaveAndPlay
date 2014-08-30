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
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Practices.ServiceLocation;
using SaveAndPlay.Resources;
using SaveAndPlay.ViewModels;
using WP7_Barcode_Library;

namespace SaveAndPlay.UI.Pages
{
    public partial class URLSelectorQRCodePage : BasePage
    {
        public URLSelectorQRCodePage()
        {
            InitializeComponent();
            base.baseProgressOverlay = this.progressOverlay;
            this.progressOverlay.Hide();
        }

        private void Validate_Click(object sender, EventArgs e)
        {
            ((NewDownloadViewModel)this.DataContext).URLValidation(this.QRCodeUrl.Text, "/UI/Pages/AddDownloadPage.xaml");
        }

        private void Scan_Click(object sender, EventArgs e)
        {
            try
            {
                WP7BarcodeManager.ScanMode = com.google.zxing.BarcodeFormat.QR_CODE;
                WP7BarcodeManager.ScanBarcode(BarcodeResults_Finished);
            }
            catch
            {
                MessageBox.Show(NewDownload.QRCodeFailed, NewDownload.Error, MessageBoxButton.OK);
            }
        }

        public void BarcodeResults_Finished(BarcodeCaptureResult BCResults)
        {
            try
            {
                if (WP7BarcodeManager.LastCaptureResults.BarcodeImage != null)
                {
                    this.QRCodeImage.Source = WP7BarcodeManager.LastCaptureResults.BarcodeImage; //Display image
                }
                if (BCResults.State == WP7_Barcode_Library.CaptureState.Success)
                {
                    ServiceLocator.Current.GetInstance<NewDownloadViewModel>().URL = BCResults.BarcodeText;
                }
                else
                {
                    MessageBox.Show(NewDownload.QRCodeFailed, NewDownload.Error, MessageBoxButton.OK);
                }
            }
            catch (Exception)
            {
                MessageBox.Show(NewDownload.QRCodeFailed, NewDownload.Error, MessageBoxButton.OK);
            }
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((NewDownloadViewModel)this.DataContext).StopURLValidation();
        }
    }
}
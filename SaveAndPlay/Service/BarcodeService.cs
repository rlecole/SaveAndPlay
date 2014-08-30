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
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using com.google.zxing;

namespace SaveAndPlay.Service
{
    public class BarcodeService : IBarcodeService
    {
        public bool TryToRecognizeBarcode(WriteableBitmap wb, out string barCode)
        {
            // set some recognition settings
            var zxhints = new Dictionary<object, object>() 
                { 
                    { DecodeHintType.TRY_HARDER, true }, 
                    { DecodeHintType.POSSIBLE_FORMATS, new List<Object>() { BarcodeFormat.UPC_A, BarcodeFormat.UPC_E } } 
                };

            // create reader instance
            var reader = new com.google.zxing.oned.MultiFormatUPCEANReader(zxhints);
            return TryToRecognize(wb, reader, zxhints, out barCode);
        }

        public bool TryToRecognizeQRcode(WriteableBitmap wb, out string qrCode)
        {
            // create reader instance
            var reader = new com.google.zxing.qrcode.QRCodeReader();
            return TryToRecognize(wb, reader, null, out qrCode);
        }

        public bool TryToRecognizeDataMatrix(WriteableBitmap wb, out string dtm)
        {
            // create reader instance
            var reader = new com.google.zxing.datamatrix.DataMatrixReader();
            return TryToRecognize(wb, reader, null, out dtm);
        }

        private bool TryToRecognize(WriteableBitmap wb, Reader reader, Dictionary<object, object> zxhints, out string output)
        {
            bool res = false;
            output = null;
            try
            {
                var luminiance = new RGBLuminanceSource(wb, wb.PixelWidth, wb.PixelHeight);
                var binarizer = new com.google.zxing.common.HybridBinarizer(luminiance);
                var binBitmap = new com.google.zxing.BinaryBitmap(binarizer);

                // recognize
                var results = reader.decode(binBitmap, zxhints); // exception will be thrown if reader cannot decode image.

                output = results.Text;
                res = true;
            }
            catch {}
            return res;
        }
    }
}

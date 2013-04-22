/*
 * Copyright (C) 2013 Tomasz Cielecki (Twitter: @Cheesebaron)
 * Copyright (C) 2013 James Montemagno (Twitter: @JamesMontemagno)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.IO;
using Android.Graphics;

namespace SignatureCapture.Library
{
    public static class SignatureCaptureUtils
    {
        /// <summary>
        /// Saves the current signature out to the SD card
        /// </summary>
        /// <param name="capture">capture to save out</param>
        /// <param name="folderName">What folder to put it in</param>
        /// <param name="fileName">Name of the file (.png will be added)</param>
        /// <returns></returns>
        public static bool SaveToPicturesFolder(SignatureCaptureView capture, string folderName = null, string fileName = null)
        {
            try
            {
                var path =
                    Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);

                var folder = path .AbsolutePath +
                            Java.IO.File.Separator +
                            (string.IsNullOrWhiteSpace(folderName) ? "MySignatures" : folderName);
                var extFileName = folder +
                                  Java.IO.File.Separator +
                                  (string.IsNullOrWhiteSpace(fileName) ? DateTime.Now.ToString("yyyyMMdd_HHmmss") : fileName) + ".png";

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                using (var fs = new FileStream(extFileName, FileMode.OpenOrCreate))
                {
                    capture.CanvasBitmap.Compress(Bitmap.CompressFormat.Png, 100, fs);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Saves the current signature out to the SD card
        /// </summary>
        /// <param name="capture">capture to save out</param>
        /// <param name="folderName">What folder to put it in</param>
        /// <param name="fileName">Name of the file (.png will be added)</param>
        /// <returns></returns>
        public static bool SaveToSd(SignatureCaptureView capture, string folderName = null, string fileName = null)
        {
            try
            {
                var folder = Android.OS.Environment.ExternalStorageDirectory + 
                            Java.IO.File.Separator + 
                            (string.IsNullOrWhiteSpace(folderName) ? "MySignatures" : folderName);
                var extFileName = folder +
                                  Java.IO.File.Separator +
                                  (string.IsNullOrWhiteSpace(fileName) ? DateTime.Now.ToString("yyyyMMdd_HHmmss") : fileName) + ".png";

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                using (var fs = new FileStream(extFileName, FileMode.OpenOrCreate))
                {
                    capture.CanvasBitmap.Compress(Bitmap.CompressFormat.Png, 100, fs);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
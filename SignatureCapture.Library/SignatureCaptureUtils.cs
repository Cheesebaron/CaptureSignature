using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace SignatureCapture.Library
{
    public static class SignatureCaptureUtils
    {
        /// <summary>
        /// Saves the signature to the SDCard
        /// </summary>
        /// 
        public static bool SaveToSd(SignatureCaptureView capture, string folderName, string fileName)
        {
            var folder = Android.OS.Environment.ExternalStorageDirectory + Java.IO.File.Separator + "MySignatures";
            var extFileName = folder +
                              Java.IO.File.Separator +
                              Guid.NewGuid() + ".png";

            try
            {
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
/*
 * Origins from FingerPaint.java from the Android SDK API Samples and has been modified
 * to be able to save canvas to SDcard and set stroke width of paint.
 * 
 * Copyright (C) 2007 The Android Open Source Project
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

//#define TESTSAVE

using System;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.OS;
using Android.Widget;
using SignatureCapture.Library;

namespace SignatureCapture.Sample
{
    [Activity(Label = "Signature Capture", MainLauncher = true, Icon = "@drawable/ic_launcher")]
    public class CaptureSignatureActivity : Activity
    {
        private Library.SignatureCaptureView m_SignatureCapture;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.main);

            m_SignatureCapture = FindViewById<Library.SignatureCaptureView>(Resource.Id.capture_view);

#if TESTSAVE
            var saveState = LastNonConfigurationInstance as SaveData;
            if (saveState != null)
            {
                m_SignatureCapture.LoadSaveData(saveState);
            }
#endif
        }


#if TESTSAVE
        public override Java.Lang.Object OnRetainNonConfigurationInstance()
        {
            return m_SignatureCapture.CaptureSave;
        }
#endif

        public void ColorChanged(Color color)
        {
            m_SignatureCapture.StrokeColor = color;
        }

        private const int ColorMenuID = Menu.First;
        private const int SelectStrokeWidthMenuID = Menu.First + 1;
        private const int SelectStrokeStyleID = Menu.First + 2;
        private const int SelectStrokeCapID = Menu.First + 3;
        private const int SelectStrokeJoinID = Menu.First + 4;
        private const int ClearCanvasMenuID = Menu.First + 5;
        private const int SaveToSDCardID = Menu.First + 6;
        private const int SaveToPictureFolderID = Menu.First + 7;


        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            menu.Add(0, ColorMenuID, 0, "Select Color").SetShortcut('3', 'c');
            menu.Add(0, SelectStrokeWidthMenuID, 0, "Select Stroke Width").SetShortcut('4', 'w');
            menu.Add(0, SelectStrokeStyleID, 0, "Select Stroke Style").SetShortcut('5', 't');
            menu.Add(0, SelectStrokeCapID, 0, "Select Stroke Cap").SetShortcut('6', 'a');
            menu.Add(0, SelectStrokeJoinID, 0, "Select Stroke Join").SetShortcut('7', 'j');
            menu.Add(0, ClearCanvasMenuID, 0, "Clear Canvas").SetShortcut('8', 'e');
            menu.Add(0, SaveToSDCardID, 0, "Save to External Storage").SetShortcut('9', 's');
            return true;
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {


            switch (item.ItemId)
            {
                case ColorMenuID:
                    var cPicker = new ColorPickerDialog(this, m_SignatureCapture.StrokeColor);
                    cPicker.ColorChanged += delegate(object sender, ColorChangedEventArgs args)
                                            {
                                                m_SignatureCapture.StrokeColor = args.Color;
                                            };
                    cPicker.Show();
                    return true;
                case ClearCanvasMenuID:
                    m_SignatureCapture.ClearCanvas();
                    return true;
                case SaveToSDCardID:
                    SaveToSd();
                    return true;
                case SelectStrokeWidthMenuID:
                    StrokeWidthDialog();
                    return true;
                case SelectStrokeJoinID:
                    StrokeJoinDialog();
                    return true;
                case SelectStrokeStyleID:
                    StrokeStyleDialog();
                    return true;
                case SelectStrokeCapID:
                    StrokeCapsDialog();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void StrokeWidthDialog()
        {
            var dialog = new Dialog(this);
            dialog.SetTitle("Stroke Width");
            dialog.SetContentView(Resource.Layout.stroke_width);
            var theSpinner = (Spinner) dialog.FindViewById(Resource.Id.StrokeWidth);
            theSpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, Resources.GetStringArray(Resource.Array.stroke_widths));
            dialog.FindViewById(Resource.Id.StrokeOkButton).Click += (sender, args) =>
                {
                    var strokeWidth = theSpinner.GetItemAtPosition(theSpinner.SelectedItemPosition);
                    m_SignatureCapture.StrokeWidth = int.Parse(strokeWidth.ToString());
                    dialog.Dismiss();
                };
            dialog.Show();
        }

         private void StrokeCapsDialog()
        {
            var dialog = new Dialog(this);
            dialog.SetTitle("Stroke Cap");
            dialog.SetContentView(Resource.Layout.stroke_width);
            var theSpinner = (Spinner) dialog.FindViewById(Resource.Id.StrokeWidth);
            theSpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, Resources.GetStringArray(Resource.Array.stroke_caps));
            dialog.FindViewById(Resource.Id.StrokeOkButton).Click += (sender, args) =>
                {
                    var cap = theSpinner.GetItemAtPosition(theSpinner.SelectedItemPosition).ToString();
                    switch (cap)
                    {
                        case "round":
                            m_SignatureCapture.StrokeCap = StrokeCap.Round;
                            break;
                        case "butt":
                            m_SignatureCapture.StrokeCap = StrokeCap.Butt;
                            break;
                        case "square":
                            m_SignatureCapture.StrokeCap = StrokeCap.Square;
                            break;
                    }
                    dialog.Dismiss();
                };
            dialog.Show();
        }

         private void StrokeStyleDialog()
         {
             var dialog = new Dialog(this);
             dialog.SetTitle("Stroke Style");
             dialog.SetContentView(Resource.Layout.stroke_width);
             var theSpinner = (Spinner)dialog.FindViewById(Resource.Id.StrokeWidth);
             theSpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, Resources.GetStringArray(Resource.Array.stroke_styles));
             dialog.FindViewById(Resource.Id.StrokeOkButton).Click += (sender, args) =>
                 {
                     var cap = theSpinner.GetItemAtPosition(theSpinner.SelectedItemPosition).ToString();
                     switch (cap)
                     {
                         case "stroke":
                             m_SignatureCapture.StrokeStyle = StrokeStyle.Stroke;
                             break;
                         case "fill":
                             m_SignatureCapture.StrokeStyle = StrokeStyle.Fill;
                             break;
                         case "fill+stroke":
                             m_SignatureCapture.StrokeStyle = StrokeStyle.FillAndStroke;
                             break;
                     }
                     dialog.Dismiss();
                 };
             dialog.Show();
         }

         private void StrokeJoinDialog()
         {
             var dialog = new Dialog(this);
             dialog.SetTitle("Stroke Join");
             dialog.SetContentView(Resource.Layout.stroke_width);
             var theSpinner = (Spinner)dialog.FindViewById(Resource.Id.StrokeWidth);
             theSpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, Resources.GetStringArray(Resource.Array.stroke_joins));
            
             dialog.FindViewById(Resource.Id.StrokeOkButton).Click += (sender, args) =>
                 {
                     var cap = theSpinner.GetItemAtPosition(theSpinner.SelectedItemPosition).ToString();
                     switch (cap)
                     {
                         case "round":
                             m_SignatureCapture.StrokeJoin = StrokeJoin.Round;
                             break;
                         case "miter":
                             m_SignatureCapture.StrokeJoin = StrokeJoin.Miter;
                             break;
                         case "bevel":
                             m_SignatureCapture.StrokeJoin = StrokeJoin.Bevel;
                             break;
                     }
                     dialog.Dismiss();
                 };
             dialog.Show();
         }

        /// <summary>
        /// Saves the signature to the SDCard
        /// </summary>
        private void SaveToSd()
        {
            bool success = SignatureCapture.Library.SignatureCaptureUtils.SaveToSd(m_SignatureCapture, "Signature Capture", null);

            if (!success)
            {
                RunOnUiThread(() =>
                {
                    var builder = new AlertDialog.Builder(this);
                    builder.SetMessage("Saving signature went wrong");
                    builder.SetTitle("Unable to save signature");
                    builder.Show();
                });
            }

        }
    }
}


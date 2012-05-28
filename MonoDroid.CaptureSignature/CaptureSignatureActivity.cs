/*
 * Origins from FingerPaint.java from the Android SDK API Samples and has been modified
 * to be able to save canvas to SDcard and set stroke width of paint.
 * 
 * Copyright (C) 2007 The Android Open Source Project
 * Copyright (C) 2012 Tomasz Cielecki (tomasz@ostebaronen.dk)
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
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.OS;
using Android.Widget;

namespace MonoDroid.CaptureSignature
{
    [Activity(Label = "Capture Signature", MainLauncher = true, Icon = "@drawable/icon")]
    public class CaptureSignatureActivity : Activity
    {
        private CaptureSignatureView _capture;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            _capture = new CaptureSignatureView(this);
            SetContentView(_capture);

            _paint = new Paint
                     {
                         AntiAlias = true,
                         Dither = true,
                         Color = Color.Argb(255, 0, 0, 0)
                     };
            _paint.SetStyle(Paint.Style.Stroke);
            _paint.StrokeJoin = Paint.Join.Round;
            _paint.StrokeCap = Paint.Cap.Round;
            _paint.StrokeWidth = 12;
        }

        public void ColorChanged(Color color)
        {
            _paint.Color = color;
        }

        private static Paint _paint;

        public class CaptureSignatureView : View
        {
            private Bitmap _bitmap;
            private Canvas _canvas;
            private readonly Android.Graphics.Path _path;
            private readonly Paint _bitmapPaint;

            public CaptureSignatureView(Context c) : base(c)
            {
                _path = new Android.Graphics.Path();
                _bitmapPaint = new Paint(PaintFlags.Dither);
            }

            protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
            {
                base.OnSizeChanged(w, h, oldw, oldh);
                _bitmap = Bitmap.CreateBitmap(w, h, Bitmap.Config.Argb8888);
                _canvas = new Canvas(_bitmap);
            }

            protected override void OnDraw(Canvas canvas)
            {
                canvas.DrawColor(Color.White);

                canvas.DrawBitmap(_bitmap, 0, 0, _bitmapPaint);

                canvas.DrawPath(_path, _paint);
            }

            private float _mX, _mY;
            private const float TouchTolerance = 4;

            private void TouchStart(float x, float y)
            {
                _path.Reset();
                _path.MoveTo(x, y);
                _mX = x;
                _mY = y;
            }

            private void TouchMove(float x, float y)
            {
                float dx = Math.Abs(x - _mX);
                float dy = Math.Abs(y - _mY);

                if (dx >= TouchTolerance || dy >= TouchTolerance)
                {
                    _path.QuadTo(_mX, _mY, (x + _mX) / 2, (y + _mY) / 2);
                    _mX = x;
                    _mY = y;
                }
            }

            private void TouchUp()
            {
                _path.LineTo(_mX, _mY);
                _canvas.DrawPath(_path, _paint);

                _path.Reset();
            }

            public override bool OnTouchEvent(MotionEvent e)
            {
                var x = e.GetX();
                var y = e.GetY();

                switch (e.Action)
                {
                    case MotionEventActions.Down:
                        TouchStart(x, y);
                        Invalidate();
                        break;
                    case MotionEventActions.Move:
                        TouchMove(x, y);
                        Invalidate();
                        break;
                    case MotionEventActions.Up:
                        TouchUp();
                        Invalidate();
                        break;
                }
                return true;
            }

            public void ClearCanvas()
            {
                _canvas.DrawColor(Color.White);
                Invalidate();
            }

            public Bitmap CanvasBitmap()
            {
                return _bitmap;
            }
        }

        private const int ColorMenuID = Menu.First;
        private const int SelectStrokeWidthMenuID = Menu.First + 1;
        private const int ClearCanvasMenuID = Menu.First + 2;
        private const int SaveToSDCardID = Menu.First + 3;

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            menu.Add(0, ColorMenuID, 0, "Select Color").SetShortcut('3', 'c');
            menu.Add(0, SelectStrokeWidthMenuID, 0, "Select Stroke Width").SetShortcut('4', 'w');
            menu.Add(0, ClearCanvasMenuID, 0, "Clear Canvas").SetShortcut('5', 'e');
            menu.Add(0, SaveToSDCardID, 0, "Save to External Storage").SetShortcut('6', 's');

            return true;
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            _paint.SetXfermode(null);
            _paint.Alpha = 255;

            switch (item.ItemId)
            {
                case ColorMenuID:
                    var cPicker = new ColorPickerDialog(this, _paint.Color);
                    cPicker.ColorChanged += delegate(object sender, ColorChangedEventArgs args)
                                            {
                                                _paint.Color = args.Color;
                                            };
                    cPicker.Show();
                    return true;
                case ClearCanvasMenuID:
                    _capture.ClearCanvas();
                    return true;
                case SaveToSDCardID:
                    SaveToSd();
                    return true;
                case SelectStrokeWidthMenuID:
                    StrokeDialog();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void StrokeDialog()
        {
            var dialog = new Dialog(this);
            dialog.SetTitle("Stroke width");
            dialog.SetContentView(Resource.Layout.Main);
            ((Spinner)dialog.FindViewById(Resource.Id.StrokeWidth)).ItemSelected += delegate(object sender, AdapterView.ItemSelectedEventArgs args)
                                    {
                                        var spinner = (Spinner)sender;
                                        var strokeWidth = spinner.GetItemAtPosition(args.Position);
                                        _paint.StrokeWidth = int.Parse(strokeWidth.ToString());
                                    };
            dialog.FindViewById(Resource.Id.StrokeOkButton).Click += (sender, args) => dialog.Dismiss();
            dialog.Show();
        }

        /// <summary>
        /// Saves the signature to the SDCard
        /// </summary>
        private void SaveToSd()
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
                    _capture.CanvasBitmap().Compress(Bitmap.CompressFormat.Png, 100, fs);
                }    
            }
            catch
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


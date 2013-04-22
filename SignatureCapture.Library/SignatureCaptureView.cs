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
//#define TESTSAVE

using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;

namespace SignatureCapture.Library
{
    public enum StrokeStyle
    {
        Stroke = 0,
        Fill = 1,
        FillAndStroke = 2
    }

    public enum StrokeJoin
    {
        Round = 0,
        Miter = 1,
        Bevel = 2
    }

    public enum StrokeCap
    {
        Round = 0,
        Butt = 1,
        Square = 2
    }

#if TESTSAVE
    public class SaveData : Java.Lang.Object
    {
        public List<Paint> Paints { get; set; }
        public List<Path> Paths { get; set; }
    }
#endif
    

    public class SignatureCaptureView : View
    {
        

        private Canvas _canvas;
        private Path _path;
        private Paint _bitmapPaint;
        private Paint _paint;
#if TESTSAVE
        public SaveData CaptureSave { get; protected set; }
        private SaveData m_ToLoad;
#endif

        private float _mX, _mY;
        private const float TouchTolerance = 4;

        public SignatureCaptureView(Context context, IAttributeSet attrs, int defStyle)
        : base(context, attrs, defStyle)
        {
            var a = context.ObtainStyledAttributes(attrs, Resource.Styleable.signaturecapture);
            Init(a);
        }

        public SignatureCaptureView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            var a = context.ObtainStyledAttributes(attrs, Resource.Styleable.signaturecapture);
            Init(a);

        }

        public SignatureCaptureView(Context c)
            : base(c)
        {
            Init(null);
        }

        private void Init(Android.Content.Res.TypedArray attributes)
        {
            ResourceIdManager.UpdateIdValues();

#if TESTSAVE
            CaptureSave = new SaveData();
            CaptureSave.Paints = new List<Paint>();
            CaptureSave.Paths = new List<Path>();
#endif

            _path = new Path();
            _bitmapPaint = new Paint(PaintFlags.Dither);
            _paint = new Paint
                {
                    AntiAlias = true,
                    Dither = true,
                    StrokeJoin = Paint.Join.Round,
                    StrokeCap = Paint.Cap.Round,
                    StrokeWidth = 12,
                };

            _paint.SetStyle(Paint.Style.Stroke);

            BackgroundColor = Resources.GetColor(Resource.Color.signaturecapture_white);
            StrokeColor = Resources.GetColor(Resource.Color.signaturecapture_black);
            if (attributes == null)
            {
                return;
            }

            //If attributes exist then let's go ahead and set them up
            StrokeWidth = attributes.GetFloat(Resource.Styleable.signaturecapture_stroke_width, 12.0f);
            StrokeColor = attributes.GetColor(Resource.Styleable.signaturecapture_paint_color, StrokeColor);
            BackgroundColor = attributes.GetColor(Resource.Styleable.signaturecapture_background_color, BackgroundColor);
            StrokeJoin = (StrokeJoin)attributes.GetInt(Resource.Styleable.signaturecapture_stroke_join, 0);
            StrokeStyle = (StrokeStyle)attributes.GetInt(Resource.Styleable.signaturecapture_stroke_style, 0);
            StrokeCap = (StrokeCap)attributes.GetInt(Resource.Styleable.signaturecapture_stroke_cap, 0);
            attributes.Recycle();
        }


        #region Properties

        /// <summary>
        /// Gets or sets the stroke style
        /// </summary>
        public StrokeStyle StrokeStyle
        {
            get
            {
                var style = _paint.GetStyle();
                if (style == Paint.Style.Fill)
                    return StrokeStyle.Fill;

                if (style == Paint.Style.FillAndStroke)
                    return StrokeStyle.FillAndStroke;

                if (style == Paint.Style.Stroke)
                    return StrokeStyle.Stroke;

                return StrokeStyle.Stroke;
            }
            set
            {
                switch (value)
                {
                    case StrokeStyle.Fill:
                        _paint.SetStyle(Paint.Style.Fill);
                        break;
                    case StrokeStyle.FillAndStroke:
                        _paint.SetStyle(Paint.Style.FillAndStroke);
                        break;
                    case StrokeStyle.Stroke:
                        _paint.SetStyle(Paint.Style.Stroke);
                        break;
                    default:
                        _paint.SetStyle(Paint.Style.Stroke);
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the stroke join
        /// </summary>
        public StrokeJoin StrokeJoin
        {
            get
            {
                if (_paint.StrokeJoin == Paint.Join.Round)
                    return StrokeJoin.Round;

                if (_paint.StrokeJoin == Paint.Join.Miter)
                    return StrokeJoin.Miter;

                if (_paint.StrokeJoin == Paint.Join.Bevel)
                    return StrokeJoin.Bevel;

                return StrokeJoin.Round;
            }
            set
            {
                switch (value)
                {
                    case StrokeJoin.Bevel:
                        _paint.StrokeJoin = Paint.Join.Bevel;
                        break;
                    case StrokeJoin.Round:
                        _paint.StrokeJoin = Paint.Join.Round;
                        break;
                    case StrokeJoin.Miter:
                        _paint.StrokeJoin = Paint.Join.Miter;
                        break;
                    default:
                        _paint.StrokeJoin = Paint.Join.Round;
                        break;
                }
            }
        }

        /// <summary>
        /// gets or sets the stroke cap
        /// </summary>
        public StrokeCap StrokeCap
        {
            get
            {
                if(_paint.StrokeCap == Paint.Cap.Round)
                    return StrokeCap.Round;
                
                if(_paint.StrokeCap == Paint.Cap.Square)
                    return StrokeCap.Square;

                if (_paint.StrokeCap == Paint.Cap.Butt)
                    return StrokeCap.Butt;

                return StrokeCap.Round;
            }
            set
            {
                switch (value)
                {
                    case StrokeCap.Butt:
                        _paint.StrokeCap = Paint.Cap.Butt;
                        break;
                    case StrokeCap.Round:
                        _paint.StrokeCap = Paint.Cap.Round;
                        break;
                    case StrokeCap.Square:
                        _paint.StrokeCap = Paint.Cap.Square;
                        break;
                    default:
                        _paint.StrokeCap = Paint.Cap.Round;
                        break;
                }
            }
        }

     

           public Bitmap CanvasBitmap { get; private set; }

        /// <summary>
        /// gets or sets the color of the stroke
        /// </summary>
        public Color StrokeColor
        {
            get { return _paint.Color; }
            set
            {
                _paint.Color = value;
            }
        }

        /// <summary>
        /// gets or sets the color of the stroke
        /// </summary>
        public Color BackgroundColor
        {
            get { return _bitmapPaint.Color; }
            set
            {
                _bitmapPaint.Color = value;
            }
        }

        /// <summary>
        /// Gets or sets the stroke width
        /// Will set to 1 if less than 1
        /// </summary>
        public float StrokeWidth
        {
            get { return _paint.StrokeWidth; }
            set
            {
                _paint.StrokeWidth = value;

                if (_paint.StrokeWidth < 1)
                    _paint.StrokeWidth = 1;
            }
        }

        #endregion

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            CanvasBitmap = Bitmap.CreateBitmap(w, h, Bitmap.Config.Argb8888);
            _canvas = new Canvas(CanvasBitmap);

#if TESTSAVE
            if (m_ToLoad != null)
            {
                LoadData(m_ToLoad);
                CaptureSave = m_ToLoad;
                m_ToLoad = null;
            }
#endif
        }

        protected override void OnDraw(Canvas canvas)
        {
            canvas.DrawColor(BackgroundColor);
            canvas.DrawBitmap(CanvasBitmap, 0, 0, _bitmapPaint);
            canvas.DrawPath(_path, _paint);
           
        }

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

#if TESTSAVE
            CaptureSave.Paints.Add(new Paint(_paint));
            CaptureSave.Paths.Add(new Path(_path));
#endif

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

#region public methods

#if TESTSAVE
        public void UndoLastPath()
        {
            //quick hack... don't do it.
            if (CaptureSave.Paints.Count != CaptureSave.Paths.Count || CaptureSave.Paths.Count == 0)
                return;

            ClearCanvas();

            CaptureSave.Paths.RemoveAt(CaptureSave.Paths.Count - 1);
            CaptureSave.Paints.RemoveAt(CaptureSave.Paints.Count - 1);
            LoadData(CaptureSave);
        }

        private void LoadData(SaveData data)
        {
            //quick hack... don't do it.
            if (data.Paints.Count != data.Paths.Count)
                return;

            for (int i = 0; i < data.Paints.Count; i++)
            {
                _canvas.DrawPath(data.Paths[i], data.Paints[i]);
            }
            Invalidate();
        }

        public void LoadSaveData(SaveData data)
        {
            m_ToLoad = data;
        }
#endif

        public void ClearCanvas()
        {
            _canvas.DrawColor(BackgroundColor);
            Invalidate();
        }
#endregion
    }

}

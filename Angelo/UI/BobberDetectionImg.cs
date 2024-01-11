using Angelo.Bot.Bobber;
using Angelo.Screen;
using Angelo.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Angelo.UI
{
    internal class BobberDetectionImg
    {
        private Bitmap? _baseBmp;
        private WriteableBitmap? _writableBmp;
        private int _pixelSelectStartHue = 0;
        private int _pixelSelectDistSum = 0;
        private int _pixelSelectDistLeft = 0;
        private int _pixelSelectDistRight = 0;
        private int _pixelSelectionCount = 0;
        private bool[,]? _pixelMatrix;

        public System.Windows.Controls.Image Image { get; } = new();

        public BobberDetectionImg()
        {
            Image.Stretch = Stretch.None;
            RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.NearestNeighbor);
            Image.MouseDown += Image_MouseDown;
            Image.MouseUp += Image_MouseUp;
            Image.MouseMove += Image_MouseMove;
        }

        /// <summary>
        /// Selected pixels updated.
        /// </summary>
        /// <param name="lastPixelHue">Latest selected pixel value.</param>
        /// <param name="avgHue">Average hue of all selected pixels.</param>
        /// <param name="minHue">Minimum hue in current selection, can go negative without wrapping around!</param>
        /// <param name="maxHue">Maximum hue in current selection, can go above 360 without wrapping around!</param>
        /// <param name="selectionCount">The number of selected pixels.</param>
        public delegate void PixelSelectEvent(int lastPixelHue, int avgHue, int minHue, int maxHue, int selectionCount);
        public event PixelSelectEvent? PixelSelect;

        /// <summary>
        /// Set the image to show and work with.
        /// </summary>
        /// <param name="bmp"></param>
        public void SetImg(Bitmap? bmp)
        {
            _baseBmp = bmp;

            if (bmp == null)
            {
                Image.Source = null;
                _writableBmp = null;
                return;
            }

            _pixelMatrix = new bool[bmp.Width, bmp.Height];
            ResetWritableImage();
        }

        /// <summary>
        /// Draw rectangles around bobber positions.
        /// </summary>
        /// <param name="areas"></param>
        /// <param name="offset">Offset used if bobber coordinates do not match image coordinates. E.g. if only a region of the screen is shown.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void DrawBobberPositions(List<FloodCountResult> areas, Point? offset)
        {
            if (_writableBmp == null)
                throw new InvalidOperationException("An image must be set first!");

            const int borderColor = 0xFF0000;
            int offsetX = offset.HasValue ? -offset.Value.X : 0;
            int offsetY = offset.HasValue ? -offset.Value.Y : 0;
            int bpp = _writableBmp.BackBufferStride / _writableBmp.PixelWidth;

            foreach (var area in areas)
            {
                // Offset x and y coordinates so they are relative to region instead of screen.
                var bBox = new Rectangle(area.X, area.Y, area.Width, area.Height);
                bBox.Offset(offsetX, offsetY);

                try
                {
                    _writableBmp.Lock();

                    unsafe
                    {
                        byte* bbuffer = (byte*)_writableBmp.BackBuffer;

                        for (int x = bBox.X; x <= bBox.Right; x++)
                        {
                            // Top border
                            *(int*)(bbuffer + (bBox.Y * _writableBmp.BackBufferStride) + (x * bpp)) = borderColor;
                            // Bottom border
                            *(int*)(bbuffer + (bBox.Bottom * _writableBmp.BackBufferStride) + (x * bpp)) = borderColor;
                        }

                        for (int y = bBox.Y; y <= bBox.Bottom; y++)
                        {
                            // Left border
                            *(int*)(bbuffer + (y * _writableBmp.BackBufferStride) + (bBox.X * bpp)) = borderColor;
                            // Right border
                            *(int*)(bbuffer + (y * _writableBmp.BackBufferStride) + (bBox.Right * bpp)) = borderColor;
                        }
                    }

                    _writableBmp.AddDirtyRect(new System.Windows.Int32Rect(bBox.X, bBox.Y, bBox.Width, bBox.Height));
                }
                finally
                {
                    _writableBmp.Unlock();
                }
            }
        }

        private void ResetWritableImage()
        {
            if (_baseBmp == null)
                throw new NullReferenceException("Base image should not be null at this point!");

            BitmapImage bmpimg = new();
            MemoryStream mstream = new();
            _baseBmp.Save(mstream, System.Drawing.Imaging.ImageFormat.Bmp);
            bmpimg.BeginInit();
            mstream.Seek(0, SeekOrigin.Begin);
            bmpimg.StreamSource = mstream;
            bmpimg.EndInit();

            _writableBmp = new WriteableBitmap(bmpimg);
            Image.Source = _writableBmp;
        }

        private void AddMousePixel(MouseEventArgs e)
        {
            if (_baseBmp == null)
                return;

            var mousePos = e.GetPosition(Image);
            int x = (int)mousePos.X;
            int y = (int)mousePos.Y;

            if (_pixelMatrix == null)
                throw new NullReferenceException("Pixel matrix was null when it shouldn't be.");

            if (_pixelMatrix[x, y])
                return;

            PixelColor pc = new(_baseBmp.GetPixel(x, y).ToArgb());
            int hue = pc.GetHue();

            _pixelSelectionCount++;
            if (_pixelSelectionCount == 1)
            {
                _pixelSelectStartHue = hue;
            }
            else
            {
                int dist = hue - _pixelSelectStartHue;
                double rot = dist / 360.0;
                int closestDirection = Convert.ToInt32(rot);
                double closestRot = rot - closestDirection;
                int hueOffset = Convert.ToInt32(360 * closestRot);
                _pixelSelectDistSum += hueOffset;
                _pixelSelectDistLeft = Math.Min(_pixelSelectDistLeft, hueOffset);
                _pixelSelectDistRight = Math.Max(_pixelSelectDistRight, hueOffset);
            }
            _pixelMatrix[x, y] = true;

            if (_writableBmp != null)
            {
                try
                {
                    _writableBmp.Lock();
                    int bpp = _writableBmp.BackBufferStride / _writableBmp.PixelWidth;
                    unsafe
                    {
                        byte* bbuffer = (byte*)_writableBmp.BackBuffer;
                        *(int*)(bbuffer + (y * _writableBmp.BackBufferStride) + (x * bpp)) = 0xFFFFFF;
                    }
                    _writableBmp.AddDirtyRect(new System.Windows.Int32Rect(x, y, 1, 1));
                }
                finally
                {
                    _writableBmp.Unlock();
                }
            }

            int avgHue = Convert.ToInt32(_pixelSelectStartHue + (double)_pixelSelectDistSum / _pixelSelectionCount);
            int leftMost = _pixelSelectStartHue + _pixelSelectDistLeft;
            int rightMost = _pixelSelectStartHue + _pixelSelectDistRight;
            PixelSelect?.Invoke(hue, avgHue, leftMost, rightMost, _pixelSelectionCount);
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_baseBmp == null)
                return;

            ResetWritableImage();
            _pixelSelectionCount = 0;
            _pixelSelectStartHue = 0;
            _pixelSelectDistSum = 0;
            _pixelSelectionCount = 0;
            _pixelSelectDistLeft = 0;
            _pixelSelectDistRight = 0;
            _pixelMatrix = new bool[_baseBmp.Width, _baseBmp.Height];
            AddMousePixel(e);
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            AddMousePixel(e);

            if (_writableBmp == null)
                return;

            System.Drawing.Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)_writableBmp));
                enc.Save(outStream);
                bmp = new Bitmap(outStream);
            }

            int avgHue = Convert.ToInt32(_pixelSelectStartHue + (double)_pixelSelectDistSum / _pixelSelectionCount);
            if (avgHue < 0) avgHue += 360;

            ResetWritableImage();

            BobberFinder bf = new(_writableBmp);
            var settings = SettingsManager.GetSettings();
            var results = bf.FindBobbers(settings.BobberPixels.Value, avgHue, settings.BobberHueTolerance.Value, new Rectangle(0, 0, _writableBmp.PixelWidth, _writableBmp.PixelHeight));
            DrawBobberPositions(results, null);
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            AddMousePixel(e);
        }
    }
}

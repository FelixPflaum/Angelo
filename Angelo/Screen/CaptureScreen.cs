using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Angelo.Screen
{
    internal class CaptureScreen
    {
        private const PixelFormat PIXELFORMART = PixelFormat.Format32bppRgb;
        private const uint BITS_PER_PIXEL = 4;

        private readonly Bitmap _bmp;
        private readonly Graphics _gfx;
        private BitmapData _bmpd;

        public readonly ScreenData Screen;

        public CaptureScreen()
        {
            Screen = ScreenHelpers.GetScreenData();
            _bmp = new Bitmap((int)Screen.Width, (int)Screen.Height, PIXELFORMART);
            _gfx = Graphics.FromImage(_bmp);
            _gfx.CopyFromScreen(0, 0, 0, 0, new Size(0, 0), CopyPixelOperation.SourceCopy);
            _bmpd = _bmp.LockBits(new Rectangle(0, 0, 1, 1), ImageLockMode.ReadOnly, PIXELFORMART);
        }

        /// <summary>
        /// Update internal image data to current screen content.
        /// </summary>
        public void Update()
        {
            _bmp.UnlockBits(_bmpd);
            _gfx.CopyFromScreen(0, 0, 0, 0, new Size((int)Screen.Width, (int)Screen.Height), CopyPixelOperation.SourceCopy);
            _bmpd = _bmp.LockBits(new Rectangle(0, 0, _bmp.Width, _bmp.Height), ImageLockMode.ReadWrite, PIXELFORMART);
        }

        /// <summary>
        /// Update a select region of the image data with current screen content.
        /// </summary>
        /// <param name="xStart"></param>
        /// <param name="yStart"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void UpdatePartial(int xStart, int yStart, int width, int height)
        {
            _bmp.UnlockBits(_bmpd);
            _gfx.CopyFromScreen(xStart, yStart, xStart, yStart, new Size(width, height), CopyPixelOperation.SourceCopy);
            //_bmpd = _bmp.LockBits(new Rectangle(xStart, yStart, width, height), ImageLockMode.ReadOnly, PIXELFORMART);
            // This does not seem to change the duration of this operation in any meaningful way.
            // The bottleneck seems to be somewhere else. Why not have constant dimensions then?
            _bmpd = _bmp.LockBits(new Rectangle(0, 0, _bmp.Width, _bmp.Height), ImageLockMode.ReadOnly, PIXELFORMART);
        }

        /// <summary>
        /// Draw bitmap on internal image.
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="x">x coordinate of top left corner.</param>
        /// <param name="y">y coordinate of top left corner.</param>
        public void SetBitmap(Bitmap bmp, uint x, uint y)
        {
            _bmp.UnlockBits(_bmpd);
            _gfx.DrawImage(bmp, x, y);
            _bmpd = _bmp.LockBits(new Rectangle(0, 0, _bmp.Width, _bmp.Height), ImageLockMode.ReadOnly, PIXELFORMART);
        }

        /// <summary>
        /// Get a Bitmap from buffer data. Bitmap will be manually filled, this functions is most likely slow.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns>Bitmap with the current state from last Update() or UpdatePartial() call.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Bitmap GetBitmapFromBuffer(uint x, uint y, uint width, uint height)
        {
            if (x + width >= _bmpd.Width)
                throw new ArgumentOutOfRangeException(nameof(x), "Coordinate can't be outside of screen!");
            if (y + height >= _bmpd.Height)
                throw new ArgumentOutOfRangeException(nameof(y), "Coordinate can't be outside of screen!");

            Bitmap recostructed = new((int)width, (int)height, PIXELFORMART);
            uint xEnd = x + width;
            uint yEnd = y + height;

            unsafe
            {
                uint* pixelPtr = (uint*)_bmpd.Scan0;
                uint pixelValue;

                for (; x < xEnd; x++)
                {
                    for (; y < yEnd; y++)
                    {
                        pixelPtr = (uint*)_bmpd.Scan0 + (y * _bmpd.Stride / BITS_PER_PIXEL) + x;
                        pixelValue = *pixelPtr & 0xFFFFFF;
                        pixelValue |= 0xFF000000;
                        recostructed.SetPixel((int)x, (int)y, Color.FromArgb((int)pixelValue));
                    }
                }
            }
            return recostructed;
        }

        /// <inheritdoc cref="GetBitmapFromBuffer(uint, uint, uint, uint)"/>
        public Bitmap GetBitmapFromBuffer()
        {
            return GetBitmapFromBuffer(0, 0, Screen.Width, Screen.Height);
        }

        /// <summary>
        /// Get the color of a pixel.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public PixelColor GetPixel(uint x, uint y)
        {
            if (x >= Screen.Width)
                throw new ArgumentOutOfRangeException(nameof(x), "Coordinate can't be outside of screen!");
            if (y >= Screen.Height)
                throw new ArgumentOutOfRangeException(nameof(y), "Coordinate can't be outside of screen!");

            unsafe
            {
                byte* pixelRow = (byte*)_bmpd.Scan0 + (y * _bmpd.Stride);
                uint pixelStart = x * BITS_PER_PIXEL;
                byte b = pixelRow[pixelStart];
                byte g = pixelRow[pixelStart + 1];
                byte r = pixelRow[pixelStart + 2];
                return new PixelColor(r, g, b);
            }
        }

        /// <summary>
        /// Check if color at position matches.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="val"></param>
        /// <returns>True if color at postition is equal.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool CheckColorAt(uint x, uint y, uint val)
        {
            if (x >= Screen.Width)
                throw new ArgumentOutOfRangeException(nameof(x), "Coordinate can't be outside of screen!");
            if (y >= Screen.Height)
                throw new ArgumentOutOfRangeException(nameof(y), "Coordinate can't be outside of screen!");

            unsafe
            {
                uint* pixelPtr = (uint*)_bmpd.Scan0 + (y * _bmpd.Stride / BITS_PER_PIXEL) + x;
                uint pixelVal = *pixelPtr & 0xFFFFFF;
                return pixelVal == val;
            }
        }

        /// <param name="r"></param>
        /// <inheritdoc cref="CheckColorAt(uint, uint, uint)"/>
        public bool CheckColorAt(uint x, uint y, PixelColor pc)
        {
            return CheckColorAt(x, y, pc.Value);
        }

        /// <summary>
        /// Find the first pixel matching the color. Using this after UpdatePartial() may have unexpected results.
        /// </summary>
        /// <param name="value">The pixel color value to look for.</param>
        /// <param name="offset">Start search from this offset.</param>
        /// <returns>Point with the pixel position. Has no value if no pixel was found.</returns>
        public Point? FindPixel(uint value, uint offset = 0)
        {
            unsafe
            {
                uint linePxCount = (uint)_bmpd.Stride / BITS_PER_PIXEL;
                uint length = linePxCount * (uint)_bmpd.Height;
                uint* pixelPtr = (uint*)_bmpd.Scan0;
                uint pixelValue;
                for (; offset < length; offset++)
                {
                    pixelValue = pixelPtr[offset] & 0xFFFFFF;
                    if (pixelValue == value)
                    {
                        int y = (int)(offset / linePxCount);
                        int x = (int)(offset % linePxCount);
                        return new Point(x, y);
                    }
                }
            }

            return null;
        }

        /// <inheritdoc cref="FindPixel(uint, uint)"/>
        public Point? FindPixel(PixelColor pc, uint offset = 0)
        {
            return FindPixel(pc.Value, offset);
        }

        /// <inheritdoc cref="FindPixel(uint, uint)"/>
        public Point? FindPixel(uint value, Point offset)
        {
            uint offsetVal = (uint)(offset.X + offset.Y * Screen.Width);
            return FindPixel(value, offsetVal);
        }

        /// <inheritdoc cref="FindPixel(uint, uint)"/>
        public Point? FindPixel(PixelColor pc, Point offset)
        {
            return FindPixel(pc.Value, offset);
        }
    }
}

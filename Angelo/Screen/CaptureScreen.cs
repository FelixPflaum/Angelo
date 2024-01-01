using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Angelo.Screen
{
    internal class CaptureScreen
    {
        private const PixelFormat PIXELFORMART = PixelFormat.Format32bppRgb;
        private const int BITS_PER_PIXEL = 4;

        private readonly Bitmap _bmp;
        private readonly Graphics _gfx;
        private BitmapData _bmpd;

        public readonly ScreenData Screen;

        public CaptureScreen()
        {
            Screen = ScreenHelpers.GetScreenData();
            _bmp = new Bitmap(Screen.Width, Screen.Height, PIXELFORMART);
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
            _gfx.CopyFromScreen(0, 0, 0, 0, new Size(Screen.Width, Screen.Height), CopyPixelOperation.SourceCopy);
            _bmpd = _bmp.LockBits(new Rectangle(0, 0, _bmp.Width, _bmp.Height), ImageLockMode.ReadWrite, PIXELFORMART);
        }

        /// <summary>
        /// Update a select region of the image data with current screen content.
        /// </summary>
        /// <param name="xStart"></param>
        /// <param name="yStart"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Update(int xStart, int yStart, int width, int height)
        {
            if (xStart > Screen.Width || xStart < 0)
                throw new ArgumentOutOfRangeException(nameof(xStart), "Values must fit into screen dimensions!");
            if (yStart > Screen.Height || yStart < 0)
                throw new ArgumentOutOfRangeException(nameof(yStart), "Values must fit into screen dimensions!");
            if (width > Screen.Width || width < 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Values must fit into screen dimensions!");
            if (height > Screen.Height || height < 0)
                throw new ArgumentOutOfRangeException(nameof(height), "Values must fit into screen dimensions!");
            if (xStart + width > Screen.Width || yStart + height > Screen.Height)
                throw new ArgumentException("Resulting rectangle must fit within screen dimensions!");

            _bmp.UnlockBits(_bmpd);
            _gfx.CopyFromScreen(xStart, yStart, xStart, yStart, new Size(width, height), CopyPixelOperation.SourceCopy);
            // Not reducing dimensions here doesn't seem to impact performance in any meaningful way.
            // Leave dimensions constant to make things easier.
            _bmpd = _bmp.LockBits(new Rectangle(0, 0, _bmp.Width, _bmp.Height), ImageLockMode.ReadOnly, PIXELFORMART);
        }

        /// <summary>
        /// Draw bitmap on internal image.
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="x">x coordinate of top left corner.</param>
        /// <param name="y">y coordinate of top left corner.</param>
        public void SetBitmap(Bitmap bmp, int x, int y)
        {
            _bmp.UnlockBits(_bmpd);
            _gfx.DrawImage(bmp, x, y);
            _bmpd = _bmp.LockBits(new Rectangle(0, 0, _bmp.Width, _bmp.Height), ImageLockMode.ReadOnly, PIXELFORMART);
        }

        /// <summary>
        /// Get a Bitmap from buffer data. Bitmap will be manually filled, this functions is most likely slow.
        /// </summary>
        /// <param name="xStart"></param>
        /// <param name="yStart"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="pixelRatio">How many pixels to map to one pixel on the created Bitmap.</param>
        /// <returns>Bitmap with the current state from last Update() or UpdatePartial() call.</returns>
        public Bitmap GetBitmapFromBuffer(int xStart, int yStart, int width, int height, int pixelRatio = 1)
        {
            if (xStart > Screen.Width || xStart < 0)
                throw new ArgumentOutOfRangeException(nameof(xStart), "Values must fit into screen dimensions!");
            if (yStart > Screen.Height || yStart < 0)
                throw new ArgumentOutOfRangeException(nameof(yStart), "Values must fit into screen dimensions!");
            if (width > Screen.Width || width < 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Values must fit into screen dimensions!");
            if (height > Screen.Height || height < 0)
                throw new ArgumentOutOfRangeException(nameof(height), "Values must fit into screen dimensions!");
            if (xStart + width > Screen.Width || yStart + height > Screen.Height)
                throw new ArgumentException("Resulting rectangle must fit within screen dimensions!");
            if (pixelRatio != 1 && pixelRatio % 2 != 0)
                throw new ArgumentOutOfRangeException(nameof(pixelRatio), "Ratio has to be divisible by 2!");

            width /= pixelRatio;
            height /= pixelRatio;
            int xEnd = xStart + width;
            int yEnd = yStart + height;

            Bitmap recostructed = new(width, height, PIXELFORMART);

            unsafe
            {
                int* pixelPtr = (int*)_bmpd.Scan0;
                int pixelValue;

                for (int x = xStart; x < xEnd; x++)
                {
                    for (int y = yStart; y < yEnd; y++)
                    {
                        pixelPtr = (int*)_bmpd.Scan0 + ((y * _bmpd.Stride / BITS_PER_PIXEL) + x) * pixelRatio;
                        pixelValue = *pixelPtr & 0xFFFFFF;
                        pixelValue |= unchecked((int)0xFF000000);
                        recostructed.SetPixel(x - xStart, y - yStart, Color.FromArgb(pixelValue));
                    }
                }
            }

            return recostructed;
        }

        /// <inheritdoc cref="GetBitmapFromBuffer(int, int, int, int, int)"/>
        public Bitmap GetBitmapFromBuffer(int pixelRatio = 1)
        {
            return GetBitmapFromBuffer(0, 0, Screen.Width, Screen.Height, pixelRatio);
        }

        /// <summary>
        /// Get the color of a pixel.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public PixelColor GetPixel(int x, int y)
        {
            if (x >= Screen.Width || x < 0)
                throw new ArgumentOutOfRangeException(nameof(x), "Coordinate can't be outside of screen!");
            if (y >= Screen.Height || y < 0)
                throw new ArgumentOutOfRangeException(nameof(y), "Coordinate can't be outside of screen!");

            unsafe
            {
                byte* pixelRow = (byte*)_bmpd.Scan0 + (y * _bmpd.Stride);
                int pixelStart = x * BITS_PER_PIXEL;
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
        public bool CheckColorAt(int x, int y, int val)
        {
            if (x >= Screen.Width || x < 0)
                throw new ArgumentOutOfRangeException(nameof(x), "Coordinate can't be outside of screen!");
            if (y >= Screen.Height || y < 0)
                throw new ArgumentOutOfRangeException(nameof(y), "Coordinate can't be outside of screen!");

            unsafe
            {
                int* pixelPtr = (int*)_bmpd.Scan0 + (y * _bmpd.Stride / BITS_PER_PIXEL) + x;
                int pixelVal = *pixelPtr & 0xFFFFFF;
                return pixelVal == val;
            }
        }

        /// <param name="r"></param>
        /// <inheritdoc cref="CheckColorAt(int, int, int)"/>
        public bool CheckColorAt(int x, int y, PixelColor pc)
        {
            return CheckColorAt(x, y, pc.Value);
        }

        /// <summary>
        /// Find the first pixel matching the color. Using this after UpdatePartial() may have unexpected results.
        /// </summary>
        /// <param name="searchValue">The pixel color value to look for.</param>
        /// <param name="offset">Start search from this offset.</param>
        /// <returns>Point with the pixel position. Has no value if no pixel was found.</returns>
        public Point? FindPixel(int searchValue, int offset = 0)
        {
            unsafe
            {
                int linePxCount = _bmpd.Stride / BITS_PER_PIXEL;
                int length = linePxCount * _bmpd.Height;
                int* pixelPtr = (int*)_bmpd.Scan0;
                int pixelValue;
                for (; offset < length; offset++)
                {
                    pixelValue = pixelPtr[offset] & 0xFFFFFF;
                    if (pixelValue == searchValue)
                    {
                        int y = offset / linePxCount;
                        int x = offset % linePxCount;
                        return new Point(x, y);
                    }
                }
            }

            return null;
        }

        /// <inheritdoc cref="FindPixel(int, int)"/>
        public Point? FindPixel(PixelColor pc, int offset = 0)
        {
            return FindPixel(pc.Value, offset);
        }

        /// <inheritdoc cref="FindPixel(int, int)"/>
        public Point? FindPixel(int value, Point offset)
        {
            int offsetVal = offset.X + offset.Y * Screen.Width;
            return FindPixel(value, offsetVal);
        }

        /// <inheritdoc cref="FindPixel(int, int)"/>
        public Point? FindPixel(PixelColor pc, Point offset)
        {
            return FindPixel(pc.Value, offset);
        }
    }
}

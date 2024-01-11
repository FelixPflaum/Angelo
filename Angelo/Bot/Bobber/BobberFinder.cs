using Angelo.Screen;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace Angelo.Bot.Bobber
{
    internal class BobberFinder
    {
        private readonly CaptureScreen? _captureScreen;
        private readonly WriteableBitmap? _writeableBitmap;

        public BobberFinder(CaptureScreen captureScreen)
        {
            _captureScreen = captureScreen;
        }

        public BobberFinder(WriteableBitmap writeableBitmap)
        {
            _writeableBitmap = writeableBitmap;
        }

        private FloodCountHue GetFCH(int hue, int hueTolerance)
        {
            if (_captureScreen != null)
                return new(_captureScreen, hue, hueTolerance);

            if (_writeableBitmap != null)
                return new(_writeableBitmap, hue, hueTolerance);

            throw new InvalidOperationException("Both _captureScreen and _writeableBitmap are null!");
        }

        private PixelColor GetPixel(int x, int y)
        {
            if (_captureScreen != null)
                return _captureScreen.GetPixel(x, y);

            if (_writeableBitmap != null)
            {
                unsafe
                {
                    byte* pBuffer = (byte*)_writeableBitmap.BackBuffer;
                    int bpp = _writeableBitmap.BackBufferStride / _writeableBitmap.PixelWidth;
                    int pxVal = *(int*)(pBuffer + y * _writeableBitmap.BackBufferStride + x * bpp);
                    return new PixelColor(pxVal);
                }
            }

            throw new InvalidOperationException("Both _captureScreen and _writeableBitmap are null!");
        }

        /// <summary>
        /// Find possible bobber positions by finding areas of pixels with similar color.
        /// </summary>
        /// <param name="minConnected">Minimum number of connected pixels to qualify an area.</param>
        /// <param name="hue">The target hue.</param>
        /// <param name="hueTolerance">Tolerance to target hue.</param>
        /// <param name="searchRegion"></param>
        /// <returns>A list of all found potential bobbers. Limited to 10.</returns>
        public List<FloodCountResult> FindBobbers(int minConnected, int hue, int hueTolerance, Rectangle searchRegion)
        {
            // Don't scan more lines after we have this many possible areas already.
            const int sanityCheckAmount = 10;

            int xStart = searchRegion.X;
            int yStart = searchRegion.Y;
            int width = searchRegion.Width;
            int height = searchRegion.Height;
            int xEnd = xStart + width;
            int yEnd = yStart + height;

            FloodCountHue fch = GetFCH(hue, hueTolerance);
            List<FloodCountResult> foundAreas = new();

            for (int y = yStart; y < yEnd; y++)
            {
                if (foundAreas.Count > sanityCheckAmount)
                    break;

                for (int x = xStart; x < xEnd; x++)
                {
                    PixelColor pixel = GetPixel(x, y);
                    if (!fch.IsPixelInHueRange(pixel))
                        continue;

                    bool isInExistingArea = false;
                    foreach (FloodCountResult area in foundAreas)
                    {
                        if (area.Contains(x, y))
                        {
                            x = area.Right;
                            isInExistingArea = true;
                            break;
                        }
                    }
                    if (isInExistingArea)
                        continue;

                    FloodCountResult res = fch.CountFrom(x, y);
                    if (res.ConnectedPixels >= minConnected)
                        foundAreas.Add(res);

                    x += res.Width;
                }
            }

            return foundAreas;
        }
    }
}

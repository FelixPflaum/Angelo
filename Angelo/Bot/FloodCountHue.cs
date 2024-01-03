using System;
using System.Collections.Generic;
using System.Drawing;
using Angelo.Screen;

namespace Angelo.Bot
{
    internal readonly struct FloodCountResult
    {
        public readonly int ConnectedPixels;
        public readonly Rectangle BoundingBox;
        public readonly Point Center;

        public FloodCountResult(int count, int xLeft, int xRight, int yTop, int yBottom)
        {
            ConnectedPixels = count;
            BoundingBox = new Rectangle(xLeft, yTop, xRight - xLeft, yBottom - yTop);
            Center = new Point(xLeft, yTop);
            Center.Offset(BoundingBox.Width / 2, BoundingBox.Height / 2);
        }
    }

    /// <summary>
    /// Used to count connected pixels in a given hue range.
    /// </summary>
    internal class FloodCountHue
    {
        private readonly CaptureScreen _captureScreen;
        private readonly int _width;
        private readonly int _height;
        private readonly int _minHue;
        private readonly int _maxHue;
        private bool[,] _confirmedPixels;

        public FloodCountHue(CaptureScreen captureScreen, int hue, int hueTolerance)
        {
            _captureScreen = captureScreen;
            _confirmedPixels = new bool[1, 1];
            _width = captureScreen.Screen.Width;
            _height = captureScreen.Screen.Height;

            hue = ClipHue(hue);
            _minHue = ClipHue(hue - hueTolerance);
            _maxHue = ClipHue(hue + hueTolerance);
        }

        /// <summary>
        /// Clips given hue number into the 360° range.
        /// </summary>
        /// <param name="hue"></param>
        /// <returns>The remainder of the hue rotation.</returns>
        private static int ClipHue(int hue)
        {
            hue %= 360;
            if (hue < 0)
                hue += 360;
            return hue;
        }

        /// <summary>
        /// Count all connected pixels in range of configured hue.
        /// </summary>
        /// <param name="x">The start pixel x-coordinate.</param>
        /// <param name="y">The start pixel y-coordinate.</param>
        /// <param name="debugMap">If not null will fill found pixel positions on the bitmap white.</param>
        /// <returns>The <see cref="FloodCountResult"/> object containing collected data.</returns>
        public FloodCountResult CountFrom(int x, int y, Bitmap? debugMap = null)
        {
            _confirmedPixels = new bool[_width, _height];
            return CombinedScanAndFill(x, y, debugMap);
        }

        /// <summary>
        /// Check if Pixel is in hue range.
        /// </summary>
        /// <param name="pc"></param>
        /// <returns>true if pixel is in range</returns>
        public bool IsPixelInHueRange(PixelColor pc)
        {
            int hue = pc.GetHue();

            if (hue == 0)
                return false;

            if (_maxHue < _minHue)
                return hue <= _maxHue || hue >= _minHue;
            else
                return hue >= _minHue && hue <= _maxHue;
        }

        private bool IsInRangeAndUnconfirmed(int x, int y)
        {
            if (_confirmedPixels[x, y])
                return false;

            return IsPixelInHueRange(_captureScreen.GetPixel(x, y));
        }

        private readonly struct StackEntry
        {
            public readonly int X1;
            public readonly int X2;
            public readonly int Y;
            public readonly int Dy;

            public StackEntry(int x, int x2, int y, int dy)
            {
                X1 = x;
                X2 = x2;
                Y = y;
                Dy = dy;
            }
        }

        // Implementation of https://en.wikipedia.org/wiki/Flood_fill#Span_filling combined-scan-and-fill
        // Modified to count found pixels and create a bounding box.
        private FloodCountResult CombinedScanAndFill(int x, int y, Bitmap? debugMap)
        {
            if (debugMap != null)
                if (debugMap.Width != _width || debugMap.Height != _height)
                    throw new ArgumentException("Bitmap must have same size as current working surface!", nameof(debugMap));

            if (!IsInRangeAndUnconfirmed(x, y))
                throw new ArgumentException("Given start coordinates are not a valid pixel!");

            int count = 0;
            int xMin = _width;
            int xMax = 0;
            int yMin = _height;
            int yMax = 0;
            int x1, x2, dy;

            Stack<StackEntry> s = new();

            s.Push(new StackEntry(x, x, y, 1));
            s.Push(new StackEntry(x, x, y - 1, -1));

            while (s.Count > 0)
            {
                StackEntry e = s.Pop();
                x1 = e.X1;
                x2 = e.X2;
                dy = e.Dy;
                y = e.Y;
                x = e.X1;

                if (IsInRangeAndUnconfirmed(x, y))
                {
                    x--;
                    while (x >= 0 && IsInRangeAndUnconfirmed(x, y))
                    {
                        _confirmedPixels[x, y] = true;
                        debugMap?.SetPixel(x, y, Color.White);
                        xMin = Math.Min(x, xMin);
                        xMax = Math.Max(x, xMax);
                        yMin = Math.Min(y, yMin);
                        yMax = Math.Max(y, yMax);
                        count++;

                        x--;
                    }
                    x++;

                    int newY = y - dy;
                    if (x < x1 && newY >= 0 && newY < _height)
                    {
                        s.Push(new StackEntry(x, x1 - 1, y - dy, -dy));
                    }
                }

                while (x1 <= x2)
                {
                    while (x1 < _width && IsInRangeAndUnconfirmed(x1, y))
                    {
                        _confirmedPixels[x1, y] = true;
                        debugMap?.SetPixel(x1, y, Color.White);
                        xMin = Math.Min(x1, xMin);
                        xMax = Math.Max(x1, xMax);
                        yMin = Math.Min(y, yMin);
                        yMax = Math.Max(y, yMax);
                        count++;
                        x1++;
                    }

                    int newY = y + dy;
                    if (x1 > x && newY >= 0 && newY < _height)
                    {
                        s.Push(new StackEntry(x, x1 - 1, y + dy, dy));
                    }

                    newY = y - dy;
                    if (x1 - 1 > x2 && newY >= 0 && newY < _height)
                    {
                        s.Push(new StackEntry(x2 + 1, x1 - 1, y - dy, -dy));
                    }

                    x1++;
                    while (x1 < x2 && !IsInRangeAndUnconfirmed(x1, y))
                    {
                        x1++;
                    }

                    x = x1;
                }
            }

            return new FloodCountResult(count, xMin, xMax, yMin, yMax);
        }
    }
}

using Angelo.Screen;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Angelo.Bot
{
    internal class ScreenHandler
    {
        private static readonly Point DATA_PX_OFFSET = new(5, 5);
        private static readonly PixelColor[] ANCHOR_COLORS = { new(0xFF0000), new(0x00FF00), new(0x0000FF), new(0xFFFFFF) };

        private readonly CaptureScreen _capture;
        private readonly Point[] _anchorPositions;
        private Rectangle _anchorRegion;

        private bool _haveAnchors = false;

        public ScreenHandler()
        {
            _capture = new CaptureScreen();
            _anchorPositions = new Point[4];
            _anchorRegion = new Rectangle();
        }

        /// <summary>
        /// Check for data pixel state.
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="noUpdate">Do not update image and reuse old screen data.</param>
        /// <returns>True if value(s) are set.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool CheckDataPixel(DataColors colors, bool noUpdate = false)
        {
            if (!_haveAnchors)
                throw new InvalidOperationException("CheckDataPixel can't be called before anchors are set!");

            int dpxoffset = (int)((uint)colors & 0xFF000000) >> 24;
            Point a0 = _anchorPositions[0];
            int dataX = a0.X + DATA_PX_OFFSET.X * (1 + dpxoffset);
            int dataY = a0.Y + DATA_PX_OFFSET.Y;

            if (!noUpdate)
                _capture.Update(dataX, dataY, 1, 1);

            PixelColor pixel = _capture.GetPixel(dataX, dataY);
            return pixel.Contains((int)colors & 0xFFFFFF);
        }

        /// <summary>
        /// Anchor positions are set.
        /// </summary>
        /// <returns></returns>
        public bool HaveAnchorPositions()
        {
            return _haveAnchors;
        }

        /// <summary>
        /// Check if all anchors are visible.
        /// </summary>
        /// <returns>True if all anchors could be checked.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool AreAnchorsVisible()
        {
            if (!_haveAnchors)
                throw new InvalidOperationException("AreAnchorsVisible can't be called before anchors are set!");

            _capture.Update(_anchorRegion.X, _anchorRegion.Y, _anchorRegion.Width, _anchorRegion.Height);

            foreach (Point a in _anchorPositions)
            {
                if (!_capture.CheckColorAt(a.X, a.Y, ANCHOR_COLORS[0]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Attempt to find anchor points on screen.
        /// This is required to succeeed for everything else to work.
        /// </summary>
        /// <returns>True if all anchor points were found.</returns>
        public bool SetupAnchors()
        {
            for (int i = 0; i < _anchorPositions.Length; i++)
            {
                _anchorPositions[i].X = 0;
                _anchorPositions[i].Y = 0;
            }

            _capture.Update();

            int nextAnchor = 0;
            Point offset = new(0, 0);

            while (nextAnchor < 4)
            {
                Point? point = _capture.FindPixel(ANCHOR_COLORS[0], offset);

                if (point == null)
                    break;

                if (!_capture.CheckColorAt(point.Value.X + 1, point.Value.Y, ANCHOR_COLORS[1])
                    || !_capture.CheckColorAt(point.Value.X, point.Value.Y + 1, ANCHOR_COLORS[2])
                    || !_capture.CheckColorAt(point.Value.X + 1, point.Value.Y + 1, ANCHOR_COLORS[3]))
                {
                    offset = point.Value;
                    offset.Offset(1, 0);
                    continue;
                }

                switch (nextAnchor)
                {
                    case 0: // Top left
                    case 2: // Bottom left
                        {
                            _anchorPositions[nextAnchor] = point.Value;

                            offset.X = _anchorPositions[0].X + 100;
                            offset.Y = point.Value.Y;
                        }
                        break;
                    case 1: // Top right
                    case 3: // Bottom right
                        {
                            if (_anchorPositions[nextAnchor - 1].Y != point.Value.Y)
                                return false;

                            _anchorPositions[nextAnchor] = point.Value;

                            offset.X = _anchorPositions[0].X;
                            offset.Y = point.Value.Y + 100;
                        }
                        break;
                }

                nextAnchor++;
            }

            if (nextAnchor == _anchorPositions.Length)
            {
                _anchorRegion.X = _anchorPositions[0].X;
                _anchorRegion.Y = _anchorPositions[0].Y;
                _anchorRegion.Width = _anchorPositions[3].X - _anchorPositions[0].X;
                _anchorRegion.Height = _anchorPositions[3].Y - _anchorPositions[0].Y;

                _haveAnchors = true;
            }

            return _haveAnchors;
        }

        /// <summary>
        /// Count all pixels with each color component above givent threshold in an area.
        /// </summary>
        /// <param name="x">The x coordinate of the center of the area.</param>
        /// <param name="y">The y coordinate of the center of the area.</param>
        /// <param name="sideLength">The size of the area to check.</param>
        /// <param name="threshold">The color component threshold value.</param>
        /// <param name="dontUpdate">Set true to not update image data and reuse existing data.</param>
        /// <returns>The amount of pixels that satisfied the condition.</returns>
        public int CountAreaPixelsAbove(int x, int y, int sideLength, byte threshold, bool dontUpdate = false)
        {
            x -= sideLength / 2;
            y -= sideLength / 2;
            int endX = x + sideLength;
            int endY = y + sideLength;
            int count = 0;

            if (!dontUpdate)
                _capture.Update(x, y, sideLength, sideLength);

            while (y < endY)
            {
                PixelColor pixel = _capture.GetPixel(x, y);

                if (pixel.R >= threshold && pixel.G >= threshold && pixel.B >= threshold)
                    count++;

                x++;
                if (x >= endX)
                {
                    x = endX - sideLength;
                    y++;
                }
            }

            return count;
        }

        /// <summary>
        /// Get the region defined by the found anchor points.
        /// </summary>
        /// <returns>A <see cref="Rectangle"/> of the region.</returns>
        public Rectangle GetAnchorRegion()
        {
            return new Rectangle(_anchorRegion.X, _anchorRegion.Y, _anchorRegion.Width, _anchorRegion.Height);
        }

        /// <summary>
        /// Get currently buffered image of anchor region.
        /// </summary>
        /// <returns></returns>
        public Bitmap GetAnchorRegionImg()
        {
            return _capture.GetBitmapFromBuffer(_anchorRegion);
        }

        /// <summary>
        /// Find possible bobber positions by finding areas of pixels with similar color.
        /// </summary>
        /// <param name="minConnected">Minimum number of connected pixels to qualify an area.</param>
        /// <param name="hue">The target hue.</param>
        /// <param name="hueTolerance">Tolerance to target hue.</param>
        /// <param name="getBitmap">If set will paint found areas white. Has to have the same dimensions as the screen! This costs performance!</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If anchors are not yet set.</exception>
        private List<FloodCountResult> FindBobberPriv(int minConnected, int hue, int hueTolerance, Bitmap? getBitmap)
        {
            // Don't scan more lines after we have this many possible areas already.
            const int sanityCheckAmount = 10;

            if (!_haveAnchors)
                throw new InvalidOperationException("FindBobberPositions can't be called before anchors are set!");

            int xStart = _anchorRegion.X;
            int yStart = _anchorRegion.Y;
            int width = _anchorRegion.Width;
            int height = _anchorRegion.Height;
            int xEnd = xStart + width;
            int yEnd = yStart + height;

            FloodCountHue fch = new(_capture, hue, hueTolerance);
            List<FloodCountResult> foundAreas = new();

            for (int y = yStart; y < yEnd; y++)
            {
                if (foundAreas.Count > sanityCheckAmount)
                    break;

                for (int x = xStart; x < xEnd; x++)
                {
                    PixelColor pixel = _capture.GetPixel(x, y);
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

                    FloodCountResult res = fch.CountFrom(x, y, getBitmap);
                    if (res.ConnectedPixels >= minConnected)
                        foundAreas.Add(res);

                    x += res.Width;
                }
            }

            return foundAreas;
        }

        /// <inheritdoc cref="FindBobberPriv(int, int, int, Bitmap?)"/>
        public List<FloodCountResult> FindBobberPositions(int minConnected, int hue, int hueTolerance)
        {
            _capture.Update(_anchorRegion);
            return FindBobberPriv(minConnected, hue, hueTolerance, null);
        }

        /// <inheritdoc cref="FindBobberPriv(int, int, int, Bitmap?)"/>
        public List<FloodCountResult> FindBobberPositions(int minConnected, int hue, int hueTolerance, out Bitmap bmp)
        {
            _capture.Update(_anchorRegion);
            bmp = _capture.GetBitmapFromBuffer();
            return FindBobberPriv(minConnected, hue, hueTolerance, bmp);
        }
    }
}

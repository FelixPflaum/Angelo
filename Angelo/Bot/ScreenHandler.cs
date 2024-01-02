using Angelo.Screen;
using System;
using System.Drawing;

namespace Angelo.Bot
{
    internal class ScreenHandler
    {
        private static readonly Point DATA_PX_OFFSET = new(3, 3);
        private static readonly PixelColor[] ANCHOR_COLORS = { new(0xFF0000), new(0x00FF00), new(0x0000FF), new(0xFFFFFF) };

        private readonly CaptureScreen _capture;
        private readonly Point[] _anchorPositions;

        private bool _haveAnchors = false;

        public ScreenHandler()
        {
            _capture = new CaptureScreen();
            _anchorPositions = new Point[4];
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

            Point a0 = _anchorPositions[0];
            int dataX = a0.X + DATA_PX_OFFSET.X;
            int dataY = a0.Y + DATA_PX_OFFSET.Y;

            if (!noUpdate)
                _capture.Update(dataX, dataY, 1, 1);

            PixelColor pixel = _capture.GetPixel(dataX, dataY);
            return pixel.Contains((int)colors);
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

            int xStart = _anchorPositions[0].X;
            int yStart = _anchorPositions[0].Y;
            int width = _anchorPositions[3].X - xStart;
            int height = _anchorPositions[3].Y - yStart;

            _capture.Update(xStart, yStart, width, height);

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

            _haveAnchors = nextAnchor == _anchorPositions.Length;
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
    }
}

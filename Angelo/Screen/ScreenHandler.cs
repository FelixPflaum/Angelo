using System;
using System.Drawing;

namespace Angelo.Screen
{
    internal class ScreenHandler
    {
        private const int DATA_PX_OFFSET = 10;
        private readonly PixelColor COLOR_ANCHOR = new(0xFF00FF);

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
            int dataX = a0.X + DATA_PX_OFFSET;
            int dataY = a0.Y;

            if (!noUpdate)
                _capture.UpdatePartial(dataX, dataY, 1, 1);

            PixelColor pixel = _capture.GetPixel((uint)dataX, (uint)dataY);
            return pixel.Contains((uint)colors);
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
                throw new InvalidOperationException("CheckDataPixel can't be called before anchors are set!");

            int xStart = _anchorPositions[0].X;
            int yStart = _anchorPositions[0].Y;
            int width = _anchorPositions[3].X - xStart;
            int height = _anchorPositions[3].Y - yStart;

            _capture.UpdatePartial(xStart, yStart, width, height);

            foreach (Point a in _anchorPositions)
            {
                if (!_capture.CheckColorAt((uint)a.X, (uint)a.Y, COLOR_ANCHOR))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Attempt to find anchor points on screen.
        /// This is required to succeeed for everything else to work.
        /// </summary>
        /// <returns>True if all anchor points were found.</returns>
        public bool FindAnchors()
        {
            for (int i = 0; i < _anchorPositions.Length; i++)
            {
                _anchorPositions[i].X = 0;
                _anchorPositions[i].Y = 0;
            }

            int nextAnchor = 0;
            int offset = 0;

            while (nextAnchor < _anchorPositions.Length)
            {
                Point? point = _capture.FindPixel(COLOR_ANCHOR, (uint)offset);

                if (point == null)
                    break;

                switch (nextAnchor)
                {
                    case 2:
                    case 0:
                        {
                            _anchorPositions[nextAnchor] = point.Value;
                            // There will be multiple pixels. Skip them.
                            offset = point.Value.X + 50;
                        }
                        break;
                    case 1:
                    case 3:
                        {
                            // If this isn't on the same line we missed one.
                            if (_anchorPositions[nextAnchor - 1].Y != point.Value.Y)
                                return false;

                            _anchorPositions[1] = point.Value;
                            // Set offset a few lines forward for to skip addition pixels.
                            offset = (point.Value.Y + 10) * (int)_capture.Screen.Width;
                        }
                        break;
                    default:
                        throw new IndexOutOfRangeException();
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
        public int CountAreaPixelsAbove(uint x, uint y, uint sideLength, byte threshold, bool dontUpdate = false)
        {
            x -= sideLength / 2;
            y -= sideLength / 2;
            uint endX = x + sideLength;
            uint endY = y + sideLength;
            int count = 0;

            if (!dontUpdate)
                _capture.UpdatePartial((int)x, (int)y, (int)sideLength, (int)sideLength);

            while (y < endY)
            {
                PixelColor pixel = _capture.GetPixel((uint)x, (uint)y);

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

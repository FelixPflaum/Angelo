using System;
using static Angelo.WinAPI.User32;
using static Angelo.WinAPI.User32Defs;
using static Angelo.KBM.KBMHelpers;

namespace Angelo.Bot
{
    internal class MouseHandler
    {
        private const int USAGE_LINGER_TIME = 3000;

        private Point _lastPos;
        private long _unchangedSince;

        public MouseHandler()
        {
            GetCursorPos(out _lastPos);
            _unchangedSince = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Checks if mouse was moved by user.
        /// </summary>
        /// <returns>True if mouse should be treated as in use by the unser.</returns>
        public bool UpdateUsageState()
        {
            int oldX = _lastPos.X;
            int oldY = _lastPos.Y;
            GetCursorPos(out _lastPos);
            if (_lastPos.X != oldX || _lastPos.Y != oldY)
            {
                _unchangedSince = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            }
            return DateTimeOffset.Now.ToUnixTimeMilliseconds() - _unchangedSince < USAGE_LINGER_TIME;
        }

        /// <summary>
        /// Move mouse to given position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MoveTo(int x, int y)
        {
            MoveMouseAbsolute(x, y);
            GetCursorPos(out _lastPos);
        }

        /// <summary>
        /// Just a convinience alias for <see cref="MouseClick(bool, int)"/>.
        /// </summary>
        /// <inheritdoc cref="MouseClick(bool, int)"/>
        public static void Click(bool isRightClick, int releaseDelayMs)
        {
            MouseClick(isRightClick, releaseDelayMs);
        }
    }
}

using System;
using System.Text;
using static Angelo.WinAPI.User32;

namespace Angelo.Bot
{
    internal static class GameWindowHelpers
    {
        private const string GAME_WINDOW_NAME = "World of Warcraft";

        private static string? GetWindowTitle(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return null;

            int winTextLen = GetWindowTextLength(hWnd);

            if (winTextLen == 0)
                return null;

            StringBuilder outString = new StringBuilder(winTextLen);
            int resLen = GetWindowText(hWnd, outString, winTextLen + 1);

            if (resLen != winTextLen)
                return null;

            return outString.ToString();
        }

        /// <summary>
        /// Check if game is the current foreground window.
        /// </summary>
        /// <returns>true if game is in foreground.</returns>
        public static bool IsGameInForeground()
        {
            IntPtr fgWindow = GetForegroundWindow();
            return GetWindowTitle(fgWindow) == GAME_WINDOW_NAME;
        }
    }
}

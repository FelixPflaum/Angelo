using System;
using System.Runtime.InteropServices;
using static Angelo.WinAPI.User32;
using static Angelo.WinAPI.User32Defs;

namespace Angelo.Screen
{
    internal static class ScreenHelpers
    {
        private static ScreenData? screenData;

        /// <summary>
        /// Get device name of the primary screen.
        /// </summary>
        /// <returns>The device name if screen was found, otherwise null.</returns>
        private static string? GetPrimaryDisplayDevice()
        {
            DISPLAY_DEVICE dev = new();
            dev.cb = (uint)Marshal.SizeOf(dev);

            for (uint id = 0; EnumDisplayDevices(null, id, ref dev, 0); id++)
            {
                if (dev.StateFlags.HasFlag(DisplayDeviceStateFlags.AttachedToDesktop | DisplayDeviceStateFlags.PrimaryDevice))
                    return dev.DeviceName;
            }

            return null;
        }

        /// <summary>
        /// Get primary screen data.
        /// </summary>
        /// <param name="forceReload">Forces reload of data through the WinAPI.</param>
        /// <returns>ScreenData for primary screen. Value is cached and therefor invalid if display settings change!</returns>
        /// <exception cref="Exception">If WinAPI operations fail.</exception>
        public static ScreenData GetScreenData(bool forceReload = false)
        {
            if (forceReload || screenData == null)
            {
                string? primaryDevName = GetPrimaryDisplayDevice();

                if (primaryDevName == null)
                    throw new Exception("Can't get primary display device info!");

                if (!EnumDisplaySettings(primaryDevName, ENUM_CURRENT_SETTINGS, out DEVMODE devmode))
                    throw new Exception("Can't get primary display DEVMODE!");

                screenData = new ScreenData((int)devmode.dmPelsWidth, (int)devmode.dmPelsHeight, (int)devmode.dmDisplayFrequency);
            }

            return screenData.Value;
        }
    }
}

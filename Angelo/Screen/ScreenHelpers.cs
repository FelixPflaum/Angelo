using System;
using System.Runtime.InteropServices;

namespace Angelo.Screen
{
    internal readonly struct ScreenData
    {
        public readonly uint Width;
        public readonly uint Height;
        public readonly uint RefreshRate;

        public ScreenData(uint width, uint height, uint refreshRate)
        {
            Width = width;
            Height = height;
            RefreshRate = refreshRate;
        }
    }

    internal static class ScreenHelpers
    {
        private static ScreenData? screenData;

        [Flags]
        private enum DisplayDeviceStateFlags : uint
        {
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            PrimaryDevice = 0x4,
            MirroringDriver = 0x8,
            VGACompatible = 0x10,
            Removable = 0x20,
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct DISPLAY_DEVICE
        {
            public uint cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceStateFlags StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayDevices(string? lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [StructLayout(LayoutKind.Sequential)]
        private struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public ushort dmSpecVersion;
            public ushort dmDriverVersion;
            public ushort dmSize;
            public ushort dmDriverExtra;
            public uint dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public uint dmDisplayOrientation;
            public uint dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public ushort dmLogPixels;
            public uint dmBitsPerPel;
            public uint dmPelsWidth;
            public uint dmPelsHeight;
            public uint dmDisplayFlags;
            public uint dmDisplayFrequency;
            public uint dmICMMethod;
            public uint dmICMIntent;
            public uint dmMediaType;
            public uint dmDitherType;
            public uint dmReserved1;
            public uint dmReserved2;
            public uint dmPanningWidth;
            public uint dmPanningHeight;

        }

        private const int ENUM_CURRENT_SETTINGS = -1;

        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, out DEVMODE lpDevMode);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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

                screenData = new ScreenData(devmode.dmPelsWidth, devmode.dmPelsHeight, devmode.dmDisplayFrequency);
            }

            return screenData.Value;
        }
    }
}

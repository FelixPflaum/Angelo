using System.Runtime.InteropServices;
using System;

namespace Angelo.WinAPI
{
    internal static class User32Defs
    {
        [Flags]
        public enum DisplayDeviceStateFlags : uint
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
        public struct DISPLAY_DEVICE
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

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
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

        public const int ENUM_CURRENT_SETTINGS = -1;

        public enum InputType
        {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2
        }

        [Flags]
        public enum DWKeyboardFlags
        {
            KeyDown = 0x0,
            /// <summary>
            /// If specified, the wScan scan code consists of a sequence of two bytes, 
            /// where the first byte has a value of 0xE0. See Extended-Key Flag for more info. 
            /// </summary>
            ExtendedKey = 0x1,
            KeyUp = 0x2,
            /// <summary>
            /// If specified, the system synthesizes a VK_PACKET keystroke. The wVk parameter must be zero. 
            /// This flag can only be combined with the KEYEVENTF_KEYUP flag. For more information, see the Remarks section. 
            /// </summary>
            Unicode = 0x4,
            /// <summary>
            /// If specified, wScan identifies the key and wVk is ignored. 
            /// </summary>
            Scancode = 0x8,
        }

        [Flags]
        public enum DWMouseFlags
        {
            /// <summary>
            /// Relative mouse movement! This is subject to mouse settings and my not be in pixels!
            /// </summary>
            Move = 0x1,
            LeftDown = 0x2,
            LeftUp = 0x4,
            RightDown = 0x8,
            RightUp = 0x10,
            MiddleDown = 0x20,
            MiddleUp = 0x40,
            /// <summary>
            /// Maps coordinates to the entire desktop. Must be used with MOUSEEVENTF_ABSOLUTE.
            /// </summary>
            VirtualDesk = 0x4000,
            /// <summary>
            /// The dx and dy members contain normalized absolute coordinates. 
            /// If the flag is not set, dxand dy contain relative data (the change in position since the last reported position). 
            /// This flag can be set, or not set, regardless of what kind of mouse or other pointing device, if any, 
            /// is connected to the system. For further information about relative mouse motion, see the following Remarks section.
            /// </summary>
            Absolute = 0x8000,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInput
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardInput
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HardwareInput
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)] public MouseInput mi;
            [FieldOffset(0)] public KeyboardInput ki;
            [FieldOffset(0)] public HardwareInput hi;
        }

        public struct Input
        {
            public uint type;
            public InputUnion union;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;
        }
    }
}

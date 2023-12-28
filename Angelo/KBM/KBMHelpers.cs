using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Angelo.Screen;

namespace Angelo.KBM
{
    internal static class KBMHelpers
    {
        private enum InputType
        {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2
        }

        [Flags]
        private enum DWKeyboardFlags
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
        private enum DWMouseFlags
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
        private struct MouseInput
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardInput
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HardwareInput
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public MouseInput mi;
            [FieldOffset(0)] public KeyboardInput ki;
            [FieldOffset(0)] public HardwareInput hi;
        }

        private struct Input
        {
            public uint type;
            public InputUnion union;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;
        }

        /// <summary>
        /// Retrieve current cursor position.
        /// </summary>
        /// <param name="point">Will contain the position relative to top left corner of primary screen.</param>
        /// <returns>True if successful.</returns>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out Point point);

        /// <summary>
        /// Send keyboard input to currently active window.
        /// Will first send key down events for all given virtual keys, then key up events.
        /// </summary>
        /// <param name="vKeys">Array of virtual key codes.</param>
        /// <param name="releaseDelayMs">Optionally sleep the thread this amount of ms between down and up events.</param>
        /// /// <returns>True if all inputs were sent successfully.</returns>
        public static bool SendKeyboardInput(byte[] vKeys, int releaseDelayMs = 0)
        {
            Input[] inputs = new Input[vKeys.Length];

            for (int i = 0; i < inputs.Length; i++)
            {
                inputs[i] = new Input()
                {
                    type = (uint)InputType.Keyboard,
                    union = new InputUnion()
                    {
                        ki = new KeyboardInput()
                        {
                            wVk = vKeys[i],
                            dwFlags = (uint)DWKeyboardFlags.KeyDown,
                            dwExtraInfo = GetMessageExtraInfo(),
                        }
                    }
                };
            }

            uint resDown = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));

            // Failed to send any key down events, don't bother with keyup events.
            if (resDown == 0)
                return false;

            for (int i = 0; i < inputs.Length; i++)
            {
                inputs[i].union.ki.dwFlags = (uint)DWKeyboardFlags.KeyUp;
            }

            if (releaseDelayMs > 0)
                Thread.Sleep(releaseDelayMs);

            uint resUp = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));

            if (resDown != resUp)
            {
                MessageBox.Show("Successful key up events do not match key down events! Something may be broken.", "Keyboard Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Move mouse to absolute coordinates on the primary desktop.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>True if input was sent successfully.</returns>
        public static bool MoveMouseAbsolute(int x, int y)
        {
            var bounds = ScreenReader.GetPrimaryScreenRes();
            const int normalizedMax = 0xFFFF;
            double normDX = (double)x * normalizedMax / (bounds.Width - 1);
            double normDY = (double)y * normalizedMax / (bounds.Height - 1);
            int normalizedX = (int)Math.Ceiling(normDX);
            int normalizedY = (int)Math.Ceiling(normDY);

            Input[] inputs = new Input[1]
            {
                new Input()
                {
                    type = (uint)InputType.Mouse,
                    union = new InputUnion()
                    {
                        mi = new MouseInput
                        {
                            dx = normalizedX,
                            dy = normalizedY,
                            dwFlags = (uint)(DWMouseFlags.Move | DWMouseFlags.Absolute),
                            dwExtraInfo = GetMessageExtraInfo()
                        }
                    }
                },
            };

            uint res = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
            return res != 0;
        }

        /// <summary>
        /// Move mouse using relative movement. 
        /// Sets position with absolute values due to relative 
        /// movement being affected by mouse acceleration and speed.
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <returns>True if input was sent successfully.</returns>
        public static bool MoveMouseRelative(int dx, int dy)
        {
            GetCursorPos(out var cpos);
            return MoveMouseAbsolute(cpos.X + dx, cpos.Y + dy);
        }
    }
}

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Angelo.Screen;
using static Angelo.WinAPI.User32;
using static Angelo.WinAPI.User32Defs;

namespace Angelo.KBM
{
    internal static class KBMHelpers
    {
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
            var bounds = ScreenHelpers.GetScreenData();
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

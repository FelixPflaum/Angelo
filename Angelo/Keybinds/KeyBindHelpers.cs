using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace Angelo.Keybinds
{
    internal static class KeyBindHelpers
    {
        private const int STRING_OUTPUT_SIZE = 1;

        [DllImport("user32.dll")]
        private static extern int ToUnicode(
            uint virtualKeyCode,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = STRING_OUTPUT_SIZE)] StringBuilder outBuffer,
            int outBufferLen,
            uint wFlags);

        /// <summary>
        /// Get string from virtual key code.
        /// </summary>
        /// <param name="virtualKey">Virtual key code.</param>
        /// <returns>String corresponding to the virtual key.</returns>
        public static string GetCharFromVirtKey(byte virtualKey)
        {
            const int BUFFER_SIZE = STRING_OUTPUT_SIZE * 2;
            var buffer = new StringBuilder(BUFFER_SIZE);
            var state = new byte[255];
            int res = ToUnicode(virtualKey, 0, state, buffer, BUFFER_SIZE, 0);
            if (res < 1)
            {
                MessageBox.Show("Key can't be converted to a string! It could still work but won't be displayed correctly in the UI.");
                return "??";
            }
            return buffer.ToString().ToUpper();
        }
    }
}

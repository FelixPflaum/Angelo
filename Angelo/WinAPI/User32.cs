using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using static Angelo.WinAPI.User32Defs;

namespace Angelo.WinAPI
{
    internal static class User32
    {
        /// <summary>
        /// The EnumDisplayDevices function lets you obtain information about the display devices in the current session.
        /// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enumdisplaydevicesa
        /// </summary>
        /// <param name="lpDevice">A pointer to the device name. If NULL, function returns information for the 
        /// display adapter(s) on the machine, based on iDevNum.</param>
        /// <param name="iDevNum">An index value that specifies the display device of interest. 
        /// The operating system identifies each display device in the current session with an index value. 
        /// The index values are consecutive integers, starting at 0. If the current session has three display devices, 
        /// for example, they are specified by the index values 0, 1, and 2.</param>
        /// <param name="lpDisplayDevice">A pointer to a DISPLAY_DEVICE structure that receives information about the display device specified by iDevNum.
        /// Before calling EnumDisplayDevices, you must initialize the cb member of DISPLAY_DEVICE to the size, in bytes, of DISPLAY_DEVICE.</param>
        /// <param name="dwFlags">Set this flag to EDD_GET_DEVICE_INTERFACE_NAME (0x00000001) to retrieve the device interface name for GUID_DEVINTERFACE_MONITOR, 
        /// which is registered by the operating system on a per monitor basis. The value is placed in the DeviceID member of the DISPLAY_DEVICE structure returned 
        /// in lpDisplayDevice. The resulting device interface name can be used with SetupAPI functions and serves as a link between GDI monitor devices 
        /// and SetupAPI monitor devices.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.
        /// The function fails if iDevNum is greater than the largest device index.</returns>
        [DllImport("user32.dll")]
        public static extern bool EnumDisplayDevices(string? lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        /// <summary>
        /// The EnumDisplaySettings function retrieves information about one of the graphics modes for a display device. 
        /// To retrieve information for all the graphics modes of a display device, make a series of calls to this function.
        /// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enumdisplaysettingsa
        /// </summary>
        /// <param name="lpszDeviceName">A pointer to a null-terminated string that specifies the display device about whose 
        /// graphics mode the function will obtain information. This parameter is either NULL or a DISPLAY_DEVICE.DeviceName 
        /// returned from EnumDisplayDevices.A NULL value specifies the current display device on the computer on which the 
        /// calling thread is running.</param>
        /// <param name="iModeNum">The type of information to be retrieved. This value can be a graphics mode index or one 
        /// of the following values.</param>
        /// <param name="lpDevMode">A pointer to a DEVMODE structure into which the function stores information about the 
        /// specified graphics mode. Before calling EnumDisplaySettings, set the dmSize member to sizeof(DEVMODE), 
        /// and set the dmDriverExtra member to indicate the size, in bytes, of the additional space available to 
        /// receive private driver data.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.</returns>
        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, out DEVMODE lpDevMode);

        /// <summary>
        /// Synthesizes keystrokes, mouse motions, and button clicks.
        /// </summary>
        /// <param name="nInputs">The number of structures in the pInputs array.</param>
        /// <param name="pInputs">An array of INPUT structures. Each structure represents an event to be inserted into the keyboard or mouse input stream.</param>
        /// <param name="cbSize">The size, in bytes, of an INPUT structure. If cbSize is not the size of an INPUT structure, the function fails.</param>
        /// <returns>The function returns the number of events that it successfully inserted into the keyboard or mouse input stream. 
        /// If the function returns zero, the input was already blocked by another thread. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        /// <summary>
        /// Retrieves the extra message information for the current thread. Extra message information is an application- or driver-defined value associated with the current thread's message queue.
        /// </summary>
        /// <returns>The return value specifies the extra information. The meaning of the extra information is device specific.</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetMessageExtraInfo();

        /// <summary>
        /// Retrieve current cursor position.
        /// </summary>
        /// <param name="point">Will contain the position relative to top left corner of primary screen.</param>
        /// <returns>True if successful.</returns>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out Point point);

        /// <summary>
        /// Translates the specified virtual-key code and keyboard state to the corresponding Unicode character or characters.
        /// </summary>
        /// <param name="virtualKeyCode">The virtual-key code to be translated. See Virtual-Key Codes.</param>
        /// <param name="wScanCode">The hardware scan code of the key to be translated. The high-order bit of this value is set if the key is up.</param>
        /// <param name="lpKeyState">A pointer to a 256-byte array that contains the current keyboard state. 
        /// Each element (byte) in the array contains the state of one key.</param>
        /// <param name="outBuffer">The buffer that receives the translated character or characters as array of UTF-16 code units. 
        /// This buffer may be returned without being null-terminated even though the variable name suggests that it is null-terminated. 
        /// You can use the return value of this method to determine how many characters were written.</param>
        /// <param name="outBufferLen">The size, in characters, of the buffer pointed to by the pwszBuff parameter.</param>
        /// <param name="wFlags">The behavior of the function. If bit 0 is set, a menu is active. 
        /// In this mode Alt+Numeric keypad key combinations are not handled. 
        /// If bit 2 is set, keyboard state is not changed (Windows 10, version 1607 and newer) 
        /// All other bits (through 31) are reserved.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int ToUnicode(uint virtualKeyCode, uint wScanCode, byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 1)] StringBuilder outBuffer, int outBufferLen, uint wFlags);

        /// <summary>
        /// Retrieves a handle to the foreground window (the window with which the user is currently working). 
        /// The system assigns a slightly higher priority to the thread that creates the foreground window than it does to other threads.
        /// </summary>
        /// <returns>
        /// The return value is a handle to the foreground window. 
        /// The foreground window can be NULL in certain circumstances, such as when a window is losing activation.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// Copies the text of the specified window's title bar (if it has one) into a buffer. 
        /// If the specified window is a control, the text of the control is copied. 
        /// However, GetWindowText cannot retrieve the text of a control in another application.
        /// </summary>
        /// <param name="hWnd">A handle to the window or control containing the text.</param>
        /// <param name="text">The buffer that will receive the text. If the string is as long or longer than the buffer, the string is truncated and terminated with a null character.</param>
        /// <param name="count">The maximum number of characters to copy to the buffer, including the null character. If the text exceeds this limit, it is truncated.</param>
        /// <returns>
        /// If the function succeeds, the return value is the length, in characters, of the copied string, 
        /// not including the terminating null character. If the window has no title bar or text, 
        /// if the title bar is empty, or if the window or control handle is invalid, 
        /// the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpString, int count);

        /// <summary>
        /// Retrieves the length, in characters, of the specified window's title bar text (if the window has a title bar). 
        /// If the specified window is a control, the function retrieves the length of the text within the control. 
        /// However, GetWindowTextLength cannot retrieve the length of the text of an edit control in another application.
        /// </summary>
        /// <param name="hWnd">A handle to the window or control.</param>
        /// <returns>
        /// If the function succeeds, the return value is the length, in characters, of the text. 
        /// Under certain conditions, this value might be greater than the length of the text (see Remarks).
        /// If the window has no text, the return value is zero.
        /// Function failure is indicated by a return value of zero and a GetLastError result that is nonzero.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        /// <summary>
        /// Retrieves a handle to the top-level window whose class name and window name match the specified strings. 
        /// This function does not search child windows. This function does not perform a case-sensitive search.
        /// To search child windows, beginning with a specified child window, use the FindWindowEx function.
        /// </summary>
        /// <param name="lpClassName">
        /// The class name or a class atom created by a previous call to the RegisterClass or RegisterClassEx function. The atom must be in the low-order word of lpClassName; the high-order word must be zero.
        /// If lpClassName points to a string, it specifies the window class name. The class name can be any name registered with RegisterClass or RegisterClassEx, or any of the predefined control-class names.
        /// If lpClassName is NULL, it finds any window whose title matches the lpWindowName parameter.
        /// </param>
        /// <param name="lpWindowName">The window name (the window's title). If this parameter is NULL, all window names match.</param>
        /// <returns>
        /// If the function succeeds, the return value is a handle to the window that has the specified class name and window name.
        /// If the function fails, the return value is NULL. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow([Optional, MarshalAs(UnmanagedType.LPStr)] string? lpClassName, [Optional, MarshalAs(UnmanagedType.LPStr)] string? lpWindowName);
    }
}

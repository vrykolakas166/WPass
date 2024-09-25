using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;

namespace WPass.Utility
{
    public partial class EnumWindowsHelper
    {
        private static List<AutomationElement> BrowserElements { get; set; } = [];

        // Delegate to handle the windows enumerated
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [LibraryImport("user32.dll")]
        private static partial IntPtr GetForegroundWindow();

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        // Delegate for EnumChildWindows
        private delegate bool EnumChildProc(IntPtr hWnd, IntPtr lParam);

        // PInvoke for EnumChildWindows
        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool EnumChildWindows(IntPtr hWnd, EnumChildProc lpEnumFunc, IntPtr lParam);

        // PInvoke for SendMessage to get text from a control
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, StringBuilder? lParam);

        // Constants for SendMessage
        private const int WM_GETTEXT = 0x000D;
        private const int WM_GETTEXTLENGTH = 0x000E;

        public static List<AutomationElement> GetBrowserWindows()
        {
            BrowserElements.Clear();
            EnumWindows(EnumTheWindows, IntPtr.Zero);
            return BrowserElements;
        }

        public static AutomationElement GetFocusBrowserWindow()
        {
            IntPtr hwnd = GetForegroundWindow();

            var test = "";
            // Enumerate child windows (controls)
            EnumChildWindows(hwnd, (childHwnd, lParam) =>
            {
                // Get the length of the text in the child window
                int length = SendMessage(childHwnd, WM_GETTEXTLENGTH, IntPtr.Zero, null);

                if (length > 0)
                {
                    StringBuilder sb = new(length + 1);
                    _ = SendMessage(childHwnd, WM_GETTEXT, sb.Capacity, sb);

                    test += $"Child Window Text: {sb}\n";
                }

                return true; // Continue enumeration
            }, IntPtr.Zero);

            Console.WriteLine(test);

            return AutomationElement.FromHandle(hwnd);
        }

        private static bool EnumTheWindows(IntPtr hWnd, IntPtr lParam)
        {
            const int nChars = 256;
            StringBuilder className = new(nChars);
            StringBuilder windowText = new(nChars);

            // Get window title
            _ = GetWindowText(hWnd, windowText, nChars);

            // Get window class name
            _ = GetClassName(hWnd, className, nChars);

            // Check if the window is visible
            if (IsWindowVisible(hWnd))
            {
                // Detect popular browsers based on class name and title
                // note: Chrome, Brave, Arc, UC Browser, Vivaldi, Yandex Browser, Firefox, Tor Browser, Microsoft Edge, Internet Explorer
                if (className.ToString() == "Chrome_WidgetWin_1" ||  // Chrome, Brave, Arc, UC Browser, etc..
                    className.ToString() == "MozillaWindowClass" ||  // Firefox, Tor Browser
                    className.ToString() == "ApplicationFrameWindow" ||  // Edge (UWP)
                    className.ToString() == "IEFrame" ||  // Internet Explorer
                    windowText.ToString().Contains("Opera"))  // Opera (specific to title check)
                {
                    BrowserElements.Add(AutomationElement.FromHandle(hWnd));
                }
            }

            return true;
        }
    }

}
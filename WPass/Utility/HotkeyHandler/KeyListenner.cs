using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace WPass.Utility
{
    public static partial class KeyListenner
    {
        #region WinAPI
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool UnregisterHotKey(IntPtr hWnd, int id);
        #endregion

        #region Fields
        private const int WM_HOTKEY = 0x0312;
        private static readonly Dictionary<int, Action> _hotkeyActions = []; // Store hotkey actions by ID
        private static int _currentHotkeyId = 9000; // Start ID from 9000
        private static readonly Dictionary<int, DateTime> _lastExecutedTimes = []; // Store the last execution time for debouncing
        private static readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(500); // Debounce interval
        #endregion

        private static IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int hotkeyId = wParam.ToInt32();
                if (_hotkeyActions.TryGetValue(hotkeyId, out Action? action))
                {
                    DateTime now = DateTime.Now;

                    // Check if the action should be debounced
                    if (!_lastExecutedTimes.TryGetValue(hotkeyId, out DateTime value) || (now - value) > _debounceInterval)
                    {
                        action?.Invoke();
                        value = now;
                        _lastExecutedTimes[hotkeyId] = value; // Update the last executed time
                    }

                    handled = true; // Indicate that the hotkey was handled
                }
            }

            return IntPtr.Zero; // Return 0 for unhandled messages
        }

        public static int Register(Window window, ModifierKeys modifier, Key key, Action action)
        {
            IntPtr windowHandle = new WindowInteropHelper(window).Handle;
            HwndSource source = HwndSource.FromHwnd(windowHandle);
            source.AddHook(HwndHook); // Add the hook for hotkey messages

            int hotkeyId = _currentHotkeyId++;
            _hotkeyActions[hotkeyId] = action; // Register the hotkey action

            // Register the hotkey with the specified modifier and key
            RegisterHotKey(windowHandle, hotkeyId, (uint)modifier, (uint)KeyInterop.VirtualKeyFromKey(key));

            return hotkeyId; // Return the hotkey ID for reference
        }

        public static void Unregister(Window window, int hotkeyId)
        {
            IntPtr windowHandle = new WindowInteropHelper(window).Handle;
            UnregisterHotKey(windowHandle, hotkeyId); // Unregister the hotkey
            _hotkeyActions.Remove(hotkeyId); // Remove the hotkey action from the dictionary
        }

        public static void UnregisterAll(Window window)
        {
            IntPtr windowHandle = new WindowInteropHelper(window).Handle;
            foreach (var hotkeyId in _hotkeyActions.Keys)
            {
                UnregisterHotKey(windowHandle, hotkeyId); // Unregister each hotkey
            }
            _hotkeyActions.Clear(); // Clear all hotkey actions
        }
    }
}

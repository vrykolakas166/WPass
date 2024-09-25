using System.Runtime.InteropServices;

namespace WPass.Utility.WindowHandler;

public partial class KeyboardSimulator
{
    // Constants for virtual key codes for modifiers
    const int VK_LCONTROL = 0xA2;   // Left Control
    const int VK_RCONTROL = 0xA3;   // Right Control
    const int VK_CONTROL = 0x11;    // Control key (both left and right)

    // Shift keys
    const int VK_LSHIFT = 0xA0;     // Left Shift
    const int VK_RSHIFT = 0xA1;     // Right Shift
    const int VK_SHIFT = 0x10;      // Shift key (both left and right)

    // Alt keys (Menu keys)
    const int VK_LMENU = 0xA4;      // Left Alt (Menu key)
    const int VK_RMENU = 0xA5;      // Right Alt (Menu key)
    const int VK_MENU = 0x12;       // Alt key (both left and right)

    // Windows keys (Meta keys)
    const int VK_LWIN = 0x5B;       // Left Windows key
    const int VK_RWIN = 0x5C;       // Right Windows key

    // Other modifier keys
    const int VK_CAPITAL = 0x14;    // Caps Lock
    const int VK_NUMLOCK = 0x90;    // Num Lock
    const int VK_SCROLL = 0x91;     // Scroll Lock

    // TAB key
    const int VK_TAB = 0x09;  // Tab key

    // Input structure for SendInput API
    // Windows API structures for SendInput
    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public uint Type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)] public MOUSEINPUT Mi;
        [FieldOffset(0)] public KEYBDINPUT Ki;
        [FieldOffset(0)] public HARDWAREINPUT Hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public int Dx;
        public int Dy;
        public uint MouseData;
        public uint DwFlags;
        public uint Time;
        public IntPtr DwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort WVk;
        public ushort WScan;
        public uint DwFlags;
        public uint Time;
        public IntPtr DwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HARDWAREINPUT
    {
        public uint UMsg;
        public ushort WParamL;
        public ushort WParamH;
    }

    // Constants for SendInput
    public const int INPUT_KEYBOARD = 1;
    public const uint KEYEVENTF_KEYUP = 0x0002;
    public const uint KEYEVENTF_SCANCODE = 0x0008;

    [LibraryImport("user32.dll", SetLastError = true)]
    public static partial uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

    [LibraryImport("user32.dll")]
    private static partial ushort GetAsyncKeyState(int vKey);

    // Function to send a key
    // Represents the physical key on the keyboard, layout-dependent, hardware-level.
    public static void SendKey(ushort scanCode, bool isKeyUp = false)
    {
        INPUT[] inputs = new INPUT[1];
        inputs[0].Type = INPUT_KEYBOARD;
        inputs[0].U.Ki.WScan = scanCode;
        //inputs[0].U.Ki.DwFlags = isKeyUp ? KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP : KEYEVENTF_SCANCODE;
        inputs[0].U.Ki.DwFlags = isKeyUp ? KEYEVENTF_KEYUP : KEYEVENTF_SCANCODE;
        inputs[0].U.Ki.WVk = 0; // Set to 0 for non-VK (Virtual Key) usage
        SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
    }

    // Function to send a key
    // Represents the logical meaning of a key, layout-independent, operating system-level.
    public static void SendKey(int virtualKey, bool isKeyUp)
    {
        INPUT[] inputs = new INPUT[1];
        inputs[0].Type = INPUT_KEYBOARD;
        inputs[0].U.Ki.WVk = (ushort)virtualKey;
        inputs[0].U.Ki.WScan = 0;
        inputs[0].U.Ki.DwFlags = isKeyUp ? KEYEVENTF_KEYUP : 0;
        SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
    }

    // Method to send a full sentence
    public static void SendSentence(string sentence)
    {
        foreach (char c in sentence)
        {
            SendCharacter(c);
        }
    }

    // Helper method to map characters to scan codes and send them
    public static void SendCharacter(char character)
    {
        bool shiftRequired = char.IsUpper(character) || "~!@#$%^&*()_+{}|:\"<>?".Contains(character);

        ushort scanCode = CharToScanCode(character);

        if (shiftRequired)
        {
            SendKeyWithModifier(VK_LSHIFT, scanCode);
        }
        else
        {
            // Simulate key down
            SendKey(scanCode);

            // Simulate key up
            SendKey(scanCode, true);
        }
    }

    // Check if Ctrl is pressed
    private static bool IsCtrlPressed()
    {
        return (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0;
    }

    // Function to check if the Alt key is pressed
    private static bool IsAltPressed()
    {
        return (GetAsyncKeyState(VK_MENU) & 0x8000) != 0;
    }

    // Release the Ctrl key if it's pressed
    public static void ReleaseCtrlKey()
    {
        if (IsCtrlPressed())
        {
            SendKey(VK_LCONTROL, true);
        }
    }

    // Release the Ctrl key if it's pressed
    public static void ReleaseAltKey()
    {
        if (IsAltPressed())
        {
            SendKey(VK_LMENU, true);
        }
    }

    // Function to send a key with modifier (Ctrl, Alt, Shift)
    public static void SendKeyWithModifier(int modifierKey, ushort targetKey)
    {
        // Press the modifier key (e.g., Ctrl, Alt, Shift)
        SendKey(modifierKey, false); // Key down for modifier

        // Press the target key (e.g., 'A' for Ctrl + A)
        SendKey(targetKey, false); // Key down for target key
        SendKey(targetKey, true);  // Key up for target key

        // Release the modifier key
        SendKey(modifierKey, true); // Key up for modifier
    }

    public static void SendTabKey()
    {
        SendKey(VK_TAB, false); // Tab key down
        SendKey(VK_TAB, true);  // Tab key up
    }

    public static ushort CharToScanCode(char c)
    {
        return c switch
        {
            // Lowercase letters
            'a' => 0x1E,
            'b' => 0x30,
            'c' => 0x2E,
            'd' => 0x20,
            'e' => 0x12,
            'f' => 0x21,
            'g' => 0x22,
            'h' => 0x23,
            'i' => 0x17,
            'j' => 0x24,
            'k' => 0x25,
            'l' => 0x26,
            'm' => 0x32,
            'n' => 0x31,
            'o' => 0x18,
            'p' => 0x19,
            'q' => 0x10,
            'r' => 0x13,
            's' => 0x1F,
            't' => 0x14,
            'u' => 0x16,
            'v' => 0x2F,
            'w' => 0x11,
            'x' => 0x2D,
            'y' => 0x15,
            'z' => 0x2C,
            // Uppercase letters (Shift required)
            'A' => 0x1E,
            'B' => 0x30,
            'C' => 0x2E,
            'D' => 0x20,
            'E' => 0x12,
            'F' => 0x21,
            'G' => 0x22,
            'H' => 0x23,
            'I' => 0x17,
            'J' => 0x24,
            'K' => 0x25,
            'L' => 0x26,
            'M' => 0x32,
            'N' => 0x31,
            'O' => 0x18,
            'P' => 0x19,
            'Q' => 0x10,
            'R' => 0x13,
            'S' => 0x1F,
            'T' => 0x14,
            'U' => 0x16,
            'V' => 0x2F,
            'W' => 0x11,
            'X' => 0x2D,
            'Y' => 0x15,
            'Z' => 0x2C,
            // Digits
            '0' => 0x0B,
            '1' => 0x02,
            '2' => 0x03,
            '3' => 0x04,
            '4' => 0x05,
            '5' => 0x06,
            '6' => 0x07,
            '7' => 0x08,
            '8' => 0x09,
            '9' => 0x0A,
            // Symbols and punctuation
            ' ' => 0x39,// Spacebar
            '.' => 0x34,// Period
            ',' => 0x33,// Comma
            ';' => 0x27,// Semicolon
            ':' => 0x27,// Colon (Shift required)
            '/' => 0x35,// Slash
            '?' => 0x35,// Question mark (Shift required)
            '-' => 0x0C,// Hyphen
            '_' => 0x0C,// Underscore (Shift required)
            '=' => 0x0D,// Equals
            '+' => 0x0D,// Plus (Shift required)
            '[' => 0x1A,// Open bracket
            ']' => 0x1B,// Close bracket
            '\\' => 0x2B,// Backslash
            '\'' => 0x28,// Single quote
            '\"' => 0x28,// Double quote (Shift required)
            '`' => 0x29,// Backtick
            '~' => 0x29,// Tilde (Shift required)
            '!' => 0x02,// Exclamation mark (Shift required)
            '@' => 0x03,// At symbol (Shift required)
            '#' => 0x04,// Hash (Shift required)
            '$' => 0x05,// Dollar (Shift required)
            '%' => 0x06,// Percent (Shift required)
            '^' => 0x07,// Caret (Shift required)
            '&' => 0x08,// Ampersand (Shift required)
            '*' => 0x09,// Asterisk (Shift required)
            '(' => 0x0A,// Open parenthesis (Shift required)
            ')' => 0x0B,// Close parenthesis (Shift required)
            _ => 0x00,// Unsupported characters
        };
    }

    public static void SendUsernameAndPassword(string username, string password)
    {
        ReleaseCtrlKey();
        ReleaseAltKey();

        // Simulate typing
        SendSentence(username);
        SendTabKey();
        SendSentence(password);
    }
}

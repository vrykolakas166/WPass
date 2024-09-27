using System.Windows.Input;

namespace WPass.Utility.HotkeyHandler
{
    public class KeyCollector : IDisposable
    {
        private Key NonModifierKey = Key.None;

        // Changed to a dictionary to manage Ctrl and Alt
        public readonly Dictionary<Key, bool> Modifiers = new()
        {
            { Key.LeftCtrl, false },
            { Key.RightCtrl, false },
            { Key.LeftAlt, false },
            { Key.RightAlt, false }
        };

        /// <summary>
        /// Reset default
        /// </summary>
        public void Reset()
        {
            NonModifierKey = Key.None;
            foreach (var key in Modifiers.Keys.ToList())
            {
                Modifiers[key] = false;
            }
        }

        /// <summary>
        /// Capture pressed keys
        /// </summary>
        /// <param name="k"></param>
        public bool Capture(Key k)
        {
            // Check for modifiers
            if (Modifiers.ContainsKey(k)) // check ctrl press first
            {
                Modifiers[k] = true; // Set the corresponding modifier key to true
            }
            // Handle non-modifier key
            else if (!IsModifierKey(k) && Modifiers.ContainsValue(true))
            {
                NonModifierKey = k;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clear pressed keys when they were released
        /// </summary>
        /// <param name="k"></param>
        public void Release(Key k)
        {
            // Check for modifiers
            if (Modifiers.ContainsKey(k))
            {
                Modifiers[k] = false;
            }
            // Handle non-modifier key
            else if (!IsModifierKey(k))
            {
                NonModifierKey = Key.None;
            }
        }

        /// <summary>
        /// Get user combination hotkeys in string
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public string GetCombination(KeyEventArgs e)
        {
            string hotkeyText = "";

            if (Modifiers[Key.LeftCtrl] || Modifiers[Key.RightCtrl])
                hotkeyText += "Ctrl + ";

            if (Modifiers[Key.LeftAlt] || Modifiers[Key.RightAlt])
                hotkeyText += "Alt + ";

            if (NonModifierKey != Key.None)
                hotkeyText += ConvertKeyToChar(NonModifierKey, e.KeyboardDevice.Modifiers);

            return hotkeyText;
        }

        /// <summary>
        /// Method to get Key from character string
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public Key? ConvertCharToKey(string character)
        {
            if (_charToKeyMap.TryGetValue(character, out Key key))
            {
                return key;
            }

            // If character not found, return null
            return null;
        }

        #region Private methods

        /// <summary>
        /// Method to get string from key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="modifiers"></param>
        /// <returns></returns>
        private static string ConvertKeyToChar(Key key, ModifierKeys modifiers)
        {
            // Handle Oem and special cases
            if (key == Key.OemQuotes) return "'";
            if (key == Key.Oem3) return "`";
            if (key == Key.OemComma) return ",";
            if (key == Key.OemPeriod) return ".";
            if (key == Key.OemMinus) return "-";
            if (key == Key.OemPlus) return "=";

            if (key == Key.Oem1) return modifiers.HasFlag(ModifierKeys.Shift) ? "!" : "1";
            if (key == Key.Oem2) return modifiers.HasFlag(ModifierKeys.Shift) ? "?" : "/";
            if (key == Key.Oem4) return modifiers.HasFlag(ModifierKeys.Shift) ? "{" : "[";
            if (key == Key.Oem5) return modifiers.HasFlag(ModifierKeys.Shift) ? "|" : "\\";
            if (key == Key.Oem6) return modifiers.HasFlag(ModifierKeys.Shift) ? "}" : "]";
            if (key == Key.Oem7) return modifiers.HasFlag(ModifierKeys.Shift) ? "\"" : "'";
            if (key == Key.Oem8) return "#";  // '#' on some layouts
            if (key == Key.Oem102) return "<";  // Less than on some keyboards

            // Numeric keys with Shift (to show symbols)
            if (key == Key.D0) return modifiers.HasFlag(ModifierKeys.Shift) ? ")" : "0";
            if (key == Key.D1) return modifiers.HasFlag(ModifierKeys.Shift) ? "!" : "1";
            if (key == Key.D2) return modifiers.HasFlag(ModifierKeys.Shift) ? "@" : "2";
            if (key == Key.D3) return modifiers.HasFlag(ModifierKeys.Shift) ? "#" : "3";
            if (key == Key.D4) return modifiers.HasFlag(ModifierKeys.Shift) ? "$" : "4";
            if (key == Key.D5) return modifiers.HasFlag(ModifierKeys.Shift) ? "%" : "5";
            if (key == Key.D6) return modifiers.HasFlag(ModifierKeys.Shift) ? "^" : "6";
            if (key == Key.D7) return modifiers.HasFlag(ModifierKeys.Shift) ? "&" : "7";
            if (key == Key.D8) return modifiers.HasFlag(ModifierKeys.Shift) ? "*" : "8";
            if (key == Key.D9) return modifiers.HasFlag(ModifierKeys.Shift) ? "(" : "9";

            // Default handling for other keys
            KeyConverter keyConverter = new();
            return keyConverter.ConvertToString(key) ?? string.Empty;
        }

        /// <summary>
        /// Dictionary to map characters to Key values
        /// </summary>
        private readonly Dictionary<string, Key> _charToKeyMap = BuildCharToKeyMap();

        /// <summary>
        /// Method to build the dictionary using ConvertKeyToChar logic
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, Key> BuildCharToKeyMap()
        {
            var map = new Dictionary<string, Key>();

            // Iterate over all possible keys and populate the dictionary
            foreach (Key key in Enum.GetValues(typeof(Key)))
            {
                if (!IsModifierKey(key)) // Ignore modifier keys
                {
                    var character = ConvertKeyToChar(key, ModifierKeys.None);
                    if (!string.IsNullOrEmpty(character) && !map.ContainsKey(character))
                    {
                        map[character] = key;
                    }

                    // Handle Shift + keys for symbols
                    var shiftCharacter = ConvertKeyToChar(key, ModifierKeys.Shift);
                    if (!string.IsNullOrEmpty(shiftCharacter) && !map.ContainsKey(shiftCharacter))
                    {
                        map[shiftCharacter] = key;
                    }
                }
            }

            return map;
        }

        /// <summary>
        /// Method to check if key is modifier key or not
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsModifierKey(Key key)
        {
            return key == Key.LeftCtrl || key == Key.RightCtrl ||
                   key == Key.LeftAlt || key == Key.RightAlt ||
                   key == Key.LeftShift || key == Key.RightShift ||
                   key == Key.LWin || key == Key.RWin;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

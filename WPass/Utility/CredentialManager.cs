using System.Windows;
using System.Windows.Automation;
using WPass.Core.Model;
using WPass.Utility.OtherHandler;
using WPass.Utility.SecurityHandler;
using WPass.Utility.WindowHandler;

namespace WPass.Utility
{
    public static class CredentialManager
    {
        private static readonly string[] _addressBars = ["Address and search bar", "Address field"];
        /// <summary>
        /// Set data on fields
        /// </summary>
        /// <param name="isClear">false mean set, true mean clear</param>
        public static void SetData(bool isClear = false)
        {
            var currentUrl = string.Empty;
            bool usernameIsSet = false;
            bool passwordIsSet = false;
            var windowName = "";
            windowName += $"1.AutomationElement.FocusedElement.Current.Name: {AutomationElement.FocusedElement.Current.Name}\n\n";
            var str = "Name, ProgrammaticName \n";

            string? oUsername;
            try
            {
                List<AutomationElement> browserWindows = [];

                // right now: allow to fill credentials on focus browser
                var focusedBrowser = EnumWindowsHelper.GetFocusBrowserWindow();
                if (focusedBrowser != null)
                {
                    browserWindows.Add(focusedBrowser);
                }
                //else
                //{
                //    //// Find all browser windows (like Chrome, Edge, Firefox)
                //    browserWindows = EnumWindowsHelper.GetBrowserWindows();
                //}

                foreach (AutomationElement window in browserWindows) // only 1 focused element
                {
                    windowName += "(" + window.Current.ClassName + " - " + window.Current.Name + "); ";
                    var isEdge = window.Current.Name.Contains("Microsoft​ Edge"); // UI Automation support Edge only

                    AutomationElementCollection elements;

                    if (isEdge)
                    {
                        elements = window.FindAll(TreeScope.Descendants, System.Windows.Automation.Condition.TrueCondition);
                    }
                    else
                    {
                        var editCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit);
                        elements = window.FindAll(TreeScope.Children, editCondition);
                    }

                    string? oPassword = null;
                    foreach (AutomationElement element in elements)
                    {
                        str += $"\"{element.Current.Name}\", \"{element.Current.ControlType.ProgrammaticName}\" \n";

                        // check current url
                        if (_addressBars.Contains(element.Current.Name))
                        {
                            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object pattern))
                            {
                                var valuePattern = (ValuePattern)pattern;
                                currentUrl = valuePattern.Current.Value;
                                if (isEdge)
                                {
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        windowName += $"\n{window.Current.Name}: " + currentUrl;
                        // continue; // collect info when debug
                        if (isEdge)
                        {
                            // fill data
                            if (!usernameIsSet)
                            {
                                usernameIsSet = SetUsername(element, currentUrl, out oUsername, isClear);
                            }
                            if (!passwordIsSet)
                            {
                                passwordIsSet = SetPassword(element, currentUrl, out oPassword, isClear);
                            }

                            if (usernameIsSet && passwordIsSet && !isClear)
                            {
                                break;
                            }
                            else if (usernameIsSet && passwordIsSet && isClear)
                            {
                                break;
                            }
                        }
                    }

                    if (!isEdge && !isClear)
                    {
                        _ = SetUsername(null, currentUrl, out oUsername, isClear);
                        _ = SetPassword(null, currentUrl, out oPassword, isClear);
                        // if not set
                        // go here
                        // using oUsername and oPassword and using keyboard simulator
                        KeyboardSimulator.SendUsernameAndPassword(oUsername, oPassword);
                        usernameIsSet = true;
                        passwordIsSet = true;

                        // case Edge but using keyboard simulator
                        var usernameIsSetBySimulator = false;
                        if (!usernameIsSet)
                        {
                            // log
                            Logger.Write("Cannot find username element. Using keyboard simulator instead.");

                            // if cannot find username element using keyboard simulator instead
                            // and current focused element is a edit control (username input specifically)
                            if (!string.IsNullOrEmpty(oUsername) && AutomationElement.FocusedElement.Current.ControlType == ControlType.Edit)
                            {
                                KeyboardSimulator.ReleaseCtrlKey();
                                KeyboardSimulator.ReleaseAltKey();
                                KeyboardSimulator.SendSentence(oUsername);
                                usernameIsSetBySimulator = true;
                            }
                        }
                        if (!passwordIsSet)
                        {
                            // log
                            Logger.Write($"Cannot find password element. Using keyboard simulator instead.");
                            if (!string.IsNullOrEmpty(oPassword) && AutomationElement.FocusedElement.Current.ControlType == ControlType.Edit && usernameIsSetBySimulator)
                            {
                                KeyboardSimulator.SendTabKey();
                                KeyboardSimulator.SendSentence(oPassword);
                            }
                        }
                    }

                    // Free password
                    if (oPassword != null)
                    {
                        // Overwrite the password with random data or zeros
                        unsafe
                        {
                            fixed (char* ptr = oPassword)
                            {
                                for (int i = 0; i < oPassword.Length; i++)
                                {
                                    ptr[i] = '\0'; // Overwrite with null characters
                                }
                            }
                        }
                    }
                    oPassword = null; // Nullify the reference
                    // save website info to log in order to investigate
                    Logger.Write(str, "elements.csv");
                    Logger.Write(windowName, "windows.txt");
                }
            }
            catch (Exception ex)
            {
                // log error
                Logger.Write(ex.Message);
            }
        }

        public static bool IsSameWebsite(List<Website> websites, string currentUrl)
        {
            try
            {
                var rs1 = string.Empty;
                var rs2 = string.Empty;
                if (!string.IsNullOrEmpty(currentUrl))
                {
                    Uri uri = new(currentUrl);
                    rs2 = uri.GetLeftPart(UriPartial.Authority);
                }

                foreach (Website website in websites)
                {
                    rs1 = string.Empty;
                    var savedUrl = website.Url;

                    if (!string.IsNullOrEmpty(savedUrl))
                    {
                        Uri uri = new(savedUrl);
                        rs1 = uri.GetLeftPart(UriPartial.Authority);
                    }

                    if (rs1 == rs2 && !string.IsNullOrEmpty(rs1) && !string.IsNullOrEmpty(rs2))
                    {
                        return true;
                    }
                    else
                    {
                        continue;
                    }
                }

                return false;
            }
            catch { return false; }
        }

        public static bool SetUsername(AutomationElement? element, string currentUrl, out string username, bool isClear = false)
        {
            username = string.Empty;
            if (element != null)
            {
                if (GlobalSession.BrowserElements.Any(field => field.Name.Equals(element.Current.Name, StringComparison.OrdinalIgnoreCase)) && element.Current.ControlType.Equals(ControlType.Edit))
                {
                    // Check if the element supports ValuePattern
                    if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object pattern))
                    {
                        var valuePattern = (ValuePattern)pattern;
                        if (!isClear)
                        {
                            if (GlobalSession.EntryDtos.Count > 0)
                            {
                                foreach (var entry in GlobalSession.EntryDtos)
                                {
                                    if (IsSameWebsite([.. entry.Websites], currentUrl))
                                    {
                                        valuePattern.SetValue(entry.Username);
                                        username = entry.Username;
                                        return true;
                                    }
                                }

                                valuePattern.SetValue(GlobalSession.DefaultEntry?.Username ?? "");
                                username = GlobalSession.DefaultEntry?.Username ?? "";
                                return true;
                            }
                            return false;
                        }
                        else
                        {
                            valuePattern.SetValue(string.Empty);
                        }
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Elements are not supported on this website.");
                        return false;
                    }
                }
            }

            // if cannot find element, simulate keyboard
            if (GlobalSession.EntryDtos.Count > 0)
            {
                foreach (var entry in GlobalSession.EntryDtos)
                {
                    if (IsSameWebsite([.. entry.Websites], currentUrl))
                    {
                        username = entry.Username;
                    }
                }

                username = GlobalSession.DefaultEntry?.Username ?? "";
            }

            return false;
        }

        public static bool SetPassword(AutomationElement? element, string currentUrl, out string password, bool isClear = false)
        {
            password = string.Empty;
            if (element != null)
            {
                if ((element.Current.Name.Equals("password", StringComparison.OrdinalIgnoreCase) || element.Current.Name.Equals("Mật khẩu", StringComparison.OrdinalIgnoreCase)) && element.Current.ControlType.Equals(ControlType.Edit))
                {
                    // Check if the element supports ValuePattern
                    if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object pattern))
                    {
                        var valuePattern = (ValuePattern)pattern;
                        if (!isClear)
                        {
                            if (GlobalSession.EntryDtos.Count > 0)
                            {
                                foreach (var entry in GlobalSession.EntryDtos)
                                {
                                    if (IsSameWebsite([.. entry.Websites], currentUrl))
                                    {
                                        valuePattern.SetValue(Security.Decrypt(entry.EncryptedPassword));
                                        password = Security.Decrypt(entry.EncryptedPassword);
                                        return true;
                                    }
                                }

                                valuePattern.SetValue(Security.Decrypt(GlobalSession.DefaultEntry?.EncryptedPassword ?? ""));
                                password = Security.Decrypt(GlobalSession.DefaultEntry?.EncryptedPassword ?? "");
                                return true;
                            }
                            return false;
                        }
                        else
                        {
                            valuePattern.SetValue(string.Empty);
                        }
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Elements are not supported on this website.");
                        return false;
                    }
                }
            }

            // if cannot find element, simulate keyboard
            if (GlobalSession.EntryDtos.Count > 0)
            {
                foreach (var entry in GlobalSession.EntryDtos)
                {
                    if (IsSameWebsite([.. entry.Websites], currentUrl))
                    {
                        password = Security.Decrypt(entry.EncryptedPassword);
                    }
                }

                password = Security.Decrypt(GlobalSession.DefaultEntry?.EncryptedPassword ?? "");
            }

            return false;
        }
    }
}

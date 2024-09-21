using System.Windows;
using System.Windows.Automation;
using WPass.Core.Model;

namespace WPass.Utility
{
    public static class CredentialManager
    {
        /// <summary>
        /// Set data on fields
        /// </summary>
        /// <param name="isClear">false mean set, true mean clear</param>
        public static void SetData(bool isClear = false)
        {
            try
            {
                var currentUrl = string.Empty;

                bool usernameIsSet = false;
                bool passwordIsSet = false;

                // Get the root Automation element
                AutomationElement rootElement = AutomationElement.RootElement;

                // Find all browser windows (like Chrome, Edge, Firefox)
                var condition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window);
                AutomationElementCollection browserWindows = rootElement.FindAll(TreeScope.Children, condition);

                foreach (AutomationElement window in browserWindows)
                {
                    // Filter out based on the process name (e.g., "chrome", "msedge", "firefox")
                    if (window.Current.Name.Contains("Chrome") || window.Current.Name.Contains("Edge"))
                    {
                        // Get the URL bar element or any focused element
                        AutomationElement focusedElement = AutomationElement.FocusedElement;

                        var elements = window.FindAll(TreeScope.Descendants, System.Windows.Automation.Condition.TrueCondition);
                        var str = "Name, ProgrammaticName \n";
                        foreach (AutomationElement element in elements)
                        {
                            str += $"\"{element.Current.Name}\", \"{element.Current.ControlType.ProgrammaticName}\" \n";

                            // check current url
                            if (element.Current.Name == "Address and search bar")
                            {
                                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object pattern))
                                {
                                    var valuePattern = (ValuePattern)pattern;
                                    currentUrl = valuePattern.Current.Value;
                                }
                            }

                            // fill data
                            if (!usernameIsSet)
                            {
                                usernameIsSet = SetUsername(element, currentUrl, isClear);
                            }
                            if (!passwordIsSet)
                            {
                                passwordIsSet = SetPassword(element, currentUrl, isClear);
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

                        if (!usernameIsSet)
                        {
                            // log
                            // MessageBox.Show("Cannot find username element.\n This website's elements will be sent to developer in order to fix this in the future.\n Sorry for you inconvenience.");
                        }
                        else if (!passwordIsSet)
                        {
                            // log
                            // MessageBox.Show("Cannot find password element.\n This website's elements will be sent to developer in order to fix this in the future.\n Sorry for you inconvenience.");
                        }

                        // save website info to log in order to investigate
                        //File.Create("C:\\Users\\hhtbb\\Desktop\\New folder\\text.csv").Close();
                        //File.WriteAllText("C:\\Users\\hhtbb\\Desktop\\New folder\\text.csv", str);
                    }
                }
            }
            catch
            {
                // log error
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

        public static bool SetUsername(AutomationElement element, string currentUrl, bool isClear = false)
        {
            if (GlobalSession.BrowserElements.Any(field => field.Name.Equals(element.Current.Name, StringComparison.OrdinalIgnoreCase)) && element.Current.ControlType.Equals(ControlType.Edit))
            {
                // Check if the element supports ValuePattern
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object pattern))
                {
                    var valuePattern = (ValuePattern)pattern;
                    if (!isClear)
                    {
                        // get from db
                        if (GlobalSession.EntryDtos.Count > 0)
                        {
                            foreach (var entry in GlobalSession.EntryDtos)
                            {
                                if (IsSameWebsite([.. entry.Websites], currentUrl))
                                {
                                    valuePattern.SetValue(entry.Username);
                                    return true;
                                }
                            }

                            valuePattern.SetValue(GlobalSession.DefaultEntry?.Username ?? "");
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

            return false;
        }

        public static bool SetPassword(AutomationElement element, string currentUrl, bool isClear = false)
        {
            if (element.Current.Name.ToLower().Equals("password") && element.Current.ControlType.Equals(ControlType.Edit))
            {
                // Check if the element supports ValuePattern
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object pattern))
                {
                    var valuePattern = (ValuePattern)pattern;
                    if (!isClear)
                    {
                        // get from db
                        if (GlobalSession.EntryDtos.Count > 0)
                        {
                            foreach (var entry in GlobalSession.EntryDtos)
                            {
                                if (IsSameWebsite([.. entry.Websites], currentUrl))
                                {
                                    valuePattern.SetValue(entry.Password);
                                    return true;
                                }
                            }

                            valuePattern.SetValue(GlobalSession.DefaultEntry?.Password ?? "");
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

            return false;
        }
    }
}

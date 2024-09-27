using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ViewModels.Base;
using WPass.Constant;
using WPass.Core;
using WPass.Core.Model;
using WPass.Utility;
using WPass.Utility.OtherHandler;
using WPass.Utility.WindowHandler;

namespace WPass.ViewModels
{
    public class SettingVM : BaseViewModel
    {
        private static string _lastSavedData = string.Empty;

        // Browser Elements section
        private string _browserElementsString;
        public string BrowserElementsString
        {
            get => _browserElementsString;
            set
            {
                _browserElementsString = value;
                OnPropertyChanged(nameof(BrowserElementsString));
            }
        }

        // Hotkeys section
        private string _hotkeyFill;
        public string HotkeyFill
        {
            get => _hotkeyFill;
            set
            {
                _hotkeyFill = value;
                OnPropertyChanged(nameof(HotkeyFill));
            }
        }

        private string _hotkeyClear;
        public string HotkeyClear
        {
            get => _hotkeyClear;
            set
            {
                _hotkeyClear = value;
                OnPropertyChanged(nameof(HotkeyClear));
            }
        }

        // Others section
        private bool _hideOnClose;
        public bool HideOnClose
        {
            get => _hideOnClose;
            set
            {
                _hideOnClose = value;
                OnPropertyChanged(nameof(HideOnClose));
            }
        }

        private bool _windowStartup;
        public bool WindowStartup
        {
            get => _windowStartup;
            set
            {
                _windowStartup = value;
                OnPropertyChanged(nameof(WindowStartup));
            }
        }

        public ICommand UpdatePasscodeCommand { get; set; }
        public ICommand ResetPasscodeCommand { get; set; }

        public ICommand ResetCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        public SettingVM()
        {
            _browserElementsString = string.Empty;
            _hotkeyFill = string.Empty;
            _hotkeyClear = string.Empty;

            LoadData();

            UpdatePasscodeCommand = new BaseCommand<Window>(c => true, OpenUpdatePasscode);
            ResetPasscodeCommand = new BaseCommand<Window>(c => true, ResetPasscodeAsync);

            ResetCommand = new BaseCommand<Window>(c => true, Reset);
            SaveCommand = new BaseCommand<Window>(c => CanSave(), Save);
        }

        private async Task ResetPasscodeAsync(Window w)
        {
            await PasscodeManager.ResetAsync();
            w.Close();
        }

        private static void OpenUpdatePasscode(Window w)
        {
            new LoginWindow(LoginWindow.Mode.Change).ShowDialog();
            w.Close();
        }

        private void Reset(Window w)
        {
            try
            {
                BrowserElementsString = string.Join("; ", JsonConvert.DeserializeObject<List<BrowserElement>>(BElement.DEFAULT_JSON)?.Select(e => e.Name) ?? []);

                HotkeyFill = "Ctrl + `";
                if (w.FindName("ButtonChangeHotkey_FillData") is Button b1)
                {
                    b1.Content = HotkeyFill;
                }

                HotkeyClear = "Ctrl + Q";
                if (w.FindName("ButtonChangeHotkey_ClearData") is Button b2)
                {
                    b2.Content = HotkeyClear;
                }

                HideOnClose = false;
                WindowStartup = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something's wrong. Please check log.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Write(ex.Message);
            }
        }

        public bool CanSave()
        {
            return _lastSavedData != GetChangedDataSignal();
        }

        private async Task Save(Window w)
        {
            List<BrowserElement>? oldbe;
            string? oldFill;
            string? oldClear;
            List<BrowserElement>? browserElements;
            List<Core.Model.Setting>? settings;
            var defaultBrowserElements = JsonConvert.DeserializeObject<List<BrowserElement>>(BElement.DEFAULT_JSON) ?? [];

            try
            {
                using WPContext context = new();
                oldbe = await context.BrowserElements.ToListAsync();
                oldFill = context.Settings.First(s => s.Key.Equals(Constant.Setting.HOTKEY_FILL_DATA)).Value;
                oldClear = context.Settings.First(s => s.Key.Equals(Constant.Setting.HOTKEY_CLEAR_DATA)).Value;

                browserElements = BEStringToList(BrowserElementsString);
                settings = await context.Settings
                    .OrderBy(setting => setting.Key)
                    .ToListAsync();

                context.BrowserElements.RemoveRange(oldbe);

                foreach (var item in defaultBrowserElements)
                {
                    var check = context.BrowserElements.Find(item.Name);
                    if (check == null)
                    {
                        await context.BrowserElements.AddAsync(item);
                    }
                }

                foreach (var browserElement in browserElements)
                {
                    if (!defaultBrowserElements.Any(dbe => dbe.Name.Equals(browserElement.Name)))
                    {
                        await context.BrowserElements.AddAsync(new BrowserElement() { Name = browserElement.Name });
                    }
                }

                for (var i = 0; i < settings.Count; i++)
                {
                    var setting = settings[i];
                    switch (setting.Key)
                    {
                        case Constant.Setting.HOTKEY_FILL_DATA:
                            setting.Value = HotkeyFill.ToString();
                            break;
                        case Constant.Setting.HOTKEY_CLEAR_DATA:
                            setting.Value = HotkeyClear.ToString();
                            break;
                        case Constant.Setting.HIDE_ON_CLOSE:
                            setting.Value = HideOnClose.ToString().ToLower();
                            break;
                        case Constant.Setting.WINDOW_STARTUP:
                            setting.Value = WindowStartup.ToString().ToLower();
                            break;
                        default:
                            break;
                    }
                    context.Settings.Update(setting);
                }

                await context.SaveChangesAsync();
                LoadData();

                // Check if hotkeys change
                // Ask to restart
                if (oldFill != HotkeyFill || oldClear != HotkeyClear)
                {
                    if (MessageBox.Show("Hotkeys are changed, do you want to restart now ?", "Changes", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        TriggerHelper.RestartCurrentApplication();
                    }
                }

                w?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something's wrong. Please check log.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Write(ex.Message);
            }
            finally
            {
                oldbe = null;
                oldFill = null;
                oldClear = null;
                browserElements = null;
                settings = null;
                defaultBrowserElements = null;
            }
        }

        private void LoadData()
        {
            _lastSavedData = string.Empty;

            using WPContext context = new();

            var browserElements = context.BrowserElements.ToList();
            GlobalSession.BrowserElements = browserElements; // saved to session
            var settings = context.Settings
                .OrderBy(setting => setting.Key)
                .ToList();

            _lastSavedData = BEListToString(browserElements);

            foreach (var s in settings)
            {
                if (!s.Key.Equals(Constant.Setting.PASSCODE)) // ignore passcode section
                {
                    _lastSavedData += s.Value;
                }
                switch (s.Key)
                {
                    case Constant.Setting.HOTKEY_FILL_DATA:
                        HotkeyFill = s.Value;
                        break;
                    case Constant.Setting.HOTKEY_CLEAR_DATA:
                        HotkeyClear = s.Value;
                        break;
                    case Constant.Setting.HIDE_ON_CLOSE:
                        HideOnClose = s.Value == "true";
                        break;
                    case Constant.Setting.WINDOW_STARTUP:
                        WindowStartup = s.Value == "true";
                        break;
                    default:
                        break;
                }
            }

            BrowserElementsString = BEListToString(browserElements);
        }

        private string GetChangedDataSignal()
        {
            // order by alphabet
            var str = string.Empty;
            str += BrowserElementsString;
            str += HideOnClose.ToString().ToLower();
            str += HotkeyClear;
            str += HotkeyFill;
            str += WindowStartup.ToString().ToLower();

            return str;
        }

        private static List<BrowserElement> BEStringToList(string str)
        {
            List<BrowserElement> list = [];
            var l = str.Trim().Split(";").Select(s => s.Trim()).ToList();
            foreach (var item in l)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    list.Add(new BrowserElement()
                    {
                        Name = item,
                    });
                }
            }
            return list;
        }

        private static string BEListToString(List<BrowserElement> list)
        {
            return string.Join("; ", list.Select(l => l.Name));
        }
    }
}

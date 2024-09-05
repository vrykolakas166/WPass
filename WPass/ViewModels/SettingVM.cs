using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ViewModels.Base;
using WPass.Constant;
using WPass.Core;
using WPass.Core.Model;

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

        public ICommand ResetCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        public SettingVM()
        {
            _browserElementsString = string.Empty;
            _hotkeyFill = string.Empty;
            _hotkeyClear = string.Empty;

            LoadData();

            ResetCommand = new BaseCommand<Window>(c => true, Reset);
            SaveCommand = new BaseCommand<Window>(c => CanSave(), Save);
        }

        private void Reset(Window w)
        {
            BrowserElementsString = string.Join("; ", [BElement.DEFAULT_2, BElement.DEFAULT_1, BElement.DEFAULT_0]);

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

        private bool CanSave()
        {
            return _lastSavedData != GetChangedDataSignal();
        }

        private async Task Save(Window w)
        {
            WPContext context = new();
            var oldbe = await context.BrowserElements.ToListAsync();
            var browserElements = BEStringToList(BrowserElementsString);
            var settings = await context.Settings
                .OrderBy(setting => setting.Key)
                .ToListAsync();

            foreach (var be in oldbe)
            {
                context.BrowserElements.Remove(be);
            }
            await context.BrowserElements.AddAsync(new BrowserElement() { Name = BElement.DEFAULT_0 });
            await context.BrowserElements.AddAsync(new BrowserElement() { Name = BElement.DEFAULT_1 });
            await context.BrowserElements.AddAsync(new BrowserElement() { Name = BElement.DEFAULT_2 });
            foreach (var browserElement in browserElements)
            {
                if (browserElement.Name != BElement.DEFAULT_0 &&
                    browserElement.Name != BElement.DEFAULT_1 &&
                    browserElement.Name != BElement.DEFAULT_2)
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
            w?.Close();
        }

        private void LoadData()
        {
            _lastSavedData = string.Empty;

            WPContext context = new();

            var browserElements = context.BrowserElements.ToList();
            var settings = context.Settings
                .OrderBy(setting => setting.Key)
                .ToList();

            _lastSavedData = BEListToString(browserElements);

            foreach (var s in settings)
            {
                _lastSavedData += s.Value;
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

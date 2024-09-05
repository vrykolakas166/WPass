using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ViewModels.Base;
using WPass.Constant;
using WPass.Core;
using WPass.Core.Model;
using WPass.DTO;
using WPass.Utility;

namespace WPass.ViewModels
{
    public class MainVM : BaseViewModel
    {
        private static Window? _mainWindow;
        private static List<EntryDto> _originList = [];
        private static List<Tuple<string, string>> _stringSumList = [];

        public static int HOTKEY_FILL_DATA { get; set; }
        public static int HOTKEY_CLEAR_DATA { get; set; }

        private bool _notifyIconVisible;
        public bool NotifyIconVisible
        {
            get => _notifyIconVisible;
            set
            {
                _notifyIconVisible = value;
                OnPropertyChanged(nameof(NotifyIconVisible));
            }
        }

        private ObservableCollection<EntryDto> _entries = [];
        public ObservableCollection<EntryDto> Entries
        {
            get => _entries;
            set
            {
                _entries = value;
                OnPropertyChanged(nameof(Entries));
            }
        }

        private string _filteredSearchValue;
        public string FilteredSearchValue
        {
            get => _filteredSearchValue;
            set
            {
                _filteredSearchValue = value;
                OnPropertyChanged(nameof(FilteredSearchValue));
            }
        }

        private string _selectedSortMode;
        public string SelectedSortMode
        {
            get => _selectedSortMode;
            set
            {
                _selectedSortMode = value;
                OnPropertyChanged(nameof(SelectedSortMode));
            }
        }

        private List<string> _sortModes;
        public List<string> SortModes
        {
            get => _sortModes;
            set
            {
                _sortModes = value;
                OnPropertyChanged(nameof(SortModes));
            }
        }

        private int _totalEntries;
        public int TotalEntries
        {
            get => _totalEntries;
            set
            {
                _totalEntries = value;
                OnPropertyChanged(nameof(TotalEntries));
            }
        }

        public ICommand AddOrUpdateEntryCommand { get; set; }
        public ICommand RemoveEntryCommand { get; set; }
        public ICommand ManageCommand { get; set; }

        public ICommand FilterSearchCommand { get; set; }

        public ICommand NotifyIconOpenCommand { get; set; }
        public ICommand ShowNotifyIconCommand { get; set; }
        public ICommand NotifyIconExitCommand { get; set; }

        public ICommand FileImportCommand { get; set; }
        public ICommand OpenSettingCommand { get; set; }

        public MainVM()
        {
            _filteredSearchValue = string.Empty;
            _selectedSortMode = SortMode.DEFAULT;
            _sortModes =
            [
                SortMode.DEFAULT,
                SortMode.ASC,
                SortMode.DESC
            ];

            NotifyIconVisible = false;

            AddOrUpdateEntryCommand = new BaseCommand<string>(c => true, AddOrUpdateEntry);
            RemoveEntryCommand = new BaseCommand<string>(c => true, RemoveEntry);
            ManageCommand = new BaseCommand<Button>(c => true, Manage);

            FilterSearchCommand = new BaseCommand<object>(c => true, c => FilterSearch());

            NotifyIconOpenCommand = new BaseCommand<object>(c => true, c => OpenFromNotifyIcon());
            ShowNotifyIconCommand = new BaseCommand<object>(c => true, c => ShowNotifyIcon());
            NotifyIconExitCommand = new BaseCommand<object>(c => true, c => ExitFromNotifyIcon());

            FileImportCommand = new BaseCommand<object>(c => true, async c => await ImportFile());
            OpenSettingCommand = new BaseCommand<object>(c => true, c => OpenSetting());
            // Initialize default data
            SeedData();

            LoadData().Wait();
        }

        private void FilterSearch()
        {
            Entries.Clear();
            if (string.IsNullOrEmpty(FilteredSearchValue))
            {
                foreach (var entry in _originList)
                {
                    Entries.Add(entry);
                }
            }
            else
            {
                foreach (var tup in _stringSumList.Where(item => item.Item2.Contains(FilteredSearchValue, StringComparison.CurrentCultureIgnoreCase)))
                {
                    if (!Entries.Any(e => e.Id.Equals(tup.Item1)))
                    {
                        Entries.Add(_originList.First(e => e.Id.Equals(tup.Item1)));
                    }
                }
            }
            TotalEntries = Entries.Count;
        }

        private void Manage(Button button)
        {
            ContextMenu contextMenu = new();
            MenuItem item1 = new() { Header = "Select all" };
            MenuItem item2 = new() { Header = "Cancel select" };
            MenuItem item3 = new() { Header = "Remove selected entries" };

            item1.Click += (s, args) =>
            {
                foreach (var entry in Entries)
                {
                    entry.IsSelected = true;
                }
            };

            item2.Click += (s, args) =>
            {
                foreach (var entry in Entries)
                {
                    entry.IsSelected = false;
                }
            };

            item3.Click += async (s, args) =>
            {
                if (Entries.Any(e => e.IsSelected))
                {
                    if (MessageBox.Show(_mainWindow, "Are you sure to delete all selected entries?", "Entries", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        var context = new WPContext();
                        foreach (var entry in Entries)
                        {
                            if (entry.IsSelected)
                            {
                                var deletedEntry = await context.Entries.FindAsync(entry.Id);
                                if (deletedEntry != null)
                                {
                                    context.Entries.Remove(deletedEntry);
                                }
                            }
                        }
                        var rs = await context.SaveChangesAsync();
                        if (rs > 0)
                        {
                            await LoadData();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select at least one entry.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            contextMenu.Items.Add(item1);
            contextMenu.Items.Add(item2);
            contextMenu.Items.Add(item3);

            // Show the ContextMenu below the button
            contextMenu.PlacementTarget = button;
            contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            contextMenu.IsOpen = true;
        }

        private async Task ImportFile()
        {
            try
            {
                System.Windows.Forms.OpenFileDialog dialog = new()
                {
                    Filter = "Data|*.csv",
                    Multiselect = false,
                };
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var (entries, websites) = await DataImport.ReadCSV(dialog.FileName);
                    WPContext context = new();
                    var oldEntries = await context.Entries.ToListAsync();
                    var oldWebsites = await context.Websites.ToListAsync();
                    if (entries.Count > 0)
                    {
                        foreach (var entry in entries)
                        {
                            if (oldEntries.Count > 0)
                            {
                                foreach (var oldEntry in oldEntries)
                                {
                                    if (entry.Username != oldEntry.Username &&
                                        Security.Decrypt(entry.EncryptedPassword) != Security.Decrypt(oldEntry.EncryptedPassword))
                                    {
                                        // add new entry
                                        await context.Entries.AddAsync(entry);
                                    }
                                    else
                                    {
                                        // change new website to belong to existed entry
                                        var newWebs = websites.Where(w => w.EntryId == entry.Id).ToList();
                                        foreach (var item in newWebs)
                                        {
                                            item.EntryId = oldEntry.Id;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                await context.Entries.AddAsync(entry);
                            }
                        }
                    }
                    if (websites.Count > 0)
                    {
                        foreach (var web in websites)
                        {
                            if (oldWebsites.Count > 0)
                            {
                                foreach (var oldWeb in oldWebsites)
                                {
                                    if (web.EntryId != oldWeb.EntryId &&
                                        web.Url != oldWeb.Url)
                                    {
                                        await context.Websites.AddAsync(web);
                                    }
                                }
                            }
                            else
                            {
                                await context.Websites.AddAsync(web);
                            }
                        }
                    }
                    var rs = await context.SaveChangesAsync();
                    if (rs > 0) await LoadData();
                }
            }
            catch
            {
                // log
                MessageBox.Show("Something's wrong. Please contact dev :)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void OpenSetting()
        {
            SettingWindow setting = new()
            {
                Owner = _mainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            setting.ShowDialog();
        }

        private static void ExitFromNotifyIcon()
        {
            MainWindow.ForceToClose = true;
            _mainWindow?.Close();
        }

        private void OpenFromNotifyIcon()
        {
            _mainWindow?.Show();
            _mainWindow?.BringIntoView();
            _mainWindow?.Activate();
            _mainWindow?.Focus();

            NotifyIconVisible = false;
        }

        private void ShowNotifyIcon()
        {
            ToastHelper.Show("Hidden", "App is running in taskbar.");
            _mainWindow?.Hide();
            NotifyIconVisible = true;
        }

        public static void Initialize(Window window)
        {
            _mainWindow = window;
            WPContext context = new();
            var settings = context.Settings.ToList();
            {
                foreach (var setting in settings)
                {
                    switch (setting.Key)
                    {
                        case Constant.Setting.HOTKEY_FILL_DATA:
                            var tempArr = setting.Value.Split(" + ");
                            if (tempArr.Length == 2)
                            {
                                HOTKEY_FILL_DATA = KeyListenner.Register(window, ModifierKeys.Control, KeyCollector.ConvertCharToKey(tempArr[1]) ?? Key.Oem3, () => CredentialManager.FillData());
                            }
                            else if (tempArr.Length == 3)
                            {
                                HOTKEY_FILL_DATA = KeyListenner.Register(window, ModifierKeys.Control | ModifierKeys.Alt, KeyCollector.ConvertCharToKey(tempArr[2]) ?? Key.Oem3, () => CredentialManager.FillData());
                            }
                            break;
                        case Constant.Setting.HOTKEY_CLEAR_DATA: // clear
                            tempArr = setting.Value.Split(" + ");
                            if (tempArr.Length == 2)
                            {
                                HOTKEY_CLEAR_DATA = KeyListenner.Register(window, ModifierKeys.Control, KeyCollector.ConvertCharToKey(tempArr[1]) ?? Key.Q, () => CredentialManager.FillData(true));
                            }
                            else if (tempArr.Length == 3)
                            {
                                HOTKEY_CLEAR_DATA = KeyListenner.Register(window, ModifierKeys.Control | ModifierKeys.Alt, KeyCollector.ConvertCharToKey(tempArr[2]) ?? Key.Q, () => CredentialManager.FillData(true));
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public static void Destroy(Window window)
        {
            _mainWindow = null;
            KeyListenner.UnregisterAll(window);
        }

        public async Task LoadData()
        {
            var context = new WPContext();
            Entries.Clear();
            var entries = await context.Entries
                .Include(e => e.Websites)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
            foreach (var entry in entries)
            {
                Entries.Add(EntryDto.MapFrom(entry));
                _originList.Add(EntryDto.MapFrom(entry));
            }

            TotalEntries = Entries.Count;

            // get summary string to for filter search function
            _stringSumList = [];
            foreach (var item in _originList)
            {
                _stringSumList.Add(new Tuple<string, string>(item.Id, item.Username));
                if (item.Websites.Any())
                {
                    foreach (var web in item.Websites)
                    {
                        _stringSumList.Add(new Tuple<string, string>(item.Id, web.Url));
                    }
                }
            }
        }

        private async Task AddOrUpdateEntry(string id)
        {
            EntryDetailWindow window = new(id)
            {
                Title = string.IsNullOrEmpty(id) ? "New" : "Update",
                Owner = _mainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            window.ShowDialog();
            await LoadData();
            if (!string.IsNullOrEmpty(FilteredSearchValue))
            {
                FilterSearch();
            }
        }

        private async Task RemoveEntry(string id)
        {
            if (MessageBox.Show(_mainWindow, "Are you sure to delete this?", "Entry", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var context = new WPContext();
                var deletedEntry = await context.Entries.FindAsync(id);
                if (deletedEntry != null)
                {
                    context.Entries.Remove(deletedEntry);

                    await context.SaveChangesAsync();
                    await LoadData();
                }
            }
        }

        private static void SeedData()
        {
            var context = new WPContext();
            var browserElement1 = new BrowserElement() { Name = BElement.DEFAULT_0 };
            var browserElement2 = new BrowserElement() { Name = BElement.DEFAULT_1 };
            var browserElement3 = new BrowserElement() { Name = BElement.DEFAULT_2 };

            var check1 = context.BrowserElements.Find(browserElement1.Name);
            var check2 = context.BrowserElements.Find(browserElement2.Name);
            var check3 = context.BrowserElements.Find(browserElement3.Name);

            if (check1 == null)
            {
                context.BrowserElements.Add(browserElement1);
            }
            if (check2 == null)
            {
                context.BrowserElements.Add(browserElement2);
            }
            if (check3 == null)
            {
                context.BrowserElements.Add(browserElement3);
            }

            if (!context.Settings.Any())
            {
                context.Settings.Add(new Core.Model.Setting()
                {
                    Key = Constant.Setting.HOTKEY_FILL_DATA,
                    Value = "Ctrl + `"
                });
                context.Settings.Add(new Core.Model.Setting()
                {
                    Key = Constant.Setting.HOTKEY_CLEAR_DATA,
                    Value = "Ctrl + Q"
                });
                context.Settings.Add(new Core.Model.Setting()
                {
                    Key = Constant.Setting.HIDE_ON_CLOSE,
                    Value = "false"
                });
                context.Settings.Add(new Core.Model.Setting()
                {
                    Key = Constant.Setting.WINDOW_STARTUP,
                    Value = "false"
                });
            }

            context.SaveChanges();
        }
    }
}

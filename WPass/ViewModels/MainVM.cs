using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ViewModels.Base;
using WPass.Constant;
using WPass.Core;
using WPass.DTO;
using WPass.Utility;

namespace WPass.ViewModels
{
    public class MainVM : BaseViewModel
    {
        private static Window? _mainWindow;
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
        public ICommand SetDefaultEntryCommand { get; set; }

        public ICommand FilterSearchCommand { get; set; }

        public ICommand NotifyIconOpenCommand { get; set; }
        public ICommand ShowNotifyIconCommand { get; set; }
        public ICommand NotifyIconExitCommand { get; set; }

        public ICommand FileImportCommand { get; set; }
        public ICommand OpenSettingCommand { get; set; }

        public MainVM()
        {
            GlobalSession.EntryDtos = [];

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
            SetDefaultEntryCommand = new BaseCommand<string>(CanSetDefault, SetDefaultEntry);

            FilterSearchCommand = new BaseCommand<object>(c => true, c => FilterSearch());

            NotifyIconOpenCommand = new BaseCommand<object>(c => true, c => OpenFromNotifyIcon());
            ShowNotifyIconCommand = new BaseCommand<object>(c => true, c => ShowNotifyIcon());
            NotifyIconExitCommand = new BaseCommand<object>(c => true, c => ExitFromNotifyIcon());

            FileImportCommand = new BaseCommand<object>(c => true, async c => await ImportFile());
            OpenSettingCommand = new BaseCommand<object>(c => true, c => OpenSetting());

            LoadData().Wait();
        }

        private bool CanSetDefault(string id)
        {
            return id != GlobalSession.DefaultEntry?.Id;
        }

        private async Task SetDefaultEntry(string id)
        {
            using var context = new WPContext();
            var newEntry = await context.Entries.FindAsync(id);
            var oldEntry = await context.Entries.FindAsync(GlobalSession.DefaultEntry?.Id);
            if (newEntry != null)
            {
                newEntry.IsDefault = true;
                context.Entries.Update(newEntry);

                if (oldEntry != null)
                {
                    oldEntry.IsDefault = false;
                    context.Entries.Update(oldEntry);
                }

                await context.SaveChangesAsync();
                await LoadData();
            }
        }

        private void FilterSearch()
        {
            Entries.Clear();
            if (string.IsNullOrEmpty(FilteredSearchValue))
            {
                foreach (var entry in GlobalSession.EntryDtos)
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
                        Entries.Add(GlobalSession.EntryDtos.First(e => e.Id.Equals(tup.Item1)));
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
            catch (Exception ex)
            {
                // log
                MessageBox.Show("Something's wrong. Please contact dev :)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Write(ex.Message);
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
                                HOTKEY_FILL_DATA = KeyListenner.Register(window, ModifierKeys.Control, KeyCollector.ConvertCharToKey(tempArr[1]) ?? Key.Oem3, () => CredentialManager.SetData());
                            }
                            else if (tempArr.Length == 3)
                            {
                                HOTKEY_FILL_DATA = KeyListenner.Register(window, ModifierKeys.Control | ModifierKeys.Alt, KeyCollector.ConvertCharToKey(tempArr[2]) ?? Key.Oem3, () => CredentialManager.SetData());
                            }
                            break;
                        case Constant.Setting.HOTKEY_CLEAR_DATA: // clear
                            tempArr = setting.Value.Split(" + ");
                            if (tempArr.Length == 2)
                            {
                                HOTKEY_CLEAR_DATA = KeyListenner.Register(window, ModifierKeys.Control, KeyCollector.ConvertCharToKey(tempArr[1]) ?? Key.Q, () => CredentialManager.SetData(true));
                            }
                            else if (tempArr.Length == 3)
                            {
                                HOTKEY_CLEAR_DATA = KeyListenner.Register(window, ModifierKeys.Control | ModifierKeys.Alt, KeyCollector.ConvertCharToKey(tempArr[2]) ?? Key.Q, () => CredentialManager.SetData(true));
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

        public async Task LoadData(bool isFilter = false)
        {
            bool notFound = true;
            var context = new WPContext();
            Entries.Clear();
            if (!isFilter)
            {
                GlobalSession.EntryDtos.Clear();
                _stringSumList = [];
            }

            var entries = await context.Entries
                .Include(e => e.Websites)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            foreach (var entry in entries)
            {
                var item = EntryDto.MapFrom(entry);
                if (item.IsDefault)
                {
                    GlobalSession.DefaultEntry = item;
                    Entries.Insert(0, item);
                    GlobalSession.EntryDtos.Insert(0, item);
                    notFound = false;
                }
                else
                {
                    Entries.Add(item);
                    GlobalSession.EntryDtos.Add(item);
                }
            }

            // no item map default, mean it was deleted, remove default and try to add first item in last list
            if (notFound)
            {
                var firstAdded = Entries.FirstOrDefault();
                if (firstAdded != null)
                {
                    var item = await context.Entries.FirstAsync(e => e.Id.Equals(firstAdded.Id));
                    item.IsDefault = true;
                    context.Entries.Update(item);
                    await context.SaveChangesAsync();
                    Entries[0].IsDefault = true;
                    GlobalSession.EntryDtos[0].IsDefault = true;
                    GlobalSession.DefaultEntry = EntryDto.MapFrom(item);
                }
                else // empty
                {
                    GlobalSession.DefaultEntry = null;
                }
            }

            TotalEntries = Entries.Count;

            // get summary string to for filter search function
            foreach (var item in GlobalSession.EntryDtos)
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
                    if (!string.IsNullOrEmpty(FilteredSearchValue))
                    {
                        FilterSearch();
                    }
                }
            }
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ViewModels.Base;
using WPass.Core;
using WPass.Core.Model;
using WPass.DTO;
using WPass.Utility.OtherHandler;
using WPass.Utility.SecurityHandler;

namespace WPass.ViewModels
{
    public class EntryDetailVM : BaseViewModel
    {
        private bool _lastSaveStep = false;

        private EntryDto _entry;
        public EntryDto Entry
        {
            get => _entry;
            set
            {
                _entry = value;
                OnPropertyChanged(nameof(Entry));
            }
        }

        private string _websiteSectionTitle = string.Empty;
        public string WebsiteSectionTitle
        {
            get => _websiteSectionTitle;
            set
            {
                _websiteSectionTitle = value;
                OnPropertyChanged(nameof(WebsiteSectionTitle));
            }
        }

        private bool _threeMonthsRemind;
        public bool ThreeMonthsRemind
        {
            get => _threeMonthsRemind;
            set
            {
                _threeMonthsRemind = value;
                OnPropertyChanged(nameof(ThreeMonthsRemind));
            }
        }

        private bool _isAllApplied;
        public bool IsAllApplied
        {
            get => _isAllApplied;
            set
            {
                _isAllApplied = value;
                OnPropertyChanged(nameof(IsAllApplied));
            }
        }

        private bool _isPasswordEnabled;
        public bool IsPasswordEnabled
        {
            get => _isPasswordEnabled;
            set
            {
                _isPasswordEnabled = value;
                OnPropertyChanged(nameof(IsPasswordEnabled));
            }
        }

        private string _cancelTitle = "Cancel";
        public string CancelTitle
        {
            get => _cancelTitle;
            set
            {
                _cancelTitle = value;
                OnPropertyChanged(nameof(CancelTitle));
            }
        }

        private string _saveTitle = "Save";
        public string SaveTitle
        {
            get => _saveTitle;
            set
            {
                _saveTitle = value;
                OnPropertyChanged(nameof(SaveTitle));
            }
        }

        private bool _optionSectionVisible;
        public bool OptionSectionVisible
        {
            get => _optionSectionVisible;
            set
            {
                _optionSectionVisible = value;
                OnPropertyChanged(nameof(OptionSectionVisible));
            }
        }

        public ICommand ClearPasswordCommand { get; set; }

        public ICommand CancelCommand { get; set; }
        public ICommand RemoveWebsiteCommand { get; set; }
        public ICommand AddWebsiteCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        public EntryDetailVM(string? id)
        {
            _optionSectionVisible = false;
            _isAllApplied = true;
            _isPasswordEnabled = true;
            _entry = new EntryDto();

            using WPContext context = new();
            var existedEntry = context.Entries
                .Include(e => e.Websites)
                .FirstOrDefault(e => e.Id.Equals(id));
            if (existedEntry != null)
            {
                _entry = EntryDto.MapFrom(existedEntry);
                _websiteSectionTitle = $"Websites ({_entry.Websites.Count})";
                _isPasswordEnabled = false;
            }

            ClearPasswordCommand = new BaseCommand<PasswordBox>(c => true, ClearPassword);

            CancelCommand = new BaseCommand<Window>(c => CanCancel(), Cancel);
            RemoveWebsiteCommand = new BaseCommand<WebsiteDto>(c => true, RemoveWebsite);
            AddWebsiteCommand = new BaseCommand<ScrollViewer>(c => CanAddWebsite(), AddWebsite);
            SaveCommand = new BaseCommand<Window>(c => CanSave(), Save);
        }

        private void ClearPassword(PasswordBox pb)
        {
            pb?.Clear();
            IsPasswordEnabled = true;
            if (_entry.Websites.Count > 1)
            {
                OptionSectionVisible = true;
            }
        }

        private bool CanCancel()
        {
            if (_lastSaveStep && _optionSectionVisible)
            {
                CancelTitle = "Back";
            }
            else
            {
                CancelTitle = "Cancel";
            }

            return true;
        }

        private void Cancel(Window w)
        {
            if (_lastSaveStep)
            {
                if (w.FindName("EntryDetailScrollViewer") is ScrollViewer viewer)
                {
                    AnimateScrollToOffset(viewer, viewer.HorizontalOffset - 300);
                    _lastSaveStep = false;
                }
                return;
            }


            w?.Close();
        }

        private bool CanSave()
        {
            if (_isAllApplied || _lastSaveStep || !_optionSectionVisible)
            {
                SaveTitle = "Save";
            }
            else
            {
                SaveTitle = "Next";
            }

            return true;
        }

        private bool CanAddWebsite()
        {
            if (Entry.Websites.Count == 0) return true;

            return !string.IsNullOrEmpty(Entry.Websites[^1].Url);
        }

        private void RemoveWebsite(WebsiteDto web)
        {
            Entry.Websites.Remove(web);
            WebsiteSectionTitle = $"Websites ({GetCountFromWebsiteSection(WebsiteSectionTitle) - 1})";
        }

        private void AddWebsite(ScrollViewer? viewer)
        {
            Entry.Websites.Add(new WebsiteDto
            {
                EntryId = Entry.Id,
            });
            viewer?.ScrollToEnd();

            WebsiteSectionTitle = $"Websites ({GetCountFromWebsiteSection(WebsiteSectionTitle) + 1})";
        }

        private async Task Save(Window w)
        {
            using WPContext context = new();
            IDbContextTransaction? trans = null;

            try
            {
                // Validation
                var (isValid, validationMessage) = IsValidEntry();

                // Step if not update all
                if (!IsAllApplied && !_lastSaveStep && _optionSectionVisible)
                {
                    if (isValid)
                    {
                        if (w.FindName("EntryDetailScrollViewer") is ScrollViewer viewer)
                        {
                            AnimateScrollToOffset(viewer, 300 + viewer.HorizontalOffset);
                            _lastSaveStep = true;
                        }
                    }
                    else
                    {
                        MessageBox.Show(validationMessage, "Error");
                    }
                }
                else // otherwise
                {
                    trans = await context.Database.BeginTransactionAsync();

                    if (IsPasswordEnabled)
                    {
                        PasswordBox? passwordBox = w.FindName("EntryPasswordBox") as PasswordBox;
                        Entry.EncryptedPassword = Security.Encrypt(passwordBox?.Password ?? string.Empty);
                    }

                    if (isValid)
                    {
                        var entry = await context.Entries
                            .Include(e => e.Websites)
                            .FirstOrDefaultAsync(e => e.Id.Equals(Entry.Id));

                        // create
                        if (entry == null)
                        {
                            var newEntry = new Entry()
                            {
                                Username = Entry.Username,
                                EncryptedPassword = Entry.EncryptedPassword,
                            };

                            var exists = context.Entries
                                .Where(e => e.Username.Equals(newEntry.Username));
                            var existed = false;
                            foreach (var item in exists)
                            {
                                if (Security.AreTheSame(item.EncryptedPassword, newEntry.EncryptedPassword))
                                {
                                    existed = true;
                                    break;
                                }
                            }

                            // Entry (with username and password) is not existed
                            // Add new
                            if (!existed)
                            {
                                await context.Entries.AddAsync(newEntry);
                                if (context.Entries.Count() > 500)
                                {
                                    MessageBox.Show("For best performance, the application only store maximum 500 entries. Please check again");
                                    trans.Rollback();
                                    return;
                                }

                                foreach (var website in Entry.Websites)
                                {
                                    var newWebsite = new Website
                                    {
                                        EntryId = newEntry.Id,
                                        Url = website.Url
                                    };
                                    await context.Websites.AddAsync(newWebsite);
                                }

                                await context.SaveChangesAsync();
                                await trans.CommitAsync();
                            }
                            else // otherwise cancel.
                            {
                                MessageBox.Show("Sorry, entry was created before.\nIf you want to add more websites to entry or update password on specific websites, please find and update it instead.",
                                    "Warning",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                            }
                        }
                        // update
                        else
                        {
                            if (IsAllApplied)
                            {
                                entry.Username = Entry.Username;
                                entry.EncryptedPassword = Entry.EncryptedPassword;

                                // check view websites empty => delete all
                                // entry.Websites > Entry.Websites => delete and update
                                // entry.Websites <= Entry.Websites => add and update

                                foreach (var website in entry.Websites)
                                {
                                    context.Websites.Remove(website);
                                }

                                foreach (var website in Entry.Websites)
                                {
                                    var newWebsite = new Website()
                                    {
                                        EntryId = entry.Id,
                                        Url = website.Url
                                    };
                                    await context.Websites.AddAsync(newWebsite);
                                }

                                context.Entries.Update(entry);
                            }
                            else
                            {
                                if (entry.Username == Entry.Username && Security.Decrypt(entry.EncryptedPassword) == Security.Decrypt(Entry.EncryptedPassword))
                                {
                                    MessageBox.Show("Username or password must be changed in order to apply to specific websites.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    trans.Rollback();
                                    return;
                                }

                                // update entry when websites are added or removed
                                var (addeds, deleteds) = GetChangedWebsites(entry);
                                foreach(var item in addeds)
                                {
                                    var newWebsite = new Website()
                                    {
                                        EntryId = entry.Id,
                                        Url = item.Url
                                    };
                                    await context.Websites.AddAsync(newWebsite);
                                }
                                foreach (var item in deleteds)
                                {
                                    context.Websites.Remove(item);
                                }

                                // update entry with specific websites
                                var newEntry = new Entry()
                                {
                                    Username = Entry.Username,
                                    EncryptedPassword = Entry.EncryptedPassword,
                                };
                                await context.Entries.AddAsync(newEntry);

                                if(context.Entries.Count() > 500)
                                {
                                    MessageBox.Show("For best performance, the application only store maximum 500 entries. Please check again");
                                    trans.Rollback();
                                    return;
                                }

                                // remove checked websites in current entry and create new entry to store checked websites
                                foreach (var website in entry.Websites)
                                {
                                    foreach (var viewWebsite in Entry.Websites)
                                    {
                                        // If wesbites in database contains view websites and checked
                                        // remove from database
                                        // add removed ones to a list then add to new entry
                                        if (viewWebsite.IsChecked && website.Url.Equals(viewWebsite.Url) && (website.EntryId?.Equals(viewWebsite.EntryId) ?? false))
                                        {
                                            website.EntryId = newEntry.Id;
                                        }
                                    }
                                }
                            }

                            await context.SaveChangesAsync();
                            await trans.CommitAsync();
                        }

                        // end operation
                        w?.Close();
                    }
                    else
                    {
                        MessageBox.Show(validationMessage, "Error");
                        trans.Rollback();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex.Message);
                trans?.Rollback();
            }
        }

        private (bool Result, string Message) IsValidEntry()
        {
            if (string.IsNullOrEmpty(Entry.Username))
            {
                return (false, "Username cannot be empty.");
            }

            var hashWeb = new HashSet<string>();

            foreach (var website in Entry.Websites)
            {
                if (string.IsNullOrEmpty(website.Url))
                {
                    return (false, "Website's url cannot be empty.");
                }

                bool isValid = Uri.TryCreate(website.Url, UriKind.Absolute, out Uri? uriResult)
                                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (!isValid)
                {
                    return (false, "Invalid website's url");
                }

                if (!hashWeb.Add(website.Url))
                {
                    return (false, "Duplicated website's url found.");
                }
            }
            return (true, string.Empty);
        }

        private static int GetCountFromWebsiteSection(string input)
        {
            // Define a regex pattern to find the number inside the parentheses
            string pattern = @"\((\d+)\)";

            // Use Regex.Match to extract the number
            Match match = Regex.Match(input, pattern);

            // If a match is found, return the number, otherwise return 0
            if (match.Success && int.TryParse(match.Groups[1].Value, out int count))
            {
                return count;
            }

            return 0; // Return 0 if no valid number is found
        }

        private (List<WebsiteDto> addedWebsites, List<Website> deletedWebsites) GetChangedWebsites(Entry currentEntry)
        {
            var addedWebsites = Entry.Websites
                .Where(newWeb => !currentEntry.Websites
                .Any(w => w.Url.Equals(newWeb.Url) && (w.EntryId?.Equals(newWeb.EntryId) ?? false)))
                .ToList();

            var deletedWebsites = currentEntry.Websites
                .Where(oldWeb => !Entry.Websites
                .Any(w => w.Url.Equals(oldWeb.Url) && (w.EntryId?.Equals(oldWeb.EntryId) ?? false)))
                .ToList();

            return (addedWebsites, deletedWebsites);
        }

        #region ScrollViewer Animation setup
        private const double ANIMATED_TIME = 0.75;



        // Method to animate horizontal scrolling to a target offset
        private static void AnimateScrollToOffset(ScrollViewer scrollViewer, double toOffset)
        {
            DoubleAnimation animation = new()
            {
                To = toOffset,
                Duration = TimeSpan.FromSeconds(ANIMATED_TIME), // Animation duration
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut } // Optional easing function for smoothness
            };

            // Apply the animation to the horizontal offset property
            scrollViewer.BeginAnimation(HorizontalOffsetProperty, animation);
        }

        // Attached property to animate ScrollViewer's HorizontalOffset
        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.RegisterAttached(
            "HorizontalOffset",
            typeof(double),
            typeof(EntryDetailWindow),
            new PropertyMetadata(0.0, OnHorizontalOffsetChanged));

        // Callback to set the ScrollViewer's HorizontalOffset when the property changes
        private static void OnHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToHorizontalOffset((double)e.NewValue);
            }
        }
        #endregion
    }
}

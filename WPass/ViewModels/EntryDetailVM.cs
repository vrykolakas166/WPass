using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        public ICommand ClearPasswordCommand { get; set; }

        public ICommand CancelCommand { get; set; }
        public ICommand RemoveWebsiteCommand { get; set; }
        public ICommand AddWebsiteCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        public EntryDetailVM(string? id)
        {
            _isPasswordEnabled = true;
            _entry = new EntryDto();

            using WPContext context = new();
            var existedEntry = context.Entries
                .Include(e => e.Websites)
                .FirstOrDefault(e => e.Id.Equals(id));
            if (existedEntry != null)
            {
                _entry = EntryDto.MapFrom(existedEntry);
                _isPasswordEnabled = false;
            }

            ClearPasswordCommand = new BaseCommand<PasswordBox>(c => true, ClearPassword);

            CancelCommand = new BaseCommand<Window>(c => true, Cancel);
            RemoveWebsiteCommand = new BaseCommand<Website>(c => true, RemoveWebsite);
            AddWebsiteCommand = new BaseCommand<ScrollViewer>(c => CanAddWebsite(), AddWebsite);
            SaveCommand = new BaseCommand<Window>(c => CanSave(), Save);
        }

        private void ClearPassword(PasswordBox pb)
        {
            pb?.Clear();
            IsPasswordEnabled = true;
        }

        private static void Cancel(Window w) => w?.Close();

        private bool CanSave()
        {
            return IsValidEntry();
        }

        private bool CanAddWebsite()
        {
            if (Entry.Websites.Count == 0) return true;

            return !string.IsNullOrEmpty(Entry.Websites[^1].Url);
        }

        private void RemoveWebsite(Website web)
        {
            Entry.Websites.Remove(web);
        }

        private void AddWebsite(ScrollViewer? viewer)
        {
            Entry.Websites.Add(new Website
            {
                EntryId = Entry.Id,
            });
            viewer?.ScrollToEnd();
        }

        private async Task Save(Window w)
        {
            using WPContext context = new();
            var trans = await context.Database.BeginTransactionAsync();

            try
            {
                PasswordBox? passwordBox = w.FindName("EntryPasswordBox") as PasswordBox;
                Entry.EncryptedPassword = Security.Encrypt(passwordBox?.Password ?? string.Empty);

                if (IsValidEntry())
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
                        foreach(var item in exists)
                        {
                            if(Security.AreTheSame(item.EncryptedPassword, newEntry.EncryptedPassword))
                            {
                                existed = true;
                                break;
                            }
                        }

                        if (!existed)
                        {
                            await context.Entries.AddAsync(newEntry);

                            foreach (var website in Entry.Websites)
                            {
                                var newWebsite = new Website
                                {
                                    EntryId = newEntry.Id,
                                    Url = website.Url
                                };
                                await context.Websites.AddAsync(newWebsite);
                                await context.SaveChangesAsync();
                                await trans.CommitAsync();
                            }
                        }
                        else
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
                        if(entry.Websites.Count > 1)
                        {

                        }

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
                        await context.SaveChangesAsync();
                        await trans.CommitAsync();
                    }

                    // end operation
                    w?.Close();
                }
                else
                {
                    MessageBox.Show("Something's wrong. Try again.", "Error");
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex.Message);
                trans.Rollback();
            }
        }

        private bool IsValidEntry()
        {
            if (string.IsNullOrEmpty(Entry.Username))
            {
                return false;
            }
            foreach (var website in Entry.Websites)
            {
                if (string.IsNullOrEmpty(website.Url))
                {
                    return false;
                }
            }
            return true;
        }
    }
}

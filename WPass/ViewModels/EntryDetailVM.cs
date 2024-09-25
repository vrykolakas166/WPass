using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ViewModels.Base;
using WPass.Core;
using WPass.Core.Model;
using WPass.DTO;
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

            WPContext context = new();
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
            WPContext context = new();
            PasswordBox? passwordBox = w.FindName("EntryPasswordBox") as PasswordBox;
            Entry.Password = passwordBox?.Password ?? string.Empty;

            if (!IsValidEntry())
            {
                MessageBox.Show("Something's wrong. Try again.", "Error");
                return;
            }

            var entry = await context.Entries
                .Include(e => e.Websites)
                .FirstOrDefaultAsync(e => e.Id.Equals(Entry.Id));

            if (entry == null)
            {
                // create
                var newEntry = new Entry()
                {
                    Username = Entry.Username,
                    EncryptedPassword = Security.Encrypt(Entry.Password),
                };
                await context.Entries.AddAsync(newEntry);

                foreach (var website in Entry.Websites)
                {
                    var newWebsite = new Website
                    {
                        EntryId = newEntry.Id,
                        Url = website.Url
                    };
                    await context.Websites.AddAsync(newWebsite);
                }
            }
            else
            {
                // update
                entry.Username = Entry.Username;
                entry.EncryptedPassword = Security.Encrypt(Entry.Password);

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

            var rs = await context.SaveChangesAsync();
            if (rs < 1)
            {
                MessageBox.Show("Failed.", "Error");
                return;
            }

            w?.Close();
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

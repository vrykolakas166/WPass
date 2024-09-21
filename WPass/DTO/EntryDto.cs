#nullable disable

using System.Collections.ObjectModel;
using System.ComponentModel;
using WPass.Core.Model;
using WPass.Utility;

namespace WPass.DTO
{
    public class EntryDto : INotifyPropertyChanged
    {
        public EntryDto()
        {
            Websites = [];
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsDefault { get; set; }
        public ObservableCollection<Website> Websites { get; set; }

        public void UpdateFrom(Entry entry)
        {
            Id = entry.Id;
            Username = entry.Username;
            Password = Security.Decrypt(entry.EncryptedPassword);
            Websites = [.. entry.Websites];
            IsDefault = entry.IsDefault;
        }

        public static EntryDto MapFrom(Entry entry) => new()
        {
            Id = entry.Id,
            Username = entry.Username,
            Password = Security.Decrypt(entry.EncryptedPassword),
            Websites = [.. entry.Websites],
            IsDefault = entry.IsDefault
        };

        // Detect changed properties section
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

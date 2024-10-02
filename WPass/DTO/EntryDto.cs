#nullable disable

using System.Collections.ObjectModel;
using System.ComponentModel;
using WPass.Core.Model;

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
        public string EncryptedPassword { get; set; }
        public bool IsDefault { get; set; }
        public ObservableCollection<WebsiteDto> Websites { get; set; }

        public void UpdateFrom(Entry entry)
        {
            Id = entry.Id;
            Username = entry.Username;
            EncryptedPassword = entry.EncryptedPassword;
            Websites = [.. entry.Websites.Select(WebsiteDto.MapFrom)];
            IsDefault = entry.IsDefault;
        }

        public static EntryDto MapFrom(Entry entry) => new()
        {
            Id = entry.Id,
            Username = entry.Username,
            EncryptedPassword = entry.EncryptedPassword,
            Websites = [.. entry.Websites.Select(WebsiteDto.MapFrom)],
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

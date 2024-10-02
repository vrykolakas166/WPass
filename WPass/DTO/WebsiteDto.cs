using System.ComponentModel;
using WPass.Core.Model;

#nullable disable

namespace WPass.DTO
{
    public class WebsiteDto
    {
        public string Id { get; set; }
        public string Url { get; set; }

        public string EntryId { get; set; }
        public Entry Entry { get; set; }

        public void UpdateFrom(Website website)
        {
            Id = website.Id;
            Url = website.Url;
            EntryId = website.EntryId;
            Entry = website.Entry;
        }

        public static WebsiteDto MapFrom(Website website)
        {
            return new WebsiteDto
            {
                Id = website.Id,
                Url = website.Url,
                EntryId = website.EntryId,
                Entry = website.Entry
            };
        }

        // Detect changed properties section
        private bool _isChecked = true;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
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

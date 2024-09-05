using System.Windows;
using WPass.ViewModels;

namespace WPass
{
    /// <summary>
    /// Interaction logic for EntryDetailWindow.xaml
    /// </summary>
    public partial class EntryDetailWindow : Window
    {
        public EntryDetailWindow(string? entryId = null)
        {
            InitializeComponent();
            DataContext = new EntryDetailVM(entryId);
            if (!string.IsNullOrEmpty(entryId))
            {
                EntryPasswordBox.Password = Guid.NewGuid().ToString()[..12];
            }
        }
    }
}

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

        private void EDWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TextBoxUsername.Focus();
        }

        private void ButtonInfo1_Click(object sender, RoutedEventArgs e)
        {
            var msg = "NOTE: If you want to update only on some specific websites. " +
                      "Please change username or password. " +
                      "Options section will show up and you need to uncheck option \"Apply to all websites\".";
            MessageBox.Show(msg, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

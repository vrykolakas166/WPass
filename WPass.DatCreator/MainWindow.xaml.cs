using System.Windows;
using Microsoft.Win32;
using WPass.Core;

namespace WPass.DatCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonAutoGen_Click(object sender, RoutedEventArgs e)
        {
            EncryptionText.Text = DpapiHelper.GenerateStrongPassword(32);
        }

        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(EncryptionText.Text);
        }

        private void ButtonCreateDatFile_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(EncryptionText.Text))
            {
                MessageBox.Show("Please generate or create a password first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (EncryptionText.Text.Length != 16 && EncryptionText.Text.Length != 24 && EncryptionText.Text.Length != 32)
            {
                MessageBox.Show("Encryption length must be 16, 24 or 36 characters.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Dat Files (*.dat)|*.dat|All Files (*.*)|*.*",
                Title = "Save File",
                DefaultExt = "dat"
            };

            if (saveFileDialog.ShowDialog() ?? false)
            {
                string filePath = saveFileDialog.FileName;
                DpapiHelper.SavePassword(EncryptionText.Text, filePath);
                MessageBox.Show("File saved at: " + filePath);
            }
        }

        private void ButtonReadDatFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Dat Files (*.dat)|*.dat|All Files (*.*)|*.*",
                Title = "Open File"
            };

            if(openFileDialog.ShowDialog() ?? false)
            {
                string filePath = openFileDialog.FileName;
                try
                {
                    string password = DpapiHelper.LoadPassword(filePath);
                    DecryptText.Text = password;
                    MessageBox.Show("File read successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
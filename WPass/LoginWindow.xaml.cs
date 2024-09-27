using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WPass.Constant;
using WPass.Core;
using WPass.Utility;
using WPass.Utility.SecurityHandler;

namespace WPass
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public enum Mode
        {
            Normal,
            Create,
            Change
        }

        private readonly Dictionary<int, string> Errors = new()
        {
            {101, "Passcode is empty." },
            {102, "Current passcode is empty." },
            {103, "Re-Passcode is empty." },
            {104, "Re-Passcode is not matched." },
            {105, "Wrong code. Please try again."}
        };

        private readonly Dictionary<int, string> Successes = new()
        {
            {201, "Success !" },
            {202, "Passcode is set !" },
            {203, "Passcode is updated !" },
        };

        private object _lastContent = string.Empty;
        private readonly Mode _mode = Mode.Normal;
        private string? _passcode;
        private bool _hasCreated;

        public LoginWindow(Mode mode = Mode.Normal)
        {
            InitializeComponent();
            _mode = mode;
            Setup();

            _hasCreated = false;
        }

        private void Setup()
        {
            ContainerOldCode.Visibility = Visibility.Collapsed;
            ContainerCode.Visibility = Visibility.Collapsed;
            ContainerReCode.Visibility = Visibility.Collapsed;
            ButtonForget.Visibility = Visibility.Collapsed;

            switch (_mode)
            {
                case Mode.Normal:
                    // 1 textbox: passcode
                    // 1 button: access
                    // 1 label: enter code to access
                    ContainerCode.Visibility = Visibility.Visible;
                    ButtonForget.Visibility = Visibility.Visible;
                    ButtonForget.Click += ButtonForget_Click;
                    ButtonOne.Content = "Access";
                    ButtonOne.Click += Access;
                    LabelInfo.Content = "Enter code to access";
                    break;
                case Mode.Create:
                    // 2 textbox: passcode, re-passcode
                    // 1 button: set
                    // 1 label: set your master password
                    ContainerCode.Visibility = Visibility.Visible;
                    ContainerReCode.Visibility = Visibility.Visible;
                    ButtonOne.Content = "Set new";
                    ButtonOne.Click += SetNew;
                    LabelInfo.Content = "First time using app, you need to set a master passcode.";
                    break;
                case Mode.Change:
                    // 3 textbox: current passcode, new passcode, re-new passcode
                    // 1 button: change
                    // 1 label: 
                    ContainerOldCode.Visibility = Visibility.Visible;
                    ContainerCode.Visibility = Visibility.Visible;
                    ContainerReCode.Visibility = Visibility.Visible;
                    ButtonOne.Content = "Change";
                    ButtonOne.Click += UpdatePasscode;
                    LabelInfo.Content = "Please enter all fields to update passcode.";
                    break;
                default:
                    break;
            }
        }

        private async void ButtonForget_Click(object sender, RoutedEventArgs e)
        {
            await PasscodeManager.ResetAsync();
        }

        private async void Access(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                // check passcode with saved data
                if (PasswordBoxCode.Password.Equals(Security.Decrypt(_passcode ?? "")))
                {
                    LabelInfo.Foreground = Brushes.Green;
                    LabelInfo.Content = Successes[201];
                    await Task.Delay(500);
                    new MainWindow().Show();
                    Close();
                }
                else
                {
                    LabelInfo.Foreground = Brushes.Red;
                    LabelInfo.Content = Errors[105];
                    await Task.Delay(500);
                    PasswordBoxCode.Clear();
                }
            }
        }

        private async void SetNew(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                // check passcode with repasscode
                // check passcode with saved data
                if (PasswordBoxCode.Password.Equals(PasswordBoxReCode.Password))
                {
                    await CreateOrUpdatePasscodeAsync(PasswordBoxCode.Password);
                    LabelInfo.Foreground = Brushes.Green;
                    LabelInfo.Content = Successes[202];
                    await Task.Delay(500);
                    new MainWindow().Show();
                    Close();
                }
                else
                {
                    LabelInfo.Foreground = Brushes.Red;
                    LabelInfo.Content = Errors[104];
                    await Task.Delay(500);
                    PasswordBoxCode.Clear();
                    PasswordBoxReCode.Clear();
                }
            }
        }

        private async void UpdatePasscode(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                // check current passcode with saved data
                // check new passcode with repasscode
                if (PasswordBoxOldCode.Password.Equals(Security.Decrypt(_passcode ?? "")))
                {
                    if (PasswordBoxCode.Password.Equals(PasswordBoxReCode.Password))
                    {
                        await CreateOrUpdatePasscodeAsync(PasswordBoxCode.Password);
                        LabelInfo.Foreground = Brushes.Green;
                        LabelInfo.Content = Successes[203];
                        await Task.Delay(500);
                        Close();
                    }
                    else
                    {
                        LabelInfo.Foreground = Brushes.Red;
                        LabelInfo.Content = Errors[104];
                        await Task.Delay(500);
                        PasswordBoxCode.Clear();
                        PasswordBoxReCode.Clear();
                    }
                }
                else
                {
                    LabelInfo.Foreground = Brushes.Red;
                    LabelInfo.Content = Errors[105];
                    await Task.Delay(500);
                    PasswordBoxOldCode.Clear();
                }
            }
        }

        private bool ValidateInput()
        {
            if (!Errors.Values.Contains(LabelInfo.Content))
            {
                _lastContent = LabelInfo.Content;
            }

            LabelInfo.Foreground = Brushes.Red;
            // check empty
            if (string.IsNullOrEmpty(PasswordBoxCode.Password) && ContainerCode.Visibility == Visibility.Visible)
            {
                LabelInfo.Content = Errors[101];
                return false;
            }
            if (string.IsNullOrEmpty(PasswordBoxOldCode.Password) && ContainerOldCode.Visibility == Visibility.Visible)
            {
                LabelInfo.Content = Errors[102];
                return false;
            }
            if (string.IsNullOrEmpty(PasswordBoxReCode.Password) && ContainerReCode.Visibility == Visibility.Visible)
            {
                LabelInfo.Content = Errors[103];
                return false;
            }

            // check valid logic
            if ((PasswordBoxCode.Password != PasswordBoxReCode.Password) && ContainerCode.Visibility == Visibility.Visible && ContainerReCode.Visibility == Visibility.Visible)
            {
                LabelInfo.Content = Errors[104];
                return false;
            }

            LabelInfo.Foreground = Brushes.Black;
            LabelInfo.Content = _lastContent;
            return true;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _passcode = await FetchPersonalCodeAsync();
            if (_mode == Mode.Normal || _mode == Mode.Create)
            {
                PasswordBoxCode.Focus();
            }
            else if (_mode == Mode.Normal)
            {
                PasswordBoxOldCode.Focus();
            }
        }

        private static async Task<string> FetchPersonalCodeAsync()
        {
            await Task.Delay(1000);
            using var context = new WPContext();
            var setting = await context.Settings.FirstOrDefaultAsync(s => s.Key.Equals(Setting.PASSCODE));
            if (setting != null)
            {
                return setting.Value;
            }

            // else return default 123456
            return "123456";
        }

        private async Task CreateOrUpdatePasscodeAsync(string pass)
        {
            using var context = new WPContext();
            var setting = await context.Settings.FirstOrDefaultAsync(s => s.Key.Equals(Setting.PASSCODE));

            if (setting != null) // update
            {
                setting.Value = Security.Encrypt(pass);
                context.Settings.Update(setting);
            }
            else // create
            {
                setting = new Core.Model.Setting()
                {
                    Key = Setting.PASSCODE,
                    Value = Security.Encrypt(pass)
                };
                await context.Settings.AddAsync(setting);
                _hasCreated = true;
            }

            await context.SaveChangesAsync();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_hasCreated && _mode.Equals(Mode.Create))
            {
                // user close without creating new passcode
                MessageBox.Show("Passcode is not created. Please create one before using.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown();
            }

            base.OnClosing(e);
        }

        private void FocusZone_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // AccessCode.Focus();
        }

        private void PasswordBoxCode_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBoxCodePlaceHolder.Visibility = Visibility.Collapsed;
        }

        private void PasswordBoxCode_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordBoxCode.Password))
            {
                PasswordBoxCodePlaceHolder.Visibility = Visibility.Visible;
            }
        }

        private void PasswordBoxReCode_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBoxReCodePlaceHolder.Visibility = Visibility.Collapsed;
        }

        private void PasswordBoxReCode_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordBoxReCode.Password))
            {
                PasswordBoxReCodePlaceHolder.Visibility = Visibility.Visible;
            }
        }

        private void PasswordBoxOldCode_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBoxOldCodePlaceHolder.Visibility = Visibility.Collapsed;
        }

        private void PasswordBoxOldCode_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordBoxOldCode.Password))
            {
                PasswordBoxOldCodePlaceHolder.Visibility = Visibility.Visible;
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ButtonOne.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            }
        }

        private void ContainerOldCode_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PasswordBoxOldCode.Focus();
        }

        private void ContainerCode_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PasswordBoxCode.Focus();
        }

        private void ContainerReCode_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PasswordBoxReCode.Focus();
        }
    }
}

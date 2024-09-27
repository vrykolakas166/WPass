using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using WPass.Utility;
using WPass.Utility.HotkeyHandler;
using WPass.ViewModels;

namespace WPass
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        private SettingVM? vm;
        private readonly KeyCollector _keyCollector;
        private readonly string? _oldFill;
        private readonly string? _oldClear;

        public SettingWindow()
        {
            InitializeComponent();
            vm = Resources["vm"] as SettingVM;
            _keyCollector = new();
            _oldFill = ButtonChangeHotkey_FillData.Content.ToString();
            _oldClear = ButtonChangeHotkey_ClearData.Content.ToString();
        }

        private void ButtonChangeHotkey_FillData_Click(object sender, RoutedEventArgs e)
        {
            KeyListenner.UnregisterAll(Owner);
            _keyCollector.Reset();
            ButtonChangeHotkey_FillData.Content = "Listening...";
            ButtonChangeHotkey_FillData.PreviewKeyDown += ButtonChangeHotkey_FillData_PreviewKeyDown;
            ButtonChangeHotkey_FillData.PreviewKeyUp += ButtonChangeHotkey_FillData_PreviewKeyUp;
        }

        private void ButtonChangeHotkey_FillData_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            _keyCollector.Release(e.Key);
            if (vm != null)
            {
                if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || (e.KeyboardDevice.Modifiers == ModifierKeys.Control | e.KeyboardDevice.Modifiers == ModifierKeys.Alt && e.SystemKey != Key.LeftAlt))
                {
                    MessageBox.Show("Hotkey cannot end with Ctrl or Alt.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    vm.HotkeyFill = _oldFill ?? "Ctrl + `";
                    ButtonChangeHotkey_FillData.Content = vm.HotkeyFill;
                }
            }
        }

        private void ButtonChangeHotkey_FillData_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (vm != null)
            {
                var isCaptured = _keyCollector.Capture(e.Key);
                vm.HotkeyFill = _keyCollector.GetCombination(e);
                ButtonChangeHotkey_FillData.Content = vm.HotkeyFill;
                e.Handled = true;

                if (isCaptured) // valid
                {
                    // register new
                    // just save to current session
                    // save to database when user click save changes
                    // apply only in new instance of app

                    // Success
                    if (vm.HotkeyFill == vm.HotkeyClear)
                    {
                        MessageBox.Show("Hotkey is registerd, please change.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        vm.HotkeyFill = _oldFill ?? "Ctrl + `";
                        ButtonChangeHotkey_FillData.Content = vm.HotkeyFill;
                    }

                    ButtonChangeHotkey_FillData.PreviewKeyDown -= ButtonChangeHotkey_FillData_PreviewKeyDown;
                    ButtonChangeHotkey_FillData.PreviewKeyUp -= ButtonChangeHotkey_FillData_PreviewKeyUp;
                    // back to default
                    KeyListenner.Register(Owner, ModifierKeys.Control, Key.Oem3, () => CredentialManager.SetData());
                    KeyListenner.Register(Owner, ModifierKeys.Control, Key.Q, () => CredentialManager.SetData(true)); // clear

                }
                else if (_keyCollector.Modifiers.ContainsValue(true))
                {
                    // starting with modifiers
                    // waiting
                }
                else // failed
                {
                    MessageBox.Show("Hotkey must start with Ctrl", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    vm.HotkeyFill = _oldFill ?? "Ctrl + `";
                    ButtonChangeHotkey_FillData.Content = vm.HotkeyFill;

                    ButtonChangeHotkey_FillData.PreviewKeyDown -= ButtonChangeHotkey_FillData_PreviewKeyDown;
                    ButtonChangeHotkey_FillData.PreviewKeyUp -= ButtonChangeHotkey_FillData_PreviewKeyUp;
                    // back to default
                    KeyListenner.Register(Owner, ModifierKeys.Control, Key.Oem3, () => CredentialManager.SetData());
                    KeyListenner.Register(Owner, ModifierKeys.Control, Key.Q, () => CredentialManager.SetData(true)); // clear
                }
            }
        }

        private void ButtonChangeHotkey_ClearData_Click(object sender, RoutedEventArgs e)
        {
            KeyListenner.UnregisterAll(Owner);
            _keyCollector.Reset();
            ButtonChangeHotkey_ClearData.Content = "Listening...";
            ButtonChangeHotkey_ClearData.PreviewKeyDown += ButtonChangeHotkey_ClearData_PreviewKeyDown;
            ButtonChangeHotkey_ClearData.PreviewKeyUp += ButtonChangeHotkey_ClearData_PreviewKeyUp;
        }

        private void ButtonChangeHotkey_ClearData_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            _keyCollector.Release(e.Key);
            if (vm != null)
            {
                if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || (e.KeyboardDevice.Modifiers == ModifierKeys.Control | e.KeyboardDevice.Modifiers == ModifierKeys.Alt && e.SystemKey != Key.LeftAlt))
                {
                    MessageBox.Show("Hotkey cannot end with Ctrl or Alt.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    vm.HotkeyClear = _oldClear ?? "Ctrl + `";
                    ButtonChangeHotkey_ClearData.Content = vm.HotkeyClear;
                }
            }
        }

        private void ButtonChangeHotkey_ClearData_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (vm != null)
            {
                var isCaptured = _keyCollector.Capture(e.Key);
                vm.HotkeyClear = _keyCollector.GetCombination(e);
                ButtonChangeHotkey_ClearData.Content = vm.HotkeyClear;
                e.Handled = true;

                if (isCaptured)
                {
                    // register new
                    // just save to current session
                    // save to database when user click save changes
                    // apply only in new instance of app

                    // Success
                    if (vm.HotkeyFill == vm.HotkeyClear)
                    {
                        MessageBox.Show("Hotkey is registerd, please change.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        vm.HotkeyClear = _oldClear ?? "Ctrl + `";
                        ButtonChangeHotkey_ClearData.Content = vm.HotkeyClear;
                    }

                    ButtonChangeHotkey_ClearData.PreviewKeyDown -= ButtonChangeHotkey_ClearData_PreviewKeyDown;
                    ButtonChangeHotkey_ClearData.PreviewKeyUp -= ButtonChangeHotkey_ClearData_PreviewKeyUp;
                    // back to default
                    KeyListenner.Register(Owner, ModifierKeys.Control, Key.Oem3, () => CredentialManager.SetData());
                    KeyListenner.Register(Owner, ModifierKeys.Control, Key.Q, () => CredentialManager.SetData(true)); // clear
                }
                else if (_keyCollector.Modifiers.ContainsValue(true))
                {
                    // starting with modifiers
                }
                else
                {
                    MessageBox.Show("Hotkey must start with Ctrl", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    vm.HotkeyClear = _oldClear ?? "Ctrl + Q";
                    ButtonChangeHotkey_ClearData.Content = vm.HotkeyClear;

                    ButtonChangeHotkey_ClearData.PreviewKeyDown -= ButtonChangeHotkey_ClearData_PreviewKeyDown;
                    ButtonChangeHotkey_ClearData.PreviewKeyUp -= ButtonChangeHotkey_ClearData_PreviewKeyUp;
                    // back to default
                    KeyListenner.Register(Owner, ModifierKeys.Control, Key.Oem3, () => CredentialManager.SetData());
                    KeyListenner.Register(Owner, ModifierKeys.Control, Key.Q, () => CredentialManager.SetData(true)); // clear
                }
            }


        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (vm != null && vm.CanSave() &&
                MessageBox.Show("Discard all your changes?", "Before closing",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }
            else
            {
                base.OnClosing(e);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            vm = null;
            ButtonChangeHotkey_FillData.PreviewKeyDown -= ButtonChangeHotkey_FillData_PreviewKeyDown;
            ButtonChangeHotkey_FillData.PreviewKeyUp -= ButtonChangeHotkey_FillData_PreviewKeyUp;
            ButtonChangeHotkey_ClearData.PreviewKeyDown -= ButtonChangeHotkey_ClearData_PreviewKeyDown;
            ButtonChangeHotkey_ClearData.PreviewKeyUp -= ButtonChangeHotkey_ClearData_PreviewKeyUp;
            _keyCollector.Dispose();
            base.OnClosed(e);
        }
    }
}

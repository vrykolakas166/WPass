using System.Windows;
using System.Windows.Input;
using WPass.Utility;
using WPass.ViewModels;

namespace WPass
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        private readonly SettingVM? vm;

        public SettingWindow()
        {
            InitializeComponent();
            vm = Resources["vm"] as SettingVM;
        }

        private void ButtonChangeHotkey_FillData_Click(object sender, RoutedEventArgs e)
        {
            KeyListenner.Unregister(Owner, MainVM.HOTKEY_FILL_DATA);
            KeyCollector.Reset();
            ButtonChangeHotkey_FillData.Content = "Listening...";
            ButtonChangeHotkey_FillData.PreviewKeyDown += ButtonChangeHotkey_FillData_PreviewKeyDown;
            ButtonChangeHotkey_FillData.PreviewKeyUp += ButtonChangeHotkey_FillData_PreviewKeyUp;
        }

        private void ButtonChangeHotkey_FillData_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            KeyCollector.Release(e.Key);
        }

        private void ButtonChangeHotkey_FillData_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var isFinised = KeyCollector.Capture(e.Key);
            if (vm != null)
            {
                vm.HotkeyFill = KeyCollector.GetCombination(e);
                ButtonChangeHotkey_FillData.Content = vm.HotkeyFill;
            }
            e.Handled = true;

            if (isFinised)
            {
                ButtonChangeHotkey_FillData.PreviewKeyDown -= ButtonChangeHotkey_FillData_PreviewKeyDown;
                ButtonChangeHotkey_FillData.PreviewKeyUp -= ButtonChangeHotkey_FillData_PreviewKeyUp;

                // check valid
                if (KeyCollector.IsCtrlPressed) // valid
                {
                    // register new
                    // restart to apply
                }
                else
                {
                    if (vm != null)
                    {
                        vm.HotkeyClear = "Ctrl + `";
                    }
                }

                // back to default
                MainVM.HOTKEY_FILL_DATA = KeyListenner.Register(Owner, ModifierKeys.Control, Key.Oem3, () => CredentialManager.FillData());
            }
        }

        private void ButtonChangeHotkey_ClearData_Click(object sender, RoutedEventArgs e)
        {
            KeyListenner.Unregister(Owner, MainVM.HOTKEY_CLEAR_DATA);
            KeyCollector.Reset();
            ButtonChangeHotkey_ClearData.Content = "Listening...";
            ButtonChangeHotkey_ClearData.PreviewKeyDown += ButtonChangeHotkey_ClearData_PreviewKeyDown;
            ButtonChangeHotkey_ClearData.PreviewKeyUp += ButtonChangeHotkey_ClearData_PreviewKeyUp;
        }

        private void ButtonChangeHotkey_ClearData_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            KeyCollector.Release(e.Key);
        }

        private void ButtonChangeHotkey_ClearData_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var isFinised = KeyCollector.Capture(e.Key);
            if (vm != null)
            {
                vm.HotkeyClear = KeyCollector.GetCombination(e);
                ButtonChangeHotkey_ClearData.Content = vm.HotkeyClear;
            }
            e.Handled = true;

            if (isFinised)
            {
                ButtonChangeHotkey_ClearData.PreviewKeyDown -= ButtonChangeHotkey_ClearData_PreviewKeyDown;
                ButtonChangeHotkey_ClearData.PreviewKeyUp -= ButtonChangeHotkey_ClearData_PreviewKeyUp;

                // check valid
                if (KeyCollector.IsCtrlPressed) // valid
                {
                    // register new
                    // restart to apply
                }
                else
                {
                    if (vm != null)
                    {
                        vm.HotkeyClear = "Ctrl + Q";
                    }
                }
                // back to default
                MainVM.HOTKEY_CLEAR_DATA = KeyListenner.Register(Owner, ModifierKeys.Control, Key.Q, () => CredentialManager.FillData(true)); // clear
            }
        }
    }
}

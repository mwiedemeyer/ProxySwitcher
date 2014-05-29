using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProxySwitcher.Core;
using ProxySwitcher.Core.Resources;
using ProxySwitcher.Common;

namespace ProxySwitcher
{
    /// <summary>
    /// Interaction logic for SettingsConfigurationControl.xaml
    /// </summary>
    public partial class SettingsConfigurationControl : UserControl
    {
        private MainWindowRibbon parentWindow;

        public SettingsConfigurationControl()
        {
            InitializeComponent();
        }

        public SettingsConfigurationControl(MainWindowRibbon mainWindowRibbon)
            : this()
        {
            this.parentWindow = mainWindowRibbon;
            InitUI();
        }

        private void InitUI()
        {
            // Start Minimized
            bool startMinimized = SettingsManager.Instance.GetApplicationSetting(SettingsManager.App_StartMinimized, SettingsManager.Default_StartMinimized);
            checkBoxStartMinimized.IsChecked = startMinimized;

            // Stop after first match
            bool stopAfterFirstMatch = SettingsManager.Instance.GetApplicationSetting(SettingsManager.App_StopAfterFirstNetworkMatch, SettingsManager.Default_StopAfterFirstNetworkMatch);
            checkBoxStopAfterFirstMatch.IsChecked = stopAfterFirstMatch;

            // Update check
            bool autoUpdate = SettingsManager.Instance.GetApplicationSetting(SettingsManager.App_CheckForUpdates, SettingsManager.Default_CheckForUpdates);
            checkBoxCheckForUpdates.IsChecked = autoUpdate;

            // Autostart
            bool autoStart = SettingsManager.Instance.GetApplicationSetting(SettingsManager.App_AutoStartWithWindows, SettingsManager.Default_AutoStartWithWindows);
            checkBoxAutoStart.IsChecked = autoStart;

            // NoNewNetwork
            bool noNetwork = SettingsManager.Instance.GetApplicationSetting(SettingsManager.App_NoNewNetwork, SettingsManager.Default_NoNewNetwork);
            checkBoxNoNew.IsChecked = noNetwork;

            if (Windows7Helper.IsWindows7)
            {
                // Windows 7 Location API
                bool locApi = SettingsManager.Instance.GetApplicationSetting(SettingsManager.App_UseWin7LocationAPI, SettingsManager.Default_UseWin7LocationAPI);
                checkBoxWin7LocationAPI.IsChecked = locApi;

                // Windows 7 Mode (minimize, etc)
                bool w7Mode = SettingsManager.Instance.GetApplicationSetting(SettingsManager.App_UseWin7Mode, SettingsManager.Default_UseWin7Mode);
                checkBoxWin7Mode.IsChecked = w7Mode;
            }
            else
            {
                checkBoxWin7LocationAPI.Visibility = System.Windows.Visibility.Hidden;
                checkBoxWin7Mode.Visibility = System.Windows.Visibility.Hidden;
            }

            // Language
            string currentLanguage = SettingsManager.Instance.GetApplicationSetting(SettingsManager.App_Language, SettingsManager.Default_Language.ToString());
            for (int i = 0; i < comboBoxLanguage.Items.Count; i++)
            {
                var lang = comboBoxLanguage.Items[i] as ComboBoxItem;
                if (lang.Tag.ToString() == currentLanguage)
                {
                    comboBoxLanguage.SelectedIndex = i;
                    break;
                }
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            bool applicationRestartRequired = false;
            Dictionary<string, string> newSettings = new Dictionary<string, string>();

            // Start Minimized
            newSettings.Add(SettingsManager.App_StartMinimized, checkBoxStartMinimized.IsChecked.ToString());

            // Stop after first match
            newSettings.Add(SettingsManager.App_StopAfterFirstNetworkMatch, checkBoxStopAfterFirstMatch.IsChecked.ToString());

            // Update check
            newSettings.Add(SettingsManager.App_CheckForUpdates, checkBoxCheckForUpdates.IsChecked.ToString());

            // Autostart
            newSettings.Add(SettingsManager.App_AutoStartWithWindows, checkBoxAutoStart.IsChecked.ToString());
            SetStartWithWindows(checkBoxAutoStart.IsChecked.Value);

            // No new Network
            newSettings.Add(SettingsManager.App_NoNewNetwork, checkBoxNoNew.IsChecked.ToString());

            if (Windows7Helper.IsWindows7)
            {
                // Windows 7 Location API
                if (SettingsManager.Instance.GetApplicationSetting<bool>(SettingsManager.App_UseWin7LocationAPI, SettingsManager.Default_UseWin7LocationAPI) != checkBoxWin7LocationAPI.IsChecked.Value)
                    applicationRestartRequired = true;
                newSettings.Add(SettingsManager.App_UseWin7LocationAPI, checkBoxWin7LocationAPI.IsChecked.ToString());

                // Windows 7 Mode (minimize, etc)
                if (SettingsManager.Instance.GetApplicationSetting<bool>(SettingsManager.App_UseWin7Mode, SettingsManager.Default_UseWin7Mode) != checkBoxWin7Mode.IsChecked.Value)
                    applicationRestartRequired = true;
                newSettings.Add(SettingsManager.App_UseWin7Mode, checkBoxWin7Mode.IsChecked.ToString());
            }

            // Language
            var item = comboBoxLanguage.SelectedItem as ComboBoxItem;
            if (item != null)
            {
                int newLang = int.Parse(item.Tag.ToString());
                if (SettingsManager.Instance.GetApplicationSetting<int>(SettingsManager.App_Language, SettingsManager.Default_Language) != newLang)
                {
                    newSettings.Add(SettingsManager.App_Language, newLang.ToString());
                    applicationRestartRequired = true;
                }
            }

            SettingsManager.Instance.SaveApplicationSettings(newSettings);

            if (applicationRestartRequired)
            {
                MessageBox.Show(LanguageResources.RestartApplication, parentWindow.Title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SetStartWithWindows(bool startWithWindows)
        {
            if (startWithWindows)
            {
                RegistryHelper.SetStringValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run", "Proxy Switcher", System.Reflection.Assembly.GetEntryAssembly().Location);
            }
            else
            {
                RegistryHelper.DeleteEntry("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run", "Proxy Switcher");
            }
        }
    }
}

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
using ProxySwitcher.UI;
using ProxySwitcher.Core.Resources;
using Microsoft.Win32;

namespace ProxySwitcher
{
    /// <summary>
    /// Interaction logic for NetworkConfigurationControl.xaml
    /// </summary>
    public partial class NetworkConfigurationControl : UserControl
    {
        private NetworkConfiguration networkConfiguration;

        public NetworkConfigurationControl()
        {
            InitializeComponent();
        }

        public NetworkConfigurationControl(NetworkConfiguration networkConfiguration)
            : this()
        {
            this.networkConfiguration = networkConfiguration;

            InitUI();
        }

        private void InitUI()
        {
            PopulateMethods();

            PopulateNetworkInterfaces();

            if (this.networkConfiguration == null)
                return;

            textBoxName.Text = this.networkConfiguration.Name;
            textBoxQuery.Text = this.networkConfiguration.Query;
            comboBoxMethod.SelectedIndex = GetSelectedIndex(this.networkConfiguration.Method);
            checkBoxActive.IsChecked = this.networkConfiguration.Active;
            comboBoxSpecificNetworkInterface.SelectedIndex = GetSelectedIndexNic(this.networkConfiguration.NetworkInterfaceId);

            if (!String.IsNullOrEmpty(this.networkConfiguration.IconPath) && System.IO.File.Exists(this.networkConfiguration.IconPath))
            {
                ShowIconOnButton(this.networkConfiguration.IconPath);
            }
        }

        private void ShowIconOnButton(string filename)
        {
            try
            {
                buttonNetworkIcon.Content = new Image()
                {
                    Source = new BitmapImage(new Uri(filename))
                };

                buttonNetworkIcon.Tag = filename;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, ex);
                buttonNetworkIcon.Content = "...";
            }
        }

        private int GetSelectedIndexNic(string networkInterfaceId)
        {
            for (int i = 0; i < comboBoxSpecificNetworkInterface.Items.Count; i++)
            {
                var m = comboBoxSpecificNetworkInterface.Items[i] as ComboBoxItem;
                if (m.Tag == null)
                    continue;
                if (m.Tag.ToString() == networkInterfaceId)
                    return i;
            }

            return 0;
        }

        private void PopulateNetworkInterfaces()
        {
            comboBoxSpecificNetworkInterface.Items.Clear();

            comboBoxSpecificNetworkInterface.Items.Add(new ComboBoxItem() { Content = LanguageResources.AllNetworkInterfaces_ComboBox, Tag = string.Empty });

            foreach (var item in NetworkManager.GetAllNetworkInterfaces())
            {
                comboBoxSpecificNetworkInterface.Items.Add(new ComboBoxItem() { Content = item.Name, Tag = item.Id });
            }
        }

        private int GetSelectedIndex(NetworkConfigurationMethod networkConfigurationMethod)
        {
            for (int i = 0; i < comboBoxMethod.Items.Count; i++)
            {
                var m = comboBoxMethod.Items[i] as NetworkConfigurationUIItem;
                if (m.Method == networkConfigurationMethod)
                    return i;
            }

            return -1;
        }

        private void PopulateMethods()
        {
            comboBoxMethod.Items.Clear();

            bool isLocationEnabled = SettingsManager.Instance.GetApplicationSetting<bool>(SettingsManager.App_UseWin7LocationAPI, SettingsManager.Default_UseWin7LocationAPI);

            foreach (NetworkConfigurationMethod item in Enum.GetValues(typeof(NetworkConfigurationMethod)))
            {
                if (item == NetworkConfigurationMethod.Unknown)
                    continue;

                // Show Location only for Windows 7
                if (item == NetworkConfigurationMethod.Location && !isLocationEnabled)
                    continue;

                comboBoxMethod.Items.Add(new NetworkConfigurationUIItem(item));
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (this.networkConfiguration == null)
                this.networkConfiguration = new NetworkConfiguration();

            this.networkConfiguration.Name = textBoxName.Text;
            this.networkConfiguration.Query = textBoxQuery.Text;
            if (comboBoxMethod.SelectedItem != null)
                this.networkConfiguration.Method = ((NetworkConfigurationUIItem)comboBoxMethod.SelectedItem).Method;
            this.networkConfiguration.Active = checkBoxActive.IsChecked.Value;
            this.networkConfiguration.NetworkInterfaceId = ((ComboBoxItem)comboBoxSpecificNetworkInterface.SelectedItem).Tag.ToString();
            this.networkConfiguration.NetworkInterfaceName = ((ComboBoxItem)comboBoxSpecificNetworkInterface.SelectedItem).Content.ToString();

            if (buttonNetworkIcon.Tag != null && !String.IsNullOrEmpty(buttonNetworkIcon.Tag.ToString()))
                this.networkConfiguration.IconPath = buttonNetworkIcon.Tag.ToString();
            else
                this.networkConfiguration.IconPath = String.Empty;

            SettingsManager.Instance.SaveNetworkConfiguration(this.networkConfiguration);
        }

        private void comboBoxMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1)
                return;

            string wlanSSID = string.Empty;
            string gateway = string.Empty;
            string gatewayMAC = string.Empty;
            string dnsSuffix = string.Empty;
            string ip = string.Empty;
            string location = string.Empty;

            NetworkManager.GetPrePopulatedNetworkMethods(out wlanSSID, out gateway, out gatewayMAC, out dnsSuffix, out ip, out location);

            string previousQuery = string.Empty;
            if (e.RemovedItems.Count > 0)
            {
                var prev = e.RemovedItems[0] as NetworkConfigurationUIItem;
                if (prev != null)
                {
                    switch (prev.Method)
                    {
                        case NetworkConfigurationMethod.DNSSuffix:
                            previousQuery = dnsSuffix;
                            break;
                        case NetworkConfigurationMethod.Gateway:
                            previousQuery = gateway;
                            break;
                        case NetworkConfigurationMethod.GatewayMAC:
                            previousQuery = gatewayMAC;
                            break;
                        case NetworkConfigurationMethod.WLANSSID:
                            previousQuery = wlanSSID;
                            break;
                        case NetworkConfigurationMethod.IP:
                            previousQuery = ip;
                            break;
                        case NetworkConfigurationMethod.Location:
                            previousQuery = location;
                            break;
                        case NetworkConfigurationMethod.Unknown:
                        default:
                            break;
                    }
                }
            }

            var ncui = e.AddedItems[0] as NetworkConfigurationUIItem;
            if (ncui == null)
                return;

            switch (ncui.Method)
            {
                case NetworkConfigurationMethod.DNSSuffix:
                    if (String.IsNullOrWhiteSpace(textBoxQuery.Text) || textBoxQuery.Text == previousQuery)
                        textBoxQuery.Text = dnsSuffix;
                    break;
                case NetworkConfigurationMethod.Gateway:
                    if (String.IsNullOrWhiteSpace(textBoxQuery.Text) || textBoxQuery.Text == previousQuery)
                        textBoxQuery.Text = gateway;
                    break;
                case NetworkConfigurationMethod.GatewayMAC:
                    if (String.IsNullOrWhiteSpace(textBoxQuery.Text) || textBoxQuery.Text == previousQuery)
                        textBoxQuery.Text = gatewayMAC;
                    break;
                case NetworkConfigurationMethod.WLANSSID:
                    if (String.IsNullOrWhiteSpace(textBoxQuery.Text) || textBoxQuery.Text == previousQuery)
                        textBoxQuery.Text = wlanSSID;
                    break;
                case NetworkConfigurationMethod.IP:
                    if (String.IsNullOrWhiteSpace(textBoxQuery.Text) || textBoxQuery.Text == previousQuery)
                        textBoxQuery.Text = ip;
                    break;
                case NetworkConfigurationMethod.Location:
                    if (String.IsNullOrWhiteSpace(textBoxQuery.Text) || textBoxQuery.Text == previousQuery)
                        textBoxQuery.Text = location;
                    break;
                case NetworkConfigurationMethod.Unknown:
                default:
                    break;
            }
        }

        private void buttonNetworkIcon_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Filter = "Image Files|*.*";
            dialog.Multiselect = false;
            dialog.Title = "Proxy Switcher";
            dialog.ShowDialog();

            if (!String.IsNullOrWhiteSpace(dialog.FileName))
            {
                buttonNetworkIcon.Tag = dialog.FileName;
                ShowIconOnButton(dialog.FileName);
            }
            else
            {
                buttonNetworkIcon.Tag = String.Empty;
                buttonNetworkIcon.Content = "...";
            }
        }
    }
}

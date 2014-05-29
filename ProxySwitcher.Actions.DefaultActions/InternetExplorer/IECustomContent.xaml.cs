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
using ProxySwitcher.Common;

namespace ProxySwitcher.Actions.DefaultActions.InternetExplorer
{
    /// <summary>
    /// Interaction logic for IECustomContent.xaml
    /// </summary>
    public partial class IECustomContent : UserControl
    {
        private InternetExplorer8ProxyAction internetExplorer8ProxyAction;
        private Guid networkId;
        private bool initialize = false;

        public IECustomContent()
        {
            InitializeComponent();
        }

        public IECustomContent(InternetExplorer8ProxyAction internetExplorer8ProxyAction, Guid networkId)
            : this()
        {
            this.internetExplorer8ProxyAction = internetExplorer8ProxyAction;
            this.networkId = networkId;

            this.initialize = true;
            InitUI();
            this.initialize = false;
        }

        private void InitUI()
        {
            PopulateComboBox();

            bool useDialUp = false;
            bool.TryParse(this.internetExplorer8ProxyAction.Settings[networkId.ToString() + "_UseDialUp"], out useDialUp);
            checkBoxUseDialUp.IsChecked = useDialUp;
            comboBoxDialUpConnections.IsEnabled = useDialUp;

            string name = this.internetExplorer8ProxyAction.Settings[networkId.ToString() + "_UseDialUpName"];
            comboBoxDialUpConnections.SelectedItem = name;
        }

        private void PopulateComboBox()
        {
            comboBoxDialUpConnections.Items.Clear();

            foreach (var item in RegistryHelper.GetValuesInKey(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Connections"))
            {
                if (item == "DefaultConnectionSettings" || item == "SavedLegacySettings")
                    continue;

                comboBoxDialUpConnections.Items.Add(item);
            }
        }

        private void checkBoxUseDialUp_Checked(object sender, RoutedEventArgs e)
        {
            comboBoxDialUpConnections.IsEnabled = checkBoxUseDialUp.IsChecked.Value;

            if (this.initialize)
                return;

            this.internetExplorer8ProxyAction.Settings[networkId.ToString() + "_UseDialUp"] = checkBoxUseDialUp.IsChecked.Value.ToString();
            if (!checkBoxUseDialUp.IsChecked.Value)
            {
                this.internetExplorer8ProxyAction.Settings[networkId.ToString() + "_UseDialUpName"] = string.Empty;
                comboBoxDialUpConnections.SelectedItem = null;
            }
            else
            {
                if (comboBoxDialUpConnections.Items.Count > 0)
                    comboBoxDialUpConnections.SelectedIndex = 0;
            }
        }

        private void comboBoxDialUpConnections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.initialize)
                return;

            if (comboBoxDialUpConnections.SelectedItem != null)
                this.internetExplorer8ProxyAction.Settings[networkId.ToString() + "_UseDialUpName"] = comboBoxDialUpConnections.SelectedItem.ToString();
        }
    }
}

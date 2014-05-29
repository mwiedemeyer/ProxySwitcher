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

namespace ProxySwitcher.Actions.DefaultActions.Firefox
{
    /// <summary>
    /// Interaction logic for DisableFirefoxConfig.xaml
    /// </summary>
    public partial class DisableFirefoxConfig : UserControl
    {
        private DisableFirefoxProxyAction disableFirefoxProxyAction;
        private Guid networkId;

        public DisableFirefoxConfig()
        {
            InitializeComponent();
        }

        public DisableFirefoxConfig(DisableFirefoxProxyAction disableFirefoxProxyAction, Guid networkId)
            : this()
        {
            this.disableFirefoxProxyAction = disableFirefoxProxyAction;
            this.networkId = networkId;

            this.textBoxFolder.Text = this.disableFirefoxProxyAction.GetProfileFolder(networkId);

            PopulateComboBox();

            comboBoxProfiles.SelectedItem = this.disableFirefoxProxyAction.GetProfileToSwitch(this.networkId);
            if (comboBoxProfiles.SelectedItem == null)
                comboBoxProfiles.SelectedIndex = 0;
        }

        private void PopulateComboBox()
        {
            comboBoxProfiles.Items.Clear();

            comboBoxProfiles.Items.Add(DefaultResources.Firefox_All);
            foreach (var prof in this.disableFirefoxProxyAction.GetAllProfiles(this.networkId))
            {
                comboBoxProfiles.Items.Add(prof);
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxProfiles.SelectedItem == null)
                return;

            string profile = comboBoxProfiles.SelectedItem.ToString();
            if (profile == DefaultResources.Firefox_All)
                profile = string.Empty;

            this.disableFirefoxProxyAction.SaveData(this.networkId, profile, textBoxFolder.Text);
        }

        private void labelRefreshProfiles_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.disableFirefoxProxyAction.SaveData(this.networkId, string.Empty, textBoxFolder.Text);
            PopulateComboBox();
            labelRefreshProfiles.Visibility = System.Windows.Visibility.Hidden;
        }

        private void textBoxFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBoxFolder.Text != this.disableFirefoxProxyAction.GetProfileFolder(this.networkId))
                this.labelRefreshProfiles.Visibility = System.Windows.Visibility.Visible;
            else
                this.labelRefreshProfiles.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}

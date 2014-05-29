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
    /// Interaction logic for ProfileSelector.xaml
    /// </summary>
    public partial class ProfileSelector : Window
    {
        private FirefoxProxyAction firefoxProxyAction;
        private Guid networkId;

        public ProfileSelector()
        {
            InitializeComponent();
        }

        public ProfileSelector(FirefoxProxyAction firefoxProxyAction, Guid networkId)
            : this()
        {
            this.firefoxProxyAction = firefoxProxyAction;
            this.networkId = networkId;

            this.textBoxFolder.Text = this.firefoxProxyAction.GetProfileFolder(networkId);

            PopulateComboBox();

            comboBoxProfiles.SelectedItem = this.firefoxProxyAction.GetProfileToSwitch(this.networkId);
            if (comboBoxProfiles.SelectedItem == null)
                comboBoxProfiles.SelectedIndex = 0;
        }

        private void PopulateComboBox()
        {
            comboBoxProfiles.Items.Clear();

            comboBoxProfiles.Items.Add(DefaultResources.Firefox_All);
            foreach (var prof in this.firefoxProxyAction.GetAllProfiles(this.networkId))
            {
                comboBoxProfiles.Items.Add(prof);
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxProfiles.SelectedItem == null)
            {
                this.DialogResult = false;
                return;
            }

            string profile = comboBoxProfiles.SelectedItem.ToString();
            if (profile == DefaultResources.Firefox_All)
                profile = string.Empty;

            this.firefoxProxyAction.SaveData(this.networkId, profile, textBoxFolder.Text);
            this.DialogResult = true;
        }

        private void labelRefreshProfiles_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.firefoxProxyAction.SaveData(this.networkId, string.Empty, textBoxFolder.Text);
            PopulateComboBox();
            labelRefreshProfiles.Visibility = System.Windows.Visibility.Hidden;
        }

        private void textBoxFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBoxFolder.Text != this.firefoxProxyAction.GetProfileFolder(this.networkId))
                this.labelRefreshProfiles.Visibility = System.Windows.Visibility.Visible;
            else
                this.labelRefreshProfiles.Visibility = System.Windows.Visibility.Hidden;
        }

    }
}

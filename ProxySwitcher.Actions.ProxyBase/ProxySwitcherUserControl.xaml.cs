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

namespace ProxySwitcher.Actions.ProxyBase
{
    /// <summary>
    /// Interaction logic for ProxySwitcherUserControl.xaml
    /// </summary>
    public partial class ProxySwitcherUserControl : UserControl
    {
        private ProxySwitcherAction proxySwitcherAction;
        private ProxyEntry proxy;
        private Guid networkId;
        private ProxySettingWithScheme<string> urlStore = new ProxySettingWithScheme<string>();
        private ProxySettingWithScheme<int> portStore = new ProxySettingWithScheme<int>();
        private bool fireEvent = true;

        public ProxySwitcherUserControl()
        {
            InitializeComponent();
        }

        public ProxySwitcherUserControl(ProxySwitcherAction proxySwitcherAction, Guid networkId)
            : this()
        {
            this.proxySwitcherAction = proxySwitcherAction;
            this.networkId = networkId;

            this.proxy = this.proxySwitcherAction.GetProxyEntryFromSettings(networkId);

            PopulateSchemeComboBox();

            SetProxyToUI(proxy);
        }

        private void PopulateSchemeComboBox()
        {
            comboBoxScheme.Items.Clear();
            foreach (var item in Enum.GetNames(typeof(ProxyScheme)))
            {
                if (item == ProxyScheme.Unknown.ToString() || item == ProxyScheme.All.ToString())
                    continue;

                comboBoxScheme.Items.Add(item);
            }
        }

        private void SetProxyToUI(ProxyEntry proxyEntry)
        {
            if (proxyEntry == null)
                return;

            this.urlStore = proxyEntry.Url;
            this.portStore = proxyEntry.Port;

            fireEvent = false;

            checkBoxUseSame.IsChecked = proxyEntry.Url.IsAllSet || proxyEntry.Url.Keys.Length < 1;
            SelectScheme(proxyEntry.Url.FirstScheme());
            textBoxUrl.Text = proxyEntry.Url.FirstEntry();
            textBoxPort.Text = proxyEntry.Port.FirstEntry().ToString();
            textBoxExceptions.Text = proxyEntry.Exceptions;
            checkBoxAutoConfig.IsChecked = proxyEntry.IsAutoConf;
            checkBoxAutoDetect.IsChecked = proxyEntry.IsAutoDetect;
            checkBoxBypass.IsChecked = proxyEntry.ByPassLocal;
            checkBoxRequiresAuthentication.IsChecked = proxyEntry.RequiresAuthentication;
            textBoxUsername.Text = proxyEntry.AuthenticationUsername;
            passwordBoxPassword.Password = proxyEntry.AuthenticationPassword;

            fireEvent = true;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (proxy == null)
                proxy = new ProxyEntry();

            CleanUpStores();
            PrepareSave();

            proxy.Url = this.urlStore;
            proxy.Port = this.portStore;
            proxy.IsAutoConf = checkBoxAutoConfig.IsChecked.Value;
            proxy.IsAutoDetect = checkBoxAutoDetect.IsChecked.Value;
            proxy.ByPassLocal = checkBoxBypass.IsChecked.Value;
            proxy.Exceptions = textBoxExceptions.Text;
            proxy.RequiresAuthentication = checkBoxRequiresAuthentication.IsChecked.Value;
            proxy.AuthenticationUsername = textBoxUsername.Text;
            proxy.AuthenticationPassword = passwordBoxPassword.Password;

            try
            {
                this.proxySwitcherAction.ValidateEntry(proxy);
                this.proxySwitcherAction.SetProxyEntryToSettings(this.networkId, proxy);
            }
            catch (ProxyValidationException ex)
            {
                this.proxySwitcherAction.HostApplication.SetStatusText(this.proxySwitcherAction, ex.Message, true);
            }
        }

        private void PrepareSave()
        {
            if (checkBoxUseSame.IsChecked.Value)
            {
                this.urlStore[ProxyScheme.All] = textBoxUrl.Text;
                int port;
                if (int.TryParse(textBoxPort.Text, out port))
                    this.portStore[ProxyScheme.All] = port;
            }
            else
            {
                string scheme = comboBoxScheme.SelectedItem.ToString();
                this.urlStore[scheme] = textBoxUrl.Text;
                int port;
                if (int.TryParse(textBoxPort.Text, out port))
                    this.portStore[scheme] = int.Parse(textBoxPort.Text);
            }
        }

        public void SetCustomContent(object content)
        {
            this.contentControlAdditionalButtons.Content = content;
        }

        public bool ShowAuthentication
        {
            get { return checkBoxRequiresAuthentication.IsVisible; }
            set
            {
                Visibility v = Visibility.Visible;
                if (!value)
                    v = Visibility.Hidden;

                checkBoxRequiresAuthentication.Visibility = v;
                textBoxUsername.Visibility = v;
                passwordBoxPassword.Visibility = v;
                labelUsername.Visibility = v;
                labelPassword.Visibility = v;
            }
        }

        internal void SetDefaultProxy(ProxyEntry proxyEntry)
        {
            if (this.proxy.Port.FirstEntry() == 0 && String.IsNullOrEmpty(this.proxy.Url.FirstEntry()))
                SetProxyToUI(proxyEntry);
        }

        private void comboBoxScheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!fireEvent)
                return;

            if (comboBoxScheme.SelectedItem == null)
                return;

            string newScheme = comboBoxScheme.SelectedItem.ToString();

            if (e.RemovedItems.Count > 0)
            {
                string oldScheme = e.RemovedItems[0].ToString();
                this.urlStore[oldScheme] = textBoxUrl.Text;
                this.portStore[oldScheme] = int.Parse(textBoxPort.Text);
            }

            textBoxUrl.Text = this.urlStore[newScheme];
            textBoxPort.Text = this.portStore[newScheme].ToString();
        }

        private void checkBoxUseSame_Click(object sender, RoutedEventArgs e)
        {
            if (!fireEvent)
                return;

            CleanUpStores();

            if (checkBoxUseSame.IsChecked.Value)
            {
                SelectScheme(ProxyScheme.All);
            }
            else
            {
                SelectScheme(ProxyScheme.HTTP);
            }
        }

        private void CleanUpStores()
        {
            if (checkBoxUseSame.IsChecked.Value)
            {
                this.urlStore.RemoveAllExceptSchemeAll();
                this.portStore.RemoveAllExceptSchemeAll();
            }
            else
            {
                this.urlStore.Remove(ProxyScheme.All);
                this.portStore.Remove(ProxyScheme.All);
            }
        }

        private void SelectScheme(ProxyScheme scheme)
        {
            if (scheme == ProxyScheme.All || scheme == ProxyScheme.Unknown)
            {
                comboBoxScheme.Text = string.Empty;
                comboBoxScheme.IsEnabled = false;
                return;
            }

            comboBoxScheme.IsEnabled = true;

            foreach (string item in comboBoxScheme.Items)
            {
                if (item == scheme.ToString())
                {
                    comboBoxScheme.SelectedItem = item;
                    return;
                }
            }
        }

        private void checkBoxAutoConfig_Checked(object sender, RoutedEventArgs e)
        {
            if (!fireEvent)
                return;

            if (checkBoxAutoConfig.IsChecked.Value)
                checkBoxUseSame.IsChecked = true;
        }
    }
}

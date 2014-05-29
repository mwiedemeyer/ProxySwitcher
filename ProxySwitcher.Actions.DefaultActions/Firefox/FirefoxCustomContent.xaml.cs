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
    /// Interaction logic for FirefoxCustomContent.xaml
    /// </summary>
    public partial class FirefoxCustomContent : UserControl
    {
        private FirefoxProxyAction firefoxProxyAction;
        private Guid networkId;

        public FirefoxCustomContent()
        {
            InitializeComponent();
        }

        public FirefoxCustomContent(FirefoxProxyAction firefoxProxyAction, Guid networkId)
            : this()
        {
            this.firefoxProxyAction = firefoxProxyAction;
            this.networkId = networkId;
        }

        private void buttonSelectProfile_Click(object sender, RoutedEventArgs e)
        {
            ProfileSelector ps = new ProfileSelector(this.firefoxProxyAction, this.networkId);
            ps.ShowDialog();
        }
    }
}

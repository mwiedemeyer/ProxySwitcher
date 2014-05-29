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

namespace ChangeHomepageAction
{
    /// <summary>
    /// Interaction logic for ChangeHomepageConfig.xaml
    /// </summary>
    public partial class ChangeHomepageConfig : UserControl
    {
        private Guid networkId;
        private ChangeHomepageAction changeHomepageAction;

        public ChangeHomepageConfig()
        {
            InitializeComponent();
        }

        public ChangeHomepageConfig(Guid networkId, ChangeHomepageAction changeHomepageAction)
            : this()
        {
            this.networkId = networkId;
            this.changeHomepageAction = changeHomepageAction;

            InitUI();
        }

        private void InitUI()
        {
            textBoxHomepageURL.Text = this.changeHomepageAction.Settings[networkId + "_Homepage"];
            Browsers browsers = Browsers.Unknown;

            if (!String.IsNullOrEmpty(this.changeHomepageAction.Settings[networkId + "_Browsers"]))
                browsers = (Browsers)Enum.Parse(typeof(Browsers), this.changeHomepageAction.Settings[networkId + "_Browsers"], true);

            checkBoxIE.IsChecked = browsers.HasFlag(Browsers.IE);
            checkBoxOpera.IsChecked = browsers.HasFlag(Browsers.Opera);
            checkBoxFirefox.IsChecked = browsers.HasFlag(Browsers.Firefox);
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            Browsers browsers = Browsers.Unknown;

            if (checkBoxIE.IsChecked.Value)
                browsers = browsers | Browsers.IE;
            if (checkBoxOpera.IsChecked.Value)
                browsers = browsers | Browsers.Opera;
            if (checkBoxFirefox.IsChecked.Value)
                browsers = browsers | Browsers.Firefox;

            this.changeHomepageAction.Save(networkId, textBoxHomepageURL.Text, browsers);
        }
    }
}

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

namespace SocialMediaUpdateAction
{
    /// <summary>
    /// Interaction logic for SocialMediaConfig.xaml
    /// </summary>
    public partial class SocialMediaConfig : UserControl
    {
        private Guid networkId;
        private SocialMediaAction socialMediaAction;

        public SocialMediaConfig()
        {
            InitializeComponent();
        }

        public SocialMediaConfig(Guid networkId, SocialMediaAction socialMediaAction)
            : this()
        {
            this.networkId = networkId;
            this.socialMediaAction = socialMediaAction;

            InitUI();
        }

        private void InitUI()
        {
            // MySite
            checkBoxMySite.IsChecked = String.IsNullOrWhiteSpace(this.socialMediaAction.Settings[networkId + "_MySiteActive"]) ? false : bool.Parse(this.socialMediaAction.Settings[networkId + "_MySiteActive"]);
            textBoxMySiteUrl.Text = this.socialMediaAction.Settings[networkId + "_MySiteUrl"];
            textBoxMySiteStatus.Text = this.socialMediaAction.Settings[networkId + "_MySiteStatus"];

            // Facebook
            checkBoxFB.IsEnabled = !String.IsNullOrEmpty(this.socialMediaAction.Settings["FBAccessToken"]);
            labelEnableFacebook.Visibility = String.IsNullOrEmpty(this.socialMediaAction.Settings["FBAccessToken"]) ? Visibility.Visible : Visibility.Hidden;
            checkBoxFB.IsChecked = String.IsNullOrWhiteSpace(this.socialMediaAction.Settings[networkId + "_FBActive"]) ? false : bool.Parse(this.socialMediaAction.Settings[networkId + "_FBActive"]);
            textBoxFBStatus.Text = this.socialMediaAction.Settings[networkId + "_FBStatus"];

            // Twitter
            checkBoxTwitter.IsEnabled = !String.IsNullOrEmpty(this.socialMediaAction.Settings["TwitterAccessToken"]);
            labelEnableTwitter.Visibility = String.IsNullOrEmpty(this.socialMediaAction.Settings["TwitterAccessToken"]) ? Visibility.Visible : Visibility.Hidden;
            checkBoxTwitter.IsChecked = checkBoxTwitter.IsEnabled && (String.IsNullOrWhiteSpace(this.socialMediaAction.Settings[networkId + "_TwitterActive"]) ? false : bool.Parse(this.socialMediaAction.Settings[networkId + "_TwitterActive"]));
            textBoxTwitterStatus.Text = this.socialMediaAction.Settings[networkId + "_TwitterStatus"];
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            // MySite
            if (checkBoxMySite.IsChecked.Value)
            {
                this.socialMediaAction.SetMySiteSettings(this.networkId, textBoxMySiteUrl.Text, textBoxMySiteStatus.Text);
            }
            else
            {
                this.socialMediaAction.SetDisableMySiteSettings(this.networkId);
            }

            // Facebook
            if (checkBoxFB.IsChecked.Value)
            {
                this.socialMediaAction.SetFBSettings(this.networkId, textBoxFBStatus.Text);
            }
            else
            {
                this.socialMediaAction.SetDisableFBSettings(this.networkId);
            }

            // Twitter
            if (checkBoxTwitter.IsChecked.Value)
            {
                this.socialMediaAction.SetTwitterSettings(this.networkId, textBoxTwitterStatus.Text);
            }
            else
            {
                this.socialMediaAction.SetDisableTwitterSettings(this.networkId);
            }

            this.socialMediaAction.Save();
        }

        private void buttonTwitter_Click(object sender, RoutedEventArgs e)
        {
            TwitterAuthentication window = new TwitterAuthentication(this.socialMediaAction);
            window.ShowDialog();

            InitUI();
        }

        private void buttonFacebook_Click(object sender, RoutedEventArgs e)
        {
            FacebookConnect window = new FacebookConnect(this.socialMediaAction);
            window.ShowDialog();

            InitUI();
        }
    }
}

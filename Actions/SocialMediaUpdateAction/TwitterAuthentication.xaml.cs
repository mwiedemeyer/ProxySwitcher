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
using System.Diagnostics;

namespace SocialMediaUpdateAction
{
    /// <summary>
    /// Interaction logic for TwitterAuthentication.xaml
    /// </summary>
    public partial class TwitterAuthentication : Window
    {
        private SocialMediaAction socialMediaAction;
        private string requestToken;

        public TwitterAuthentication()
        {
            InitializeComponent();
        }

        public TwitterAuthentication(SocialMediaAction socialMediaAction)
            : this()
        {
            this.socialMediaAction = socialMediaAction;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonLogin_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(TwitterClient.GetLoginUrl(out this.requestToken));
        }

        private void buttonAuthenticate_Click(object sender, RoutedEventArgs e)
        {
            string accessToken;
            string accessTokenSecret;

            TwitterClient.CreateAccessToken(this.requestToken, textBoxPIN.Text, out accessToken, out accessTokenSecret);

            this.socialMediaAction.SetTwitterAuthSettings(accessToken, accessTokenSecret);
            this.socialMediaAction.Save();

            this.Close();
        }
    }
}

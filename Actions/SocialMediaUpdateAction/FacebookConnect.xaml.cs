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
    /// Interaction logic for FacebookConnect.xaml
    /// </summary>
    public partial class FacebookConnect : Window
    {
        private SocialMediaAction socialMediaAction;

        public FacebookConnect()
        {
            InitializeComponent();
        }

        public FacebookConnect(SocialMediaAction socialMediaAction)
            : this()
        {
            this.socialMediaAction = socialMediaAction;

            this.webBrowser1.Navigate(FacebookClient.LOGIN_URL);
        }

        private void webBrowser1_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Uri.AbsoluteUri.ToString().StartsWith(FacebookClient.SUCCESS_URL))
            {
                string token = FacebookClient.GetTokenFromResponseUri(e.Uri);
                
                this.socialMediaAction.SetFBAuthSettings(token);
                this.socialMediaAction.Save();
                
                this.Close();
            }
        }
    }
}

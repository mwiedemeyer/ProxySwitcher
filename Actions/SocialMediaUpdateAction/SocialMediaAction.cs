using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxySwitcher.Common;
using System.Reflection;
using System.Windows.Controls;

namespace SocialMediaUpdateAction
{
    [SwitcherActionAddIn]
    public class SocialMediaAction : SwitcherActionBase
    {
        public override string Name
        {
            get { return DefaultResources.SocialMediaActionName; }
        }

        public override string Description
        {
            get { return string.Empty; }
        }

        public override System.IO.Stream IconResourceStream
        {
            get { return Assembly.GetExecutingAssembly().GetManifestResourceStream("SocialMediaUpdateAction.Images.socialmedia.png"); }
        }

        public override Guid Id
        {
            get { return new Guid("AF57B2A7-C98A-4D2B-81BC-958982B6F9F9"); }
        }

        public override string Group
        {
            get { return "Social Media"; }
        }

        public override void Activate(Guid networkId, string networkName)
        {
            // MySite
            bool updateMySite = String.IsNullOrWhiteSpace(Settings[networkId + "_MySiteActive"]) ? false : bool.Parse(Settings[networkId + "_MySiteActive"]);
            string mySiteUrl = Settings[networkId + "_MySiteUrl"];
            if (!String.IsNullOrEmpty(mySiteUrl))
            {
                mySiteUrl = mySiteUrl.EndsWith("/") ? mySiteUrl : mySiteUrl + "/";
                mySiteUrl = mySiteUrl + "_vti_bin/UserProfileService.asmx";
            }
            string mySiteUsername = Environment.UserDomainName + "\\" + Environment.UserName;
            string mySiteStatus = Settings[networkId + "_MySiteStatus"];

            // Facebook
            bool updateFacebook = String.IsNullOrWhiteSpace(Settings[networkId + "_FBActive"]) ? false : bool.Parse(Settings[networkId + "_FBActive"]);
            string fbAccessToken = Settings["FBAccessToken"];
            string fbStatus = Settings[networkId + "_FBStatus"];

            // Twitter
            bool updateTwitter = String.IsNullOrWhiteSpace(Settings[networkId + "_TwitterActive"]) ? false : bool.Parse(Settings[networkId + "_TwitterActive"]);
            string twitterStatus = Settings[networkId + "_TwitterStatus"];
            string twitterAccessToken = Settings["TwitterAccessToken"];
            string twitterAccessTokenSecret = Settings["TwitterAccessTokenSecret"];

            try
            {
                if (updateMySite)
                    SharePointUMManager.UpdateStatus(mySiteUrl, mySiteUsername, mySiteStatus);

                if (updateFacebook && !String.IsNullOrEmpty(fbAccessToken))
                    FacebookClient.UpdateStatus(fbAccessToken, fbStatus);

                if (updateTwitter && !String.IsNullOrEmpty(twitterAccessToken) && !String.IsNullOrEmpty(twitterAccessTokenSecret))
                    TwitterClient.UpdateStatus(twitterStatus, twitterAccessToken, twitterAccessTokenSecret);

            }
            catch (Exception ex)
            {
                if (HostApplication != null)
                    HostApplication.SetStatusText(this, ex.Message, true);
            }
        }

        public override UserControl GetWindowControl(Guid networkId, string networkName)
        {
            return new SocialMediaConfig(networkId, this);
        }

        internal void SetMySiteSettings(Guid networkId, string url, string statusMessage)
        {
            Settings[networkId + "_MySiteActive"] = "true";
            Settings[networkId + "_MySiteUrl"] = url;
            Settings[networkId + "_MySiteStatus"] = statusMessage;
        }

        internal void SetDisableMySiteSettings(Guid networkId)
        {
            Settings[networkId + "_MySiteActive"] = "false";
        }

        internal void SetFBSettings(Guid networkId, string statusMessage)
        {
            Settings[networkId + "_FBActive"] = "true";
            Settings[networkId + "_FBStatus"] = statusMessage;
        }

        internal void SetFBAuthSettings(string accessToken)
        {
            Settings["FBAccessToken"] = accessToken;
        }

        internal void SetDisableFBSettings(Guid networkId)
        {
            Settings[networkId + "_FBActive"] = "false";
        }

        internal void SetTwitterSettings(Guid networkId, string statusMessage)
        {
            Settings[networkId + "_TwitterActive"] = "true";
            Settings[networkId + "_TwitterStatus"] = statusMessage;
        }

        internal void SetTwitterAuthSettings(string accessToken, string accessTokenSecret)
        {
            Settings["TwitterAccessToken"] = accessToken;
            Settings["TwitterAccessTokenSecret"] = accessTokenSecret;
        }

        internal void SetDisableTwitterSettings(Guid networkId)
        {
            Settings[networkId + "_TwitterActive"] = "false";
        }

        internal void Save()
        {
            this.OnSettingsChanged();

            if (HostApplication != null)
                HostApplication.SetStatusText(this, DefaultResources.Saved_Status);
        }


    }
}

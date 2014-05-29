using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.ServiceModel;

namespace SocialMediaUpdateAction
{
    internal static class SharePointUMManager
    {
        internal static void UpdateStatus(string url, string fqnUsername, string newStatusMessage)
        {
            BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
            binding.AllowCookies = true;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
            binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.Ntlm;

            EndpointAddress address = new EndpointAddress(url);

            SPUMProfileService.UserProfileServiceSoapClient client = new SPUMProfileService.UserProfileServiceSoapClient(binding, address);
            client.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
            
            var data = client.GetUserProfileByName(fqnUsername);

            var statusProp = (from d in data
                              where d.Name == "SPS-StatusNotes"
                              select d).FirstOrDefault();

            statusProp.Values[0].Value = newStatusMessage;
            statusProp.IsValueChanged = true;
            client.ModifyUserPropertyByAccountName(fqnUsername, new SPUMProfileService.PropertyData[] { statusProp });
        }
    }
}

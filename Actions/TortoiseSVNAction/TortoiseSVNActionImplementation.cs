using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxySwitcher.Common;
using System.IO;
using System.Reflection;
using ProxySwitcher.Actions.ProxyBase;

namespace TortoiseSVNAction
{
    [SwitcherActionAddIn]
    public class TortoiseSVNActionImplementation : ProxySwitcherAction
    {
        public override string Name
        {
            get { return "TortoiseSVN"; }
        }

        public override string Description
        {
            get { return string.Empty; }
        }

        public override Stream IconResourceStream
        {
            get { return Assembly.GetExecutingAssembly().GetManifestResourceStream("TortoiseSVNAction.TortoiseSVN.png"); }
        }

        public override Guid Id
        {
            get { return new Guid("093707E1-8C67-4AF6-A034-88993432FB3A"); }
        }

        public override string Group
        {
            get { return "SVN Actions 1"; }
        }

        public override void Activate(ProxyEntry proxy, Guid networkId)
        {
            string key1 = "HKEY_CURRENT_USER\\Software\\TortoiseSVN\\Servers\\global";
            string key2 = "HKEY_CURRENT_USER\\Software\\Tigris.org\\Subversion\\Servers\\global";

            RegistryHelper.SetStringValue(key1, "http-proxy-exceptions", proxy.Exceptions);
            RegistryHelper.SetStringValue(key2, "http-proxy-exceptions", proxy.Exceptions);

            RegistryHelper.SetStringValue(key1, "http-proxy-host", proxy.Url.FirstEntry());
            RegistryHelper.SetStringValue(key2, "http-proxy-host", proxy.Url.FirstEntry());

            RegistryHelper.SetStringValue(key1, "http-proxy-port", proxy.Port.FirstEntry());
            RegistryHelper.SetStringValue(key2, "http-proxy-port", proxy.Port.FirstEntry());

            if (proxy.RequiresAuthentication)
            {
                RegistryHelper.SetStringValue(key1, "http-proxy-username", proxy.AuthenticationUsername);
                RegistryHelper.SetStringValue(key2, "http-proxy-username", proxy.AuthenticationUsername);

                RegistryHelper.SetStringValue(key1, "http-proxy-password", proxy.AuthenticationPassword);
                RegistryHelper.SetStringValue(key2, "http-proxy-password", proxy.AuthenticationPassword);
            }
            else
            {
                RegistryHelper.SetStringValue(key1, "http-proxy-username", string.Empty);
                RegistryHelper.SetStringValue(key2, "http-proxy-username", string.Empty);

                RegistryHelper.SetStringValue(key1, "http-proxy-password", string.Empty);
                RegistryHelper.SetStringValue(key2, "http-proxy-password", string.Empty);
            }
        }   

        protected override bool IsAuthenticationSupported
        {
            get { return true; }
        }
    }
}

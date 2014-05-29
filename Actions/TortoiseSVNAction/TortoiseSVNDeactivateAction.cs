using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxySwitcher.Common;
using System.IO;
using System.Reflection;

namespace TortoiseSVNAction
{
    [SwitcherActionAddIn]
    public class TortoiseSVNDeactivateAction : SwitcherActionBase
    {
        public override string Name
        {
            get { return DefaultResources.Disable_TortoiseSVN; }
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
            get { return new Guid("D6734F4C-0768-42EE-A50B-391922C3D9B3"); }
        }

        public override string Group
        {
            get { return "SVN Actions 2"; }
        }

        public override void Activate(Guid networkId, string networkName)
        {
            string key = "HKEY_CURRENT_USER\\Software\\Tigris.org\\Subversion\\Servers\\global";

            RegistryHelper.DeleteEntry(key, "http-proxy-exceptions");
            RegistryHelper.DeleteEntry(key, "http-proxy-host");
            RegistryHelper.DeleteEntry(key, "http-proxy-port");
            RegistryHelper.DeleteEntry(key, "http-proxy-username");
            RegistryHelper.DeleteEntry(key, "http-proxy-password");
        }
    }
}

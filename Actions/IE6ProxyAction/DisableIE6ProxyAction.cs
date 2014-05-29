using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxySwitcher.Common;
using System.IO;
using System.Reflection;

namespace IE6ProxyAction
{
    [SwitcherActionAddIn]
    public class DisableIE6ProxyAction : SwitcherActionBase
    {
        public override string Name
        {
            get { return DefaultResources.DisableIE6; }
        }

        public override string Group
        {
            get { return "IE6"; }
        }

        public override string Description
        {
            get { return string.Empty; }
        }

        public override Stream IconResourceStream
        {
            get { return Assembly.GetExecutingAssembly().GetManifestResourceStream("IE6ProxyAction.Images.ie6.gif"); }
        }

        public override Guid Id
        {
            get { return new Guid("BB262F46-D973-4799-A54B-F1A85BE16B9C"); }
        }

        public override void Activate(Guid networkId, string networkName)
        {
            string sid = System.Security.Principal.WindowsIdentity.GetCurrent().User.Value;

            RegistryHelper.SetDWordValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings", "ProxyEnable", 0);
            RegistryHelper.SetDWordValue(@"HKEY_USERS\" + sid + @"\Software\Microsoft\Windows\CurrentVersion\Internet Settings", "ProxyEnable", 0);
            RegistryHelper.DeleteEntry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings", "AutoConfigURL");
            RegistryHelper.DeleteEntry(@"HKEY_USERS\" + sid + @"\Software\Microsoft\Windows\CurrentVersion\Internet Settings", "AutoConfigURL");

            InternetExplorer6ProxyAction.RefreshIESettings();
        }
    }
}

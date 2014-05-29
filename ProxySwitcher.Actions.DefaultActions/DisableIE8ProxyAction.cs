using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxySwitcher.Common;
using System.IO;
using System.Reflection;

namespace ProxySwitcher.Actions.DefaultActions
{
    [SwitcherActionAddIn]
    public class DisableIE8ProxyAction : SwitcherActionBase
    {
        public override string Name
        {
            get { return DefaultResources.DisableIE8; }
        }

        public override string Group
        {
            get { return "IE"; }
        }

        public override string Description
        {
            get { return string.Empty; }
        }

        public override Stream IconResourceStream
        {
            get { return Assembly.GetExecutingAssembly().GetManifestResourceStream("ProxySwitcher.Actions.DefaultActions.Images.ie8.png"); }
        }

        public override Guid Id
        {
            get { return new Guid("E451A76A-F1A5-4B02-B1A1-965291A5CCA5"); }
        }

        public override void Activate(Guid networkId, string networkName)
        {
            string sid = System.Security.Principal.WindowsIdentity.GetCurrent().User.Value;

            byte enabled = 01; // 01=disabled, 03=enabled, 05=auto config, 09=auto detect, 0D=auto config und auto detect
            byte[] configStart = new byte[] { 70, 00, 00, 00, 01, 00, 00, 03, enabled };
            byte[] merged = new byte[configStart.Length + 59];
            configStart.CopyTo(merged, 0);
            new byte[59].CopyTo(merged, configStart.Length);

            //Dial up connections ermoeglichen
            //if (UseDialUp(networkId))
            //{
            //    string connectionName = GetDialUpName(networkId);
            //    RegistryHelper.SetBinaryValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Connections", connectionName, merged);
            //}
            //else
            //{
                RegistryHelper.SetBinaryValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Connections", "DefaultConnectionSettings", merged);
            //}

            InternetExplorer8ProxyAction.RefreshIESettings();
        }
    }
}

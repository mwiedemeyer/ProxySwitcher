using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxySwitcher.Common;
using ProxySwitcher.Actions.ProxyBase;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;

namespace IE6ProxyAction
{
    [SwitcherActionAddIn]
    public class InternetExplorer6ProxyAction : ProxySwitcherAction
    {
        #region Update Internet Explorer with P/Invoke

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);

        public static void RefreshIESettings()
        {
            const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
        }

        #endregion

        protected override bool IsAuthenticationSupported
        {
            get { return false; }
        }

        public override void Activate(ProxyEntry entry, Guid networkId)
        {
            string proxy = entry.Url.FirstEntry() + ":" + entry.Port.FirstEntry().ToString();
            string sid = System.Security.Principal.WindowsIdentity.GetCurrent().User.Value;

            if (!entry.IsAutoConf) //only if not autoconf. autoconf is set in enable/disableProxy
            {
                RegistryHelper.SetStringValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings", "ProxyServer", proxy);
                RegistryHelper.SetStringValue(@"HKEY_USERS\" + sid + @"\Software\Microsoft\Windows\CurrentVersion\Internet Settings", "ProxyServer", proxy);

                if (entry.ByPassLocal || !String.IsNullOrEmpty(entry.Exceptions))
                {
                    string byPassString = entry.Exceptions;

                    if (entry.ByPassLocal)
                    {
                        if (byPassString.Length != 0)
                            byPassString += ";<local>";
                        else
                            byPassString = "<local>";
                    }

                    RegistryHelper.SetStringValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings", "ProxyOverride", byPassString);
                    RegistryHelper.SetStringValue(@"HKEY_USERS\" + sid + @"\Software\Microsoft\Windows\CurrentVersion\Internet Settings", "ProxyOverride", byPassString);
                }
                else
                {
                    RegistryHelper.DeleteEntry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings", "ProxyOverride");
                    RegistryHelper.DeleteEntry(@"HKEY_USERS\" + sid + @"\Software\Microsoft\Windows\CurrentVersion\Internet Settings", "ProxyOverride");
                }
            }


            if (!entry.IsAutoConf)
            {
                RegistryHelper.SetDWordValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings", "ProxyEnable", 1);
                RegistryHelper.SetDWordValue(@"HKEY_USERS\" + sid + @"\Software\Microsoft\Windows\CurrentVersion\Internet Settings", "ProxyEnable", 1);
            }
            else
            {
                RegistryHelper.SetStringValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings", "AutoConfigURL", entry.Url.FirstEntry());
                RegistryHelper.SetStringValue(@"HKEY_USERS\" + sid + @"\Software\Microsoft\Windows\CurrentVersion\Internet Settings", "AutoConfigURL", entry.Url.FirstEntry());
            }

            RefreshIESettings();
        }

        public override string Name
        {
            get { return "Internet Explorer 6"; }
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
            get { return new Guid("7B305A04-55D6-426D-98C1-559AFFA95444"); }
        }
    }
}

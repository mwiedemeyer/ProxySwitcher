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
    public class DisableOperaProxyAction : SwitcherActionBase
    {
        public override string Name
        {
            get { return DefaultResources.DisableOpera; }
        }

        public override string Group
        {
            get { return "Opera"; }
        }

        public override string Description
        {
            get { return string.Empty; }
        }

        public override Stream IconResourceStream
        {
            get { return Assembly.GetExecutingAssembly().GetManifestResourceStream("ProxySwitcher.Actions.DefaultActions.Images.opera.png"); }
        }

        public override Guid Id
        {
            get { return new Guid("1CB4740C-1E4F-4591-945D-F99519C0AD4B"); }
        }

        public override void Activate(Guid networkId, string networkName)
        {
            IniHelper helper = new IniHelper(GetOperaIniFile());
            helper.SetValue("Proxy", "Use HTTP", "0");
            helper.SetValue("Proxy", "Use HTTPS", "0");
            helper.SetValue("Proxy", "Use FTP", "0");
            helper.SetValue("Proxy", "Use Automatic Proxy Configuration", "0");
            helper.Save();
        }

        private string GetOperaIniFile()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Opera\\Opera");
            return Path.Combine(path, "operaprefs.ini");
        }
    }
}

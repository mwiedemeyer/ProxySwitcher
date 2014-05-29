using System;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Xml.Linq;
using ProxySwitcher.Actions.ProxyBase;
using ProxySwitcher.Common;
using System.Xml.XPath;

namespace SkypeSwitcher
{
    [SwitcherActionAddIn]
    public class DisableSkypeAction : SwitcherActionBase
    {
        public override string Name
        {
            get { return "Disable Skype Proxy"; }
        }

        public override string Description
        {
            // currently not used
            get { return string.Empty; }
        }

        public override Stream IconResourceStream
        {
            get { return Assembly.GetExecutingAssembly().GetManifestResourceStream("SkypeSwitcher.actionlogo.png"); }
        }

        public override Guid Id
        {
            get { return new Guid("{518A2F63-D2E0-4FDB-AFF3-845DC9B4BDDD}"); }
        }

        public override string Group
        {
            // Actions which should shown together can be grouped by this property.
            get { return "Skype1"; }
        }

        public override void Activate(Guid networkId, string networkName)
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Skype", "shared.xml");
            if (!File.Exists(file))
            {
                this.HostApplication.SetStatusText(this, "Skype config file not found", true);
                return;
            }

            var doc = XDocument.Load(file);
            var connectionElement = doc.Root.XPathSelectElement("Lib/Connection");

            XElement proxyElement = null;

            proxyElement = connectionElement.XPathSelectElement("HttpsProxy");
            if (proxyElement != null)
            {
                var enabl = proxyElement.Element("Enable");
                enabl.SetValue("0");
            }
            proxyElement = connectionElement.XPathSelectElement("SocksProxy");
            if (proxyElement != null)
            {
                var enabl = proxyElement.Element("Enable");
                enabl.SetValue("0");
            }

            doc.Save(file);
        }
    }
}
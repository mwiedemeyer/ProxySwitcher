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
    public class SkypeAction : ProxySwitcherAction
    {
        public override string Name
        {
            get { return "Skype Proxy"; }
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
            get { return new Guid("{518A2F63-D2E0-4FDB-AFF3-845DC9B4BDDC}"); }
        }

        public override string Group
        {
            // Actions which should shown together can be grouped by this property.
            get { return "Skype"; }
        }

        public override void Activate(ProxySwitcher.Actions.ProxyBase.ProxyEntry proxy, Guid networkId)
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Skype", "shared.xml");
            if (!File.Exists(file))
            {
                this.HostApplication.SetStatusText(this, "Skype config file not found", true);
                return;
            }

            var http = false;
            var socks = false;
            string proxyUrl = string.Empty;

            if (proxy.Url.IsAllSet)
            {
                proxyUrl = String.Format("{0}:{1}", proxy.Url[ProxyScheme.All], proxy.Port[ProxyScheme.All]);
                http = true;
            }
            else
            {
                if (proxy.Url.ContainsKey(ProxyScheme.HTTP))
                {
                    proxyUrl = String.Format("{0}:{1}", proxy.Url[ProxyScheme.HTTP], proxy.Port[ProxyScheme.HTTP]);
                    http = true;
                }
                if (proxy.Url.ContainsKey(ProxyScheme.HTTPS))
                {
                    proxyUrl = String.Format("{0}:{1}", proxy.Url[ProxyScheme.HTTPS], proxy.Port[ProxyScheme.HTTPS]);
                    http = true;
                }
                if (proxy.Url.ContainsKey(ProxyScheme.SOCKS))
                {
                    proxyUrl = String.Format("{0}:{1}", proxy.Url[ProxyScheme.SOCKS], proxy.Port[ProxyScheme.SOCKS]);
                    http = false;
                    socks = true;
                }
            }

            var doc = XDocument.Load(file);
            var connectionElement = doc.Root.XPathSelectElement("Lib/Connection");

            XElement proxyElement = null;

            if (http)
                proxyElement = connectionElement.XPathSelectElement("HttpsProxy");
            else if (socks)
                proxyElement = connectionElement.XPathSelectElement("SocksProxy");

            if (proxyElement == null)
            {
                var addrElement = new XElement("Addr", proxyUrl);
                var enableElement = new XElement("Enable", "1");

                if (http)
                    connectionElement.Add(new XElement("HttpsProxy", addrElement, enableElement));
                else if (socks)
                    connectionElement.Add(new XElement("SocksProxy", addrElement, enableElement));
            }
            else
            {
                var addr = proxyElement.Element("Addr");
                addr.SetValue(proxyUrl);

                var enabl = proxyElement.Element("Enable");
                enabl.SetValue("1");
            }

            doc.Save(file);
        }

        protected override bool IsAuthenticationSupported
        {
            get { return false; }
        }
    }
}
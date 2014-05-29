using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxySwitcher.Common;
using System.IO;
using System.Reflection;
using ProxySwitcher.Actions.ProxyBase;

namespace ProxySwitcher.Actions.DefaultActions
{
    [SwitcherActionAddIn]
    public class OperaProxyAction : ProxySwitcherAction
    {
        protected override bool IsAuthenticationSupported
        {
            get { return false; }
        }

        public override void ValidateEntry(ProxyEntry proxy)
        {
            if (proxy.Exceptions.Contains("/"))
                throw new ProxyValidationException(DefaultResources.ProxyExceptionsError);
        }

        public override void Activate(ProxyEntry proxy, Guid networkId)
        {
            IniHelper helper = new IniHelper(GetOperaIniFile());

            if (proxy.IsAutoConf)
            {
                helper.SetValue("Proxy", "Automatic Proxy Configuration URL", proxy.Url.FirstEntry());
                helper.SetValue("Proxy", "Use Automatic Proxy Configuration", "1");
            }
            else
            {
                if (proxy.Url.IsAllSet)
                {
                    helper.SetValue("Proxy", "HTTP server", proxy.Url[ProxyScheme.All] + ":" + proxy.Port[ProxyScheme.All].ToString());
                    helper.SetValue("Proxy", "HTTPS server", proxy.Url[ProxyScheme.All] + ":" + proxy.Port[ProxyScheme.All].ToString());
                    helper.SetValue("Proxy", "Use HTTP", "1");
                    helper.SetValue("Proxy", "Use HTTPS", "1");
                }
                else
                {
                    if (proxy.Url.ContainsKey(ProxyScheme.HTTP))
                    {
                        helper.SetValue("Proxy", "HTTP server", proxy.Url[ProxyScheme.HTTP] + ":" + proxy.Port[ProxyScheme.HTTP].ToString());
                        helper.SetValue("Proxy", "Use HTTP", "1");
                    }
                    if (proxy.Url.ContainsKey(ProxyScheme.HTTPS))
                    {
                        helper.SetValue("Proxy", "HTTPS server", proxy.Url[ProxyScheme.HTTPS] + ":" + proxy.Port[ProxyScheme.HTTPS].ToString());
                        helper.SetValue("Proxy", "Use HTTPS", "1");
                    }
                    if (proxy.Url.ContainsKey(ProxyScheme.FTP))
                    {
                        helper.SetValue("Proxy", "FTP server", proxy.Url[ProxyScheme.FTP] + ":" + proxy.Port[ProxyScheme.FTP].ToString());
                        helper.SetValue("Proxy", "Use FTP", "1");
                    }
                }

                if (String.IsNullOrEmpty(proxy.Exceptions))
                {
                    helper.SetValue("Proxy", "No Proxy Servers Check", "0");
                }
                else
                {
                    helper.SetValue("Proxy", "No Proxy Servers", proxy.Exceptions);
                    helper.SetValue("Proxy", "No Proxy Servers Check", "1");
                }

                if (proxy.ByPassLocal)
                {
                    helper.SetValue("Proxy", "Use Proxy On Local Names Check", "0");
                }
                else
                {
                    helper.SetValue("Proxy", "Use Proxy On Local Names Check", "1");
                }
            }

            helper.Save();
        }

        public override ProxyEntry GetDefaultProxy()
        {
            ProxyEntry proxy = new ProxyEntry();

            IniHelper helper = new IniHelper(GetOperaIniFile());

            proxy.IsAutoConf = (helper.GetValue("Proxy", "Use Automatic Proxy Configuration") == "1");
            if (proxy.IsAutoConf)
            {
                proxy.Url[ProxyScheme.All] = helper.GetValue("Proxy", "Automatic Proxy Configuration URL");
            }
            else
            {
                string[] urlPort = helper.GetValue("Proxy", "HTTP server").Split(':');
                proxy.Url[ProxyScheme.All] = urlPort[0];
                int port;
                if (urlPort.Length > 1 && int.TryParse(urlPort[1], out port))
                    proxy.Port[ProxyScheme.All] = port;
            }

            return proxy;
        }

        private string GetOperaIniFile()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Opera\\Opera");
            return Path.Combine(path, "operaprefs.ini");
        }

        public override string Name
        {
            get { return "Opera"; }
        }

        public override string Group
        {
            get { return "Opera"; }
        }

        public override string Description
        {
            get { return DefaultResources.Opera_Description; }
        }

        public override Stream IconResourceStream
        {
            get { return Assembly.GetExecutingAssembly().GetManifestResourceStream("ProxySwitcher.Actions.DefaultActions.Images.opera.png"); }
        }

        public override Guid Id
        {
            get { return new Guid("5E0E317C-F643-462E-A5A2-5ED945EF4C68"); }
        }
    }
}

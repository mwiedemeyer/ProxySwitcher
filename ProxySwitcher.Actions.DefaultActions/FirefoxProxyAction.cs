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
    public class FirefoxProxyAction : ProxySwitcherAction
    {
        protected override object GetCustomContent(Guid networkId)
        {
            return new Firefox.FirefoxCustomContent(this, networkId);
        }

        protected override bool IsAuthenticationSupported
        {
            get { return false; }
        }

        public void SaveData(Guid networkId, string profileToSwitch, string profileFolder)
        {
            if (String.IsNullOrWhiteSpace(profileToSwitch))
                Settings.Remove(networkId.ToString() + "_ProfileToSwitch");
            else
                Settings[networkId.ToString() + "_ProfileToSwitch"] = profileToSwitch;

            Settings[networkId.ToString() + "_ProfileFolder"] = profileFolder;

            OnSettingsChanged();
        }

        public string GetProfileToSwitch(Guid networkId)
        {
            if (this.Settings.ContainsKey(networkId.ToString() + "_ProfileToSwitch"))
                return Settings[networkId.ToString() + "_ProfileToSwitch"].ToString();
            return String.Empty;
        }

        public string GetProfileFolder(Guid networkId)
        {
            if (Settings.ContainsKey(networkId.ToString() + "_ProfileFolder"))
                return Settings[networkId.ToString() + "_ProfileFolder"].ToString();
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles");
        }

        public List<string> GetAllProfiles(Guid networkId)
        {
            try
            {
                string path = GetProfileFolder(networkId);
                List<string> lst = new List<string>();
                foreach (string dir in Directory.GetDirectories(path))
                {
                    lst.Add(new DirectoryInfo(dir).Name);
                }
                return lst;
            }
            catch { return new List<string>(); }
        }

        public override void Activate(ProxyEntry proxy, Guid networkId)
        {
            string file = GetProfileFolder(networkId);
            // all
            if (String.IsNullOrEmpty(GetProfileToSwitch(networkId)))
            {
                foreach (string profPath in GetAllProfiles(networkId))
                {
                    file = Path.Combine(file, profPath);
                    file = Path.Combine(file, "prefs.js");
                    EnableProxy(file, proxy);
                }
            }
            else
            {
                file = Path.Combine(file, GetProfileToSwitch(networkId));
                file = Path.Combine(file, "prefs.js");
                EnableProxy(file, proxy);
            }
        }

        private void EnableProxy(string file, ProxyEntry proxy)
        {
            string content = "";
            using (StreamReader sr = new StreamReader(file))
            {
                content = sr.ReadToEnd();
            }

            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                using (StringReader sr = new StringReader(content))
                {
                    while (true)
                    {
                        string line = sr.ReadLine();
                        if (line == null)
                            break;
                        if (line.StartsWith("user_pref(\"network.proxy.type"))
                            continue;
                        if (line.StartsWith("user_pref(\"network.proxy.autoconfig_url"))
                            continue;
                        if (line.StartsWith("user_pref(\"network.proxy.http"))
                            continue;
                        if (line.StartsWith("user_pref(\"network.proxy.http_port"))
                            continue;
                        if (line.StartsWith("user_pref(\"network.proxy.ssl"))
                            continue;
                        if (line.StartsWith("user_pref(\"network.proxy.ssl_port"))
                            continue;
                        if (line.StartsWith("user_pref(\"network.proxy.ftp"))
                            continue;
                        if (line.StartsWith("user_pref(\"network.proxy.ftp_port"))
                            continue;
                        if (line.StartsWith("user_pref(\"network.proxy.socks"))
                            continue;
                        if (line.StartsWith("user_pref(\"network.proxy.socks_port"))
                            continue;
                        if (line.StartsWith("user_pref(\"network.proxy.no_proxies_on"))
                            continue;
                        if (line.StartsWith("user_pref(\"network.proxy.share_proxy_settings"))
                            continue;

                        sw.WriteLine(line);
                    }
                }

                if (proxy.IsAutoConf)
                {
                    sw.WriteLine("user_pref(\"network.proxy.autoconfig_url\", \"" + proxy.Url.FirstEntry() + "\");");
                    sw.WriteLine("user_pref(\"network.proxy.type\", 2);");
                }
                else if (proxy.IsAutoDetect)
                {
                    sw.WriteLine("user_pref(\"network.proxy.type\", 4);");
                }
                else
                {
                    if (proxy.Url.IsAllSet)
                    {
                        sw.WriteLine("user_pref(\"network.proxy.http\", \"" + proxy.Url[ProxyScheme.All] + "\");");
                        sw.WriteLine("user_pref(\"network.proxy.http_port\", " + proxy.Port[ProxyScheme.All].ToString() + ");");
                        sw.WriteLine("user_pref(\"network.proxy.ssl\", \"" + proxy.Url[ProxyScheme.All] + "\");");
                        sw.WriteLine("user_pref(\"network.proxy.ssl_port\", " + proxy.Port[ProxyScheme.All].ToString() + ");");
                        sw.WriteLine("user_pref(\"network.proxy.ftp\", \"" + proxy.Url[ProxyScheme.All] + "\");");
                        sw.WriteLine("user_pref(\"network.proxy.ftp_port\", " + proxy.Port[ProxyScheme.All].ToString() + ");");
                        sw.WriteLine("user_pref(\"network.proxy.socks\", \"" + proxy.Url[ProxyScheme.All] + "\");");
                        sw.WriteLine("user_pref(\"network.proxy.socks_port\", " + proxy.Port[ProxyScheme.All].ToString() + ");");
                    }
                    else
                    {
                        if (proxy.Url.ContainsKey(ProxyScheme.HTTP))
                        {
                            sw.WriteLine("user_pref(\"network.proxy.http\", \"" + proxy.Url[ProxyScheme.HTTP] + "\");");
                            sw.WriteLine("user_pref(\"network.proxy.http_port\", " + proxy.Port[ProxyScheme.HTTP].ToString() + ");");
                        }
                        if (proxy.Url.ContainsKey(ProxyScheme.HTTPS))
                        {
                            sw.WriteLine("user_pref(\"network.proxy.ssl\", \"" + proxy.Url[ProxyScheme.HTTPS] + "\");");
                            sw.WriteLine("user_pref(\"network.proxy.ssl_port\", " + proxy.Port[ProxyScheme.HTTPS].ToString() + ");");
                        }
                        if (proxy.Url.ContainsKey(ProxyScheme.FTP))
                        {
                            sw.WriteLine("user_pref(\"network.proxy.ftp\", \"" + proxy.Url[ProxyScheme.FTP] + "\");");
                            sw.WriteLine("user_pref(\"network.proxy.ftp_port\", " + proxy.Port[ProxyScheme.FTP].ToString() + ");");
                        }
                        if (proxy.Url.ContainsKey(ProxyScheme.SOCKS))
                        {
                            sw.WriteLine("user_pref(\"network.proxy.socks\", \"" + proxy.Url[ProxyScheme.SOCKS] + "\");");
                            sw.WriteLine("user_pref(\"network.proxy.socks_port\", " + proxy.Port[ProxyScheme.SOCKS].ToString() + ");");
                        }
                    }
                    string noProxies = proxy.Exceptions;
                    if (proxy.ByPassLocal)
                        noProxies = "localhost, 127.0.0.1, " + noProxies;
                    sw.WriteLine("user_pref(\"network.proxy.no_proxies_on\", \"" + noProxies + "\");");
                    sw.WriteLine("user_pref(\"network.proxy.share_proxy_settings\", true);");
                    sw.WriteLine("user_pref(\"network.proxy.type\", 1);");
                }
            }

            using (StreamWriter sw = new StreamWriter(file))
            {
                sw.Write(sb.ToString());
            }
        }

        public override string Name
        {
            get { return "Firefox"; }
        }

        public override string Group
        {
            get { return "Firefox"; }
        }

        public override string Description
        {
            get { return DefaultResources.Firefox_Description; }
        }

        public override Stream IconResourceStream
        {
            get { return Assembly.GetExecutingAssembly().GetManifestResourceStream("ProxySwitcher.Actions.DefaultActions.Images.firefox.png"); }
        }

        public override Guid Id
        {
            get { return new Guid("8020E480-B835-406B-946E-983EF6E527BE"); }
        }

    }
}

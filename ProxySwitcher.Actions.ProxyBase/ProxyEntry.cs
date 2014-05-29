using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxySwitcher.Actions.ProxyBase
{
    [Serializable]
    public class ProxyEntry
    {
        private ProxySettingWithScheme<string> urlStore = new ProxySettingWithScheme<string>();
        private ProxySettingWithScheme<int> portStore = new ProxySettingWithScheme<int>();

        public ProxyEntry(string url, int port, bool byPassLocal)
        {
            this.Url[ProxyScheme.All] = url;
            this.Port[ProxyScheme.All] = port;
            this.ByPassLocal = byPassLocal;
        }

        public ProxyEntry(string url, int port)
            : this(url, port, false)
        {
        }

        public ProxyEntry(string autoconfUrl)
            : this("", 0, false)
        {
            this.IsAutoConf = true;
            this.Url[ProxyScheme.All] = autoconfUrl;
        }

        public ProxyEntry() { }

        public ProxySettingWithScheme<string> Url
        {
            get { return this.urlStore; }
            set { this.urlStore = value; }
        }

        public ProxySettingWithScheme<int> Port
        {
            get { return this.portStore; }
            set { this.portStore = value; }
        }

        public bool ByPassLocal { get; set; }

        public bool IsAutoConf { get; set; }

        public bool IsAutoDetect { get; set; }

        public string Exceptions { get; set; }

        public bool RequiresAuthentication { get; set; }

        public string AuthenticationUsername { get; set; }

        public string AuthenticationPassword { get; set; }
    }
}

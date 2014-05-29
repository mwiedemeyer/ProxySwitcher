using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using ProxySwitcher.Common;
using System.Security.Cryptography;

namespace ProxySwitcher.Actions.ProxyBase
{
    public abstract class ProxySwitcherAction : SwitcherActionBase
    {
        public abstract void Activate(ProxyEntry proxy, Guid networkId);

        public virtual ProxyEntry GetDefaultProxy()
        {
            return null;
        }

        public sealed override void Activate(Guid networkId, string networkName)
        {
            var proxy = GetProxyEntryFromSettings(networkId);
            this.Activate(proxy, networkId);
        }

        public sealed override UserControl GetWindowControl(Guid networkId, string networkName)
        {
            var psuc = new ProxySwitcherUserControl(this, networkId);

            psuc.ShowAuthentication = IsAuthenticationSupported;
            psuc.SetDefaultProxy(GetDefaultProxy());

            var additionalContent = GetCustomContent(networkId);
            if (additionalContent != null)
                psuc.SetCustomContent(additionalContent);

            return psuc;
        }

        protected virtual object GetCustomContent(Guid networkId)
        {
            return null;
        }

        protected abstract bool IsAuthenticationSupported { get; }

        /// <summary>
        /// Should throw a ProxyValidationException on error.
        /// </summary>
        /// <param name="proxy"></param>
        public virtual void ValidateEntry(ProxyEntry proxy) { }

        public ProxyEntry GetProxyEntryFromSettings(Guid networkId)
        {
            string nwId = networkId.ToString();

            ProxyEntry proxy = new ProxyEntry();
            if (!String.IsNullOrEmpty(this.Settings[nwId + "_Url"]))
            {
                proxy.Url[ProxyScheme.All] = this.Settings[nwId + "_Url"];
                proxy.Port[ProxyScheme.All] = this.Settings.ContainsKey(nwId + "_Port") ? int.Parse(this.Settings[nwId + "_Port"]) : 0;
            }
            else
            {
                foreach (var key in this.Settings.Keys)
                {
                    if (key.Contains(nwId + "_Url_"))
                    {
                        proxy.Url[key.Split('_')[2]] = this.Settings[key];
                    }
                    if (key.Contains(nwId + "_Port_"))
                    {
                        proxy.Port[key.Split('_')[2]] = int.Parse(this.Settings[key]);
                    }
                }
            }

            proxy.IsAutoConf = this.Settings.ContainsKey(nwId + "_IsAutoConf") ? bool.Parse(this.Settings[nwId + "_IsAutoConf"]) : false;
            proxy.IsAutoDetect = this.Settings.ContainsKey(nwId + "_IsAutoDetect") ? bool.Parse(this.Settings[nwId + "_IsAutoDetect"]) : false;
            proxy.ByPassLocal = this.Settings.ContainsKey(nwId + "_ByPassLocal") ? bool.Parse(this.Settings[nwId + "_ByPassLocal"]) : false;
            proxy.Exceptions = this.Settings[nwId + "_Exceptions"];
            proxy.RequiresAuthentication = this.Settings.ContainsKey(nwId + "_RequiresAuthentication") ? bool.Parse(this.Settings[nwId + "_RequiresAuthentication"]) : false;
            proxy.AuthenticationUsername = this.Settings[nwId + "_AuthenticationUsername"];
            proxy.AuthenticationPassword = Decrypt(this.Settings[nwId + "_AuthenticationPassword"]);

            return proxy;
        }

        public void SetProxyEntryToSettings(Guid networkId, ProxyEntry proxy)
        {
            string nwId = networkId.ToString();

            this.Settings.Remove(nwId + "_Url_" + ProxyScheme.All.ToString());
            this.Settings.Remove(nwId + "_Port_" + ProxyScheme.All.ToString());

            foreach (var key in proxy.Url.Keys)
            {
                this.Settings[nwId + "_Url_" + key.ToString()] = proxy.Url[key];
                this.Settings[nwId + "_Port_" + key.ToString()] = proxy.Port[key].ToString();
            }
            
            //remove old storage system
            this.Settings.Remove(nwId + "_Url");
            this.Settings.Remove(nwId + "_Port");
            
            this.Settings[nwId + "_IsAutoConf"] = proxy.IsAutoConf.ToString();
            this.Settings[nwId + "_IsAutoDetect"] = proxy.IsAutoDetect.ToString();
            this.Settings[nwId + "_ByPassLocal"] = proxy.ByPassLocal.ToString();
            this.Settings[nwId + "_Exceptions"] = proxy.Exceptions;
            this.Settings[nwId + "_RequiresAuthentication"] = proxy.RequiresAuthentication.ToString();
            this.Settings[nwId + "_AuthenticationUsername"] = proxy.AuthenticationUsername;
            this.Settings[nwId + "_AuthenticationPassword"] = Encrypt(proxy.AuthenticationPassword);

            OnSettingsChanged();

            if (HostApplication != null)
                HostApplication.SetStatusText(this, ActionResources.Saved_Status);
        }

        private string Encrypt(string data)
        {
            if (String.IsNullOrWhiteSpace(data))
                return data;
            return Convert.ToBase64String(ProtectedData.Protect(Encoding.Default.GetBytes(data), null, DataProtectionScope.CurrentUser));
        }

        private string Decrypt(string data)
        {
            if (String.IsNullOrWhiteSpace(data))
                return data;
            return Encoding.Default.GetString(ProtectedData.Unprotect(Convert.FromBase64String(data), null, DataProtectionScope.CurrentUser));
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxySwitcher.Common;
using ProxySwitcher.Core.Resources;
using ProxySwitcher.Core.NativeWifi;
using System.Net.NetworkInformation;
using System.Net;
using System.Collections.ObjectModel;

namespace ProxySwitcher.Core
{
    [Serializable]
    public class NetworkConfiguration
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string IconPath { get; set; }

        public NetworkConfigurationMethod Method { get; set; }

        public string Query { get; set; }

        public bool Active { get; set; }

        public Guid[] Actions { get; set; }

        public string NetworkInterfaceName { get; set; }

        public string NetworkInterfaceId { get; set; }

        public bool IsNetworkInterfaceDependend
        {
            get { return !String.IsNullOrWhiteSpace(this.NetworkInterfaceId); }
        }

        public bool IsUnknownNetwork
        {
            get { return (Id == SwitcherActionBase.DeactivateNetworkId); }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public void AddAction(Guid id)
        {
            List<Guid> list;
            if (Actions == null)
                list = new List<Guid>();
            else
                list = new List<Guid>(Actions);

            if (list.Contains(id))
                return;

            list.Add(id);
            Actions = list.ToArray();
        }

        public void DeleteAction(Guid id)
        {
            if (Actions == null)
                return;

            var list = new List<Guid>(Actions);
            list.Remove(id);
            Actions = list.ToArray();
        }

        public static NetworkConfiguration CreateNewNetworkConfiguration()
        {
            var nc = new NetworkConfiguration();

            nc.Id = Guid.NewGuid();
            nc.Name = LanguageResources.NewNetworkName;
            nc.Active = true;

            // Pre populate
            string wlanSSID = string.Empty;
            string gateway = string.Empty;
            string gatewayMAC = string.Empty;
            string dnsSuffix = string.Empty;
            string ip = string.Empty;
            string location = string.Empty;

            NetworkManager.GetPrePopulatedNetworkMethods(out wlanSSID, out gateway, out gatewayMAC, out dnsSuffix, out ip, out location);

            // 1. Try WLAN
            if (!String.IsNullOrWhiteSpace(wlanSSID))
            {
                nc.Method = NetworkConfigurationMethod.WLANSSID;
                nc.Query = wlanSSID;
            }
            else
            {
                // 2. Try Gateway and DNS suffix
                if (!String.IsNullOrWhiteSpace(dnsSuffix))
                {
                    nc.Method = NetworkConfigurationMethod.DNSSuffix;
                    nc.Query = dnsSuffix;
                }
                else if (!String.IsNullOrWhiteSpace(gateway))
                {
                    nc.Method = NetworkConfigurationMethod.Gateway;
                    nc.Query = gateway;
                }
                else
                {
                    // 3. Try IP address
                    if (!String.IsNullOrWhiteSpace(ip))
                    {
                        nc.Method = NetworkConfigurationMethod.IP;
                        nc.Query = ip;
                    }
                }
            }

            return nc;
        }
    }
}

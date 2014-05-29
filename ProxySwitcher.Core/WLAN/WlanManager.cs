using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxySwitcher.Core.NativeWifi;

namespace ProxySwitcher.Core.NativeWifi
{
    public static class WlanManager
    {
        public static string GetCurrentSSID()
        {
            try
            {
                using (WlanClient wc = new WlanClient())
                {
                    foreach (WlanClient.WlanInterface intf in wc.Interfaces)
                    {
                        if (intf.InterfaceState != Wlan.WlanInterfaceState.Connected)
                            continue;
                        if (intf.CurrentConnection.isState != Wlan.WlanInterfaceState.Connected)
                            continue;

                        return intf.CurrentConnection.profileName;
                    }
                }
            }
            catch (Exception ex) { Logger.Log("Getting WLAN SSID failed", ex); }

            return String.Empty;
        }

        public static bool IsSSIDAvailable(string ssid)
        {
            try
            {
                var ssidUpper = ssid.ToUpper();

                using (WlanClient wc = new WlanClient())
                {
                    foreach (WlanClient.WlanInterface intf in wc.Interfaces)
                    {
                        foreach (var network in intf.GetAvailableNetworkList(Wlan.WlanGetAvailableNetworkFlags.IncludeAllAdhocProfiles))
                        {
                            string detectedSSID = Encoding.Default.GetString(network.dot11Ssid.SSID);
                            if (detectedSSID.ToUpper().Contains(ssidUpper) && network.networkConnectable)
                                return true;
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Log("Scanning SSIDs failed", ex); }

            return false;
        }
    }
}

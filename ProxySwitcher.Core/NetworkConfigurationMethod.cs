using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxySwitcher.Core
{
    [Serializable]
    public enum NetworkConfigurationMethod : int
    {
        // Needs to be in sync with LanguageResources
        Unknown = 0,
        DNSSuffix = 1,
        Gateway = 2,
        WLANSSID = 3,
        IP = 4,
        Location = 5,
        WLANAvailable = 6,
        ServerAvailable = 7,
        GatewayMAC = 8,
        DockingStationState = 9
    }
}

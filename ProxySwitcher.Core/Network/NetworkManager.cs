using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using Microsoft.Win32;
using System.Threading;
using ProxySwitcher.Core.NativeWifi;
using System.Net;
using ProxySwitcher.Common;
using System.Threading.Tasks;

namespace ProxySwitcher.Core
{
    public class NetworkManager : IDisposable
    {
        private Semaphore semaphore = new Semaphore(1, 1);
        private AddInManager addInManager;
        private LocationManager locationManager;

        private NetworkAddressChangedEventHandler networkChangedHandler;
        private PowerModeChangedEventHandler powerModeChangedHandler;

        public NetworkManager(AddInManager addInManager)
        {
            this.addInManager = addInManager;

            networkChangedHandler = new NetworkAddressChangedEventHandler(NetworkChange_NetworkAddressChanged);
            NetworkChange.NetworkAddressChanged += networkChangedHandler;

            powerModeChangedHandler = new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
            SystemEvents.PowerModeChanged += powerModeChangedHandler;

            if (Windows7Helper.IsWindows7)
            {
                locationManager = LocationManager.Instance;
                locationManager.NewLocationAvailable += new EventHandler(locationManager_NewLocationAvailable);
            }
        }

        private void locationManager_NewLocationAvailable(object sender, EventArgs e)
        {
            RedetectNetwork();
        }

        #region Events

        public event EventHandler<NetworkSwitchedEventArgs> NetworkSwitched;

        public event EventHandler<RedetectNetworkStatusChangeEventArgs> RedetectNetworkStatusChanged;

        private void OnNetworkSwitched(NetworkConfiguration network)
        {
            if (NetworkSwitched != null)
                NetworkSwitched(this, new NetworkSwitchedEventArgs(network));
        }

        private void OnRedetectNetworkStatusChanged(NetworkChangeStatus status)
        {
            if (RedetectNetworkStatusChanged != null)
                RedetectNetworkStatusChanged(this, new RedetectNetworkStatusChangeEventArgs(status));
        }

        #endregion

        public bool AutomaticSwitching
        {
            get
            {
                return SettingsManager.Instance.GetApplicationSetting(SettingsManager.App_AutoSwitchKey, SettingsManager.Default_AutoSwitch);
            }
        }

        public Guid CurrentNetworkConfigurationId { get; private set; }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (AutomaticSwitching)
            {
                RedetectNetwork();
            }
        }

        private void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            if (!AutomaticSwitching)
                return;

            RedetectNetwork();
        }

        public void ActivateNetwork(Guid networkId)
        {
            var network = SettingsManager.Instance.GetNetworkConfiguration(networkId);
            ActivateNetworkConfiguration(network);
        }

        private void ActivateNetworkConfiguration(NetworkConfiguration nc)
        {
            if (CurrentNetworkConfigurationId == nc.Id)
                return;

            if (nc.Actions != null)
            {
                foreach (var actionId in nc.Actions)
                {
                    var action = this.addInManager.GetActionById(actionId);
                    if (action == null)
                        continue;

                    try
                    {
                        action.Activate(nc.Id, nc.Name);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(String.Format("Action '{0}' could not be activated: {1}", actionId.ToString(), ex.Message), ex);
                    }
                }
            }

            CurrentNetworkConfigurationId = nc.Id;
            OnNetworkSwitched(nc);
        }

        public void RedetectNetwork(bool forceRedetection = false)
        {
            if (forceRedetection)
                CurrentNetworkConfigurationId = Guid.Empty;

            System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object state)
            {
                InternalRedetectNetwork();
            }));
            //var t = Task.Factory.StartNew(() => InternalRedetectNetwork());            
            //t.Dispose();
        }

        private void InternalRedetectNetwork()
        {
            Logger.Log("RedetectNetwork called");

            OnRedetectNetworkStatusChanged(NetworkChangeStatus.Detecting);

            bool error = false;

            try
            {
                semaphore.WaitOne();

                bool atLeastOneFound = false;

                bool stopAfterFirstMatch = SettingsManager.Instance.GetApplicationSetting(SettingsManager.App_StopAfterFirstNetworkMatch, SettingsManager.Default_StopAfterFirstNetworkMatch);

                var allActiveInterfaces = NetworkInterface.GetAllNetworkInterfaces().Where(p => p.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up);

                foreach (var nc in SettingsManager.Instance.GetNetworkConfigurations(true))
                {
                    if (nc.IsUnknownNetwork)
                        continue;

                    #region Network
                    foreach (NetworkInterface ni in allActiveInterfaces)
                    {
                        if (nc.IsNetworkInterfaceDependend && nc.NetworkInterfaceId != ni.Id)
                            continue;

                        bool found = false;

                        switch (nc.Method)
                        {
                            case NetworkConfigurationMethod.DNSSuffix:
                                if (ni.GetIPProperties().DnsSuffix.ToUpper().Contains(nc.Query.ToUpper()))
                                {
                                    ActivateNetworkConfiguration(nc);
                                    found = true;
                                    atLeastOneFound = true;
                                }
                                break;
                            case NetworkConfigurationMethod.Gateway:
                                foreach (GatewayIPAddressInformation gi in ni.GetIPProperties().GatewayAddresses)
                                {
                                    if (gi.Address.ToString().ToUpper().Contains(nc.Query.ToUpper()))
                                    {
                                        ActivateNetworkConfiguration(nc);
                                        found = true;
                                        atLeastOneFound = true;
                                        break;
                                    }
                                }
                                break;
                            case NetworkConfigurationMethod.GatewayMAC:
                                foreach (GatewayIPAddressInformation gi in ni.GetIPProperties().GatewayAddresses)
                                {
                                    var mac = ARPRequest.GetMacAddress(gi.Address);
                                    if (mac.ToUpper().Contains(nc.Query.ToUpper()))
                                    {
                                        ActivateNetworkConfiguration(nc);
                                        found = true;
                                        atLeastOneFound = true;
                                        break;
                                    }
                                }
                                break;
                            case NetworkConfigurationMethod.WLANSSID:
                                string wlanSSID = WlanManager.GetCurrentSSID();
                                if (wlanSSID.ToUpper().Contains(nc.Query.ToUpper()))
                                {
                                    ActivateNetworkConfiguration(nc);
                                    found = true;
                                    atLeastOneFound = true;
                                }
                                break;
                            case NetworkConfigurationMethod.WLANAvailable:
                                if (WlanManager.IsSSIDAvailable(nc.Query))
                                {
                                    ActivateNetworkConfiguration(nc);
                                    found = true;
                                    atLeastOneFound = true;
                                }
                                break;
                            case NetworkConfigurationMethod.IP:
                                string hostName = Dns.GetHostName();
                                IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
                                foreach (var item in ipEntry.AddressList)
                                {
                                    if (item.ToString().ToUpper().Contains(nc.Query.ToUpper()))
                                    {
                                        ActivateNetworkConfiguration(nc);
                                        found = true;
                                        atLeastOneFound = true;
                                        break;
                                    }
                                }
                                break;
                            case NetworkConfigurationMethod.ServerAvailable:
                                try
                                {
                                    var repl = new Ping().Send(nc.Query, 5000);
                                    if (repl.Status == IPStatus.Success)
                                    {
                                        ActivateNetworkConfiguration(nc);
                                        found = true;
                                        atLeastOneFound = true;
                                    }
                                }
                                catch (PingException) { }
                                break;
                            default:
                                break;
                        }

                        if (found)
                            break;
                    }
                    #endregion

                    #region DockingState
                    // no network matched, try docking state
                    if (!atLeastOneFound)
                    {
                        if (nc.Method != NetworkConfigurationMethod.DockingStationState)
                            continue;

                        var val = RegistryHelper.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\IDConfigDB\CurrentDockInfo", "DockingState");
                        if (val != null)
                        {
                            //0 = Workstation or Server, 1 = Undocked Laptop, 2 = Docked Laptop
                            var dockState = val.ToString();
                            if (nc.Query == "1" && dockState == "2")
                            {
                                ActivateNetworkConfiguration(nc);
                                atLeastOneFound = true;
                            }
                        }
                    }
                    #endregion

                    #region Location
                    // no network matched, try location
                    if (!atLeastOneFound)
                    {
                        if (nc.Method != NetworkConfigurationMethod.Location)
                            continue;

                        if (locationManager == null)
                            continue;

                        var queryArray = nc.Query.Split(',');
                        var currentLocation = locationManager.GetCurrentLocation();
                        if (currentLocation == null)
                            continue;

                        int allQueries = queryArray.Length;
                        int queriesFound = 0;
                        foreach (var query in queryArray)
                        {
                            foreach (var locPart in currentLocation)
                            {
                                if (locPart.Trim().ToUpper().Contains(query.Trim().ToUpper()))
                                {
                                    queriesFound++;
                                    break;
                                }
                            }
                        }

                        if (allQueries == queriesFound)
                        {
                            ActivateNetworkConfiguration(nc);
                            atLeastOneFound = true;
                        }
                    }
                    #endregion

                    if (atLeastOneFound && stopAfterFirstMatch)
                        break;
                }

                // nothing found, activate "Unknown Network"
                if (!atLeastOneFound)
                {
                    var unknown = SettingsManager.Instance.GetNetworkConfiguration(SwitcherActionBase.DeactivateNetworkId);
                    ActivateNetworkConfiguration(unknown);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, ex);
                error = true;
            }
            finally
            {
                semaphore.Release();

                Logger.Log("RedetectNetwork completed");

                if (error)
                    OnRedetectNetworkStatusChanged(NetworkChangeStatus.Error);
                else
                    OnRedetectNetworkStatusChanged(NetworkChangeStatus.Completed);
            }
        }

        public static void GetPrePopulatedNetworkMethods(out string wlanSSID, out string gateway, out string gatewayMAC, out string dnsSuffix, out string ip, out string location)
        {
            wlanSSID = string.Empty;
            gateway = string.Empty;
            gatewayMAC = string.Empty;
            dnsSuffix = string.Empty;
            ip = string.Empty;
            location = string.Empty;

            wlanSSID = WlanManager.GetCurrentSSID();

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up)
                    continue;

                dnsSuffix = ni.GetIPProperties().DnsSuffix;

                foreach (GatewayIPAddressInformation gi in ni.GetIPProperties().GatewayAddresses)
                {
                    gateway = gi.Address.ToString();
                    gatewayMAC = ARPRequest.GetMacAddress(gi.Address);
                    break;
                }
            }

            string hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            foreach (var item in ipEntry.AddressList)
            {
                if (item.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ip = item.ToString();
                    break;
                }
            }

            if (Windows7Helper.IsWindows7)
            {
                location = LocationManager.Instance.GetCurrentLocationString();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            SystemEvents.PowerModeChanged -= powerModeChangedHandler;
            NetworkChange.NetworkAddressChanged -= networkChangedHandler;
            this.semaphore.Dispose();
            this.locationManager.Dispose();
        }

        #endregion

        public static NetworkInterface[] GetAllNetworkInterfaces()
        {
            return NetworkInterface.GetAllNetworkInterfaces();
        }
    }
}

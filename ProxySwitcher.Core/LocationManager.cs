using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Device.Location;
using System.Configuration;

namespace ProxySwitcher.Core
{
    public sealed class LocationManager : IDisposable
    {
        public event EventHandler NewLocationAvailable;

        private static readonly LocationManager instance = new LocationManager();
        private static bool initialized = false;
        private static object lockObject = new object();

        public static LocationManager Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (!initialized)
                        instance.Init();
                }
                return instance;
            }
        }


        private bool isAvailable;
        private GeoCoordinateWatcher watcher;
        private GeoCoordinate currentGeoLocation;
        private string[] cachedAddress;

        private LocationManager() { }

        private void Init()
        {
            initialized = true;

            if (!SettingsManager.Instance.GetApplicationSetting(SettingsManager.App_UseWin7LocationAPI, SettingsManager.Default_UseWin7LocationAPI))
                return;

            watcher = new GeoCoordinateWatcher();
            watcher.TryStart(false, TimeSpan.FromMilliseconds(1000));
            watcher.MovementThreshold = double.Parse(ConfigurationManager.AppSettings["Windows7LocationThreshold"]);
            watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);

            EnsureIfAvailable();
        }

        private void OnNewLocationAvailable()
        {
            if (NewLocationAvailable != null)
                NewLocationAvailable(this, EventArgs.Empty);
        }

        private void EnsureIfAvailable()
        {
            Logger.Log(String.Format("Location Watcher Permission: {0} Status: {1}", watcher.Permission.ToString(), watcher.Status.ToString()));

            if (watcher.Permission == GeoPositionPermission.Granted)
            {
                if (watcher.Status != GeoPositionStatus.Disabled)
                {
                    this.isAvailable = true;
                    return;
                }
            }

            this.isAvailable = false;
        }

        private void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            EnsureIfAvailable();
        }

        private void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            Logger.Log(String.Format("Position changed: Unknown: {2}, Lat: {0}, Long: {1}", e.Position.Location.Latitude.ToString(), e.Position.Location.Longitude.ToString(), e.Position.Location.IsUnknown));
            this.currentGeoLocation = e.Position.Location;

            OnNewLocationAvailable();
        }

        private string[] GetCurrentLocationString(GeoCoordinate geoCoordinate)
        {
            if (geoCoordinate == null)
                return null;

            if (currentGeoLocation == geoCoordinate && cachedAddress != null)
                return cachedAddress;

            CivicAddressResolver resolver = new CivicAddressResolver();
            var address = resolver.ResolveAddress(geoCoordinate);

            if (address.IsUnknown)
            {
                cachedAddress = null;
                return cachedAddress;
            }

            cachedAddress = BuildAddressString(address);
            return cachedAddress;
        }

        private static string[] BuildAddressString(CivicAddress civicAddress)
        {
            if (civicAddress.IsUnknown)
                return null;

            List<string> list = new List<string>();

            list.Add(civicAddress.AddressLine1);
            list.Add(civicAddress.AddressLine2);
            list.Add(civicAddress.Building);
            list.Add(civicAddress.City);
            list.Add(civicAddress.CountryRegion);
            list.Add(civicAddress.FloorLevel);
            list.Add(civicAddress.PostalCode);
            list.Add(civicAddress.StateProvince);

            return list.ToArray();
        }

        public string[] GetCurrentLocation()
        {
            if (!isAvailable)
                return null;

            return GetCurrentLocationString(this.currentGeoLocation);
        }

        public void Dispose()
        {
            if (watcher != null)
            {
                watcher.Stop();
                watcher.Dispose();
            }
        }

        public string GetCurrentLocationString()
        {
            try
            {
                if (!isAvailable)
                    return string.Empty;

                string[] location = GetCurrentLocation();
                if (location == null)
                    return string.Empty;

                return location.Aggregate((cur, next) => (cur + ", " + next));
            }
            catch (Exception ex)
            {
                Logger.Log("GetCurrentLocation failed", ex);
                return string.Empty;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxySwitcher.Common;
using System.Xml.Serialization;
using System.IO;
using ProxySwitcher.Core.Resources;
using System.Threading;

namespace ProxySwitcher.Core
{
    public class SettingsManager
    {
        #region Const

        public const string App_AutoSwitchKey = "AutoSwitchEnabled";
        public const string App_Language = "Language";
        public const string App_StartMinimized = "StartMinimized";
        public const string App_CheckForUpdates = "CheckForUpdates";
        public const string App_AutoStartWithWindows = "AutoStart";
        public const string App_UseWin7LocationAPI = "LocationAPI";
        public const string App_UseWin7Mode = "Win7Mode";
        public const string App_StopAfterFirstNetworkMatch = "StopAfterFirstNetworkMatch";
        public const string App_NoNewNetwork = "NoNewNetwork";

        public const int Default_Language = 1033;
        public const bool Default_AutoSwitch = true;
        public const bool Default_CheckForUpdates = true;
        public const bool Default_StartMinimized = false;
        public const bool Default_AutoStartWithWindows = false;
        public const bool Default_UseWin7LocationAPI = true;
        public const bool Default_UseWin7Mode = false;
        public const bool Default_StopAfterFirstNetworkMatch = false;
        public const bool Default_NoNewNetwork = false;

        private const string NetworkSettingsFileName = "Network";
        private const string ApplicationSettingsFileName = "Application";

        #endregion

        #region Static

        private static readonly SettingsManager instance = new SettingsManager();

        internal static string GetSettingsFolderLocal()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProxySwitcher\\V3");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        public static string GetSettingsFolder()
        {
            string path;
            if (!PolicyManager.GetSettingsFolderFromPolicy(out path))
            {
                path = GetSettingsFolderLocal();
            }
            return path;
        }

        public static string GetSettingsFile(string type)
        {
            return Path.Combine(GetSettingsFolder(), string.Format("Settings.{0}.xml", type));
        }

        private SettingsManager()
        {
            Load();

            try
            {
                settingsWatcher = new FileSystemWatcher(GetSettingsFolder(), "Settings.*.xml");
                settingsWatcher.Changed += new FileSystemEventHandler(settingsWatcher_Changed);
                settingsWatcher.NotifyFilter = NotifyFilters.LastWrite;
                settingsWatcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                Logger.Log("Settings file watcher could not be initialized", ex);
            }
        }

        public static SettingsManager Instance
        {
            get { return instance; }
        }

        #endregion

        private SettingsContainer<NetworkConfiguration> networkSettings;
        private SettingsContainer<string> applicationSettings;

        private FileSystemWatcher settingsWatcher;

        #region Helper

        private void settingsWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            settingsWatcher.EnableRaisingEvents = false;

            Thread.Sleep(2000);

            Load();

            if (e.Name.Contains(SettingsManager.NetworkSettingsFileName))
                OnNetworkConfigurationChangedEvent();
            else if (e.Name.Contains(SettingsManager.ApplicationSettingsFileName))
                OnApplicationConfigurationChangedEvent();
            else
                OnAddInConfigurationChangedEvent(false);

            settingsWatcher.EnableRaisingEvents = true;
        }

        private void CreateUnknownNetworkConfiguration()
        {
            NetworkConfiguration nothing = new NetworkConfiguration();
            nothing.Active = true;
            nothing.Id = SwitcherActionBase.DeactivateNetworkId;
            nothing.Name = LanguageResources.DeactivateNode_Name;
            this.SaveNetworkConfiguration(nothing);
        }

        #endregion

        #region Load/Save

        private void Load()
        {
            networkSettings = Deserialize<NetworkConfiguration>(SettingsManager.NetworkSettingsFileName);
            applicationSettings = Deserialize<string>(SettingsManager.ApplicationSettingsFileName);

            if (GetNetworkConfiguration(SwitcherActionBase.DeactivateNetworkId) == null)
            {
                CreateUnknownNetworkConfiguration();
            }

            VerifyConfiguration();
        }

        private void VerifyConfiguration()
        {
            var interfaces = NetworkManager.GetAllNetworkInterfaces();
            var networks = networkSettings.Keys.ToList();

            foreach (var key in networks)
            {
                var network = networkSettings[key];

                if (!network.IsNetworkInterfaceDependend)
                    continue;

                if (interfaces.FirstOrDefault(p => p.Id == network.NetworkInterfaceId) == null)
                {
                    var nic = interfaces.FirstOrDefault(p => p.Name == network.NetworkInterfaceName);
                    if (nic == null)
                        continue;
                    
                    network.NetworkInterfaceId = nic.Id;
                    this.SaveNetworkConfiguration(network);
                }
            }
        }

        private SettingsContainer<T> Deserialize<T>(string name) where T : class
        {
            try
            {
                SettingsContainer<T> returnData = new SettingsContainer<T>();
                XmlSerializer xml = new XmlSerializer(typeof(SettingsContainer<T>));
                using (FileStream fs = new FileStream(GetSettingsFile(name), FileMode.Open, FileAccess.Read))
                {
                    returnData = (SettingsContainer<T>)xml.Deserialize(fs);
                }
                return returnData;
            }
            catch
            {
                return new SettingsContainer<T>();
            }
        }

        private SettingsBag DeserializeAddInSettings(string name)
        {
            try
            {
                SettingsBag returnData = new SettingsBag();
                XmlSerializer xml = new XmlSerializer(typeof(SettingsBag));
                using (FileStream fs = new FileStream(GetSettingsFile(name), FileMode.Open, FileAccess.Read))
                {
                    returnData = (SettingsBag)xml.Deserialize(fs);
                }
                return returnData;
            }
            catch
            {
                return new SettingsBag();
            }
        }

        private void Serialize<T>(SettingsContainer<T> data, string name) where T : class
        {
            try
            {
                XmlSerializer xml = new XmlSerializer(typeof(SettingsContainer<T>));
                using (FileStream fs = new FileStream(GetSettingsFile(name), FileMode.Create))
                {
                    xml.Serialize(fs, data);
                }
            }
            catch (Exception ex) { Logger.Log("Serializing type " + typeof(T).ToString() + " failed.", ex); }
        }

        private void SerializeAddInSettings(SettingsBag data, string name)
        {
            try
            {
                XmlSerializer xml = new XmlSerializer(typeof(SettingsBag));
                using (FileStream fs = new FileStream(GetSettingsFile(name), FileMode.Create))
                {
                    xml.Serialize(fs, data);
                }
            }
            catch (Exception ex) { Logger.Log("Serializing AddIn settings for " + name, ex); }
        }

        private void Save()
        {
            if (settingsWatcher != null)
                settingsWatcher.EnableRaisingEvents = false;

            Serialize<NetworkConfiguration>(this.networkSettings, SettingsManager.NetworkSettingsFileName);
            Serialize<string>(this.applicationSettings, SettingsManager.ApplicationSettingsFileName);

            if (settingsWatcher != null)
                settingsWatcher.EnableRaisingEvents = true;
        }

        #endregion

        #region Network Settings

        public event EventHandler NetworkConfigurationChangedEvent;

        protected void OnNetworkConfigurationChangedEvent()
        {
            if (NetworkConfigurationChangedEvent != null)
                NetworkConfigurationChangedEvent(this, EventArgs.Empty);
        }

        public NetworkConfiguration GetNetworkConfiguration(Guid networkId)
        {
            if (!networkSettings.ContainsKey(networkId.ToString()))
                return null;

            return networkSettings[networkId.ToString()];
        }

        public void SaveNetworkConfiguration(NetworkConfiguration networkConfiguration)
        {
            networkSettings[networkConfiguration.Id.ToString()] = networkConfiguration;

            Save();

            OnNetworkConfigurationChangedEvent();
        }

        public void DeleteNetworkConfiguration(Guid id)
        {
            if (id == SwitcherActionBase.DeactivateNetworkId)
                return;

            networkSettings.Remove(id.ToString());

            Save();

            OnNetworkConfigurationChangedEvent();
        }

        public NetworkConfiguration[] GetNetworkConfigurations(bool onlyActive = false)
        {
            if (onlyActive)
                return networkSettings.Values.Where(p => p.Active).OrderBy(p => p.IsUnknownNetwork).ToArray();

            return networkSettings.Values.OrderBy(p => p.IsUnknownNetwork).ToArray();
        }

        #endregion

        #region Application Settings

        public event EventHandler ApplicationConfigurationChangedEvent;

        protected void OnApplicationConfigurationChangedEvent()
        {
            if (ApplicationConfigurationChangedEvent != null)
                ApplicationConfigurationChangedEvent(this, EventArgs.Empty);
        }

        public T GetApplicationSetting<T>(string key, T defaultValue)
        {
            try
            {
                string val = this.applicationSettings[key];
                if (String.IsNullOrWhiteSpace(val))
                    return defaultValue;

                var data = (T)Convert.ChangeType(val, typeof(T));
                return data;
            }
            catch { return defaultValue; }
        }

        public T GetApplicationSetting<T>(string key)
        {
            try
            {
                string val = this.applicationSettings[key];
                if (String.IsNullOrWhiteSpace(val))
                    return default(T);

                var data = (T)Convert.ChangeType(val, typeof(T));
                return data;
            }
            catch { return default(T); }
        }

        public string GetApplicationSetting(string key)
        {
            return this.applicationSettings[key];
        }

        public void SaveApplicationSetting(string key, string value)
        {
            applicationSettings[key] = value;

            Save();

            OnApplicationConfigurationChangedEvent();
        }

        public void SaveApplicationSettings(Dictionary<string, string> keyValues)
        {
            foreach (var item in keyValues)
            {
                applicationSettings[item.Key] = item.Value;
            }

            Save();

            OnApplicationConfigurationChangedEvent();
        }

        #endregion

        #region AddIn Settings

        public event EventHandler<AddInConfigurationChangedEventArgs> AddInConfigurationChangedEvent;

        protected void OnAddInConfigurationChangedEvent(bool userInitiated)
        {
            if (AddInConfigurationChangedEvent != null)
                AddInConfigurationChangedEvent(this, new AddInConfigurationChangedEventArgs(userInitiated));
        }

        internal SettingsBag LoadAddInSettings(Guid guid)
        {
            return DeserializeAddInSettings(guid.ToString());
        }

        internal void SaveAddInSettings(Guid guid, SettingsBag settingsBag)
        {
            settingsWatcher.EnableRaisingEvents = false;

            SerializeAddInSettings(settingsBag, guid.ToString());

            OnAddInConfigurationChangedEvent(true);

            settingsWatcher.EnableRaisingEvents = true;
        }

        #endregion

        public void MoveNetworkConfigurationAfter(NetworkConfiguration sourceConfig, NetworkConfiguration targetConfig)
        {
            if (sourceConfig == null || targetConfig == null)
                return;

            int newIndex = this.networkSettings.GetIndexOf(targetConfig.Id.ToString());
            this.networkSettings.MoveTo(sourceConfig.Id.ToString(), newIndex);

            Save();

            OnNetworkConfigurationChangedEvent();
        }
    }

    public class AddInConfigurationChangedEventArgs : EventArgs
    {
        public AddInConfigurationChangedEventArgs(bool userInitiated)
        {
            this.UserInitiated = userInitiated;
        }

        public bool UserInitiated { get; set; }
    }
}

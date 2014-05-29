using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using ProxySwitcher.Core.Resources;
using System.IO;

namespace ProxySwitcher.Core
{
    public static class PolicyManager
    {
        public static PSPolicy ValidatePolicies()
        {
            PSPolicy policy = new PSPolicy();

            bool onePolicyApplies = false;

            RegistryKey psRoot = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Policies\\ProxySwitcher");
            if (psRoot == null)
                return null;

            int isDisabled = (int)psRoot.GetValue("IsDisabled", 0);
            if (isDisabled == 1)
            {
                policy.IsDisabled = true;
                onePolicyApplies = true;
            }

            int isNetworkSettingsLocked = (int)psRoot.GetValue("NetworkSettingsLocked", 0);
            if (isNetworkSettingsLocked == 1)
            {
                policy.NetworkSettingsLocked = true;
                onePolicyApplies = true;
            }

            int isApplicationSettingsLocked = (int)psRoot.GetValue("ApplicationSettingsLocked", 0);
            if (isApplicationSettingsLocked == 1)
            {
                policy.ApplicationSettingsLocked = true;
                onePolicyApplies = true;
            }

            if (onePolicyApplies)
            {
                policy.Message = "Some settings are managed by your system administrator.";
                string url = (string)psRoot.GetValue("PolicyLink", string.Empty);
                if (!String.IsNullOrEmpty(url))
                    policy.MessageLink = url;
                else
                    policy.MessageLink = "http://projects2.mwiedemeyer.de/ProxySwitcher/SitePages/Policy.aspx";
            }

            return policy;
        }

        private static bool isCopied = false;

        internal static bool GetSettingsFolderFromPolicy(out string location)
        {
            location = string.Empty;

            RegistryKey psRoot = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Policies\\ProxySwitcher");
            if (psRoot == null)
                return false;

            string settingsLocation = (string)psRoot.GetValue("SettingsLocation", string.Empty);
            if (String.IsNullOrEmpty(settingsLocation))
                return false;

            location = Environment.ExpandEnvironmentVariables(settingsLocation);

            try
            {
                if (!Directory.Exists(location))
                    Directory.CreateDirectory(location);

                if (!Directory.Exists(location))
                    return false;

                if (!isCopied)
                {
                    isCopied = true;
                    CopySettingsToLocal(location);
                }

                return true;
            }
            catch { return false; }
        }

        private static void CopySettingsToLocal(string newLocation)
        {
            try
            {
                string localFolder = SettingsManager.GetSettingsFolderLocal();

                if (!Directory.Exists(localFolder))
                    Directory.CreateDirectory(localFolder);

                foreach (string item in Directory.GetFiles(newLocation, "*.*", SearchOption.TopDirectoryOnly))
                {
                    File.Copy(item, Path.Combine(localFolder, Path.GetFileName(item)), true);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Copy settings failed", ex);
            }
        }
    }
}

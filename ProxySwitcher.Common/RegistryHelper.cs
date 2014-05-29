using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace ProxySwitcher.Common
{
    public static class RegistryHelper
    {
        public static void SetStringValue(string key, string valueName, object value)
        {
            Registry.SetValue(key, valueName, value, RegistryValueKind.String);
        }

        public static void SetMultiStringValue(string key, string valueName, string[] values)
        {
            Registry.SetValue(key, valueName, values, RegistryValueKind.MultiString);
        }

        public static void SetDWordValue(string key, string valueName, object value)
        {
            Registry.SetValue(key, valueName, value, RegistryValueKind.DWord);
        }

        public static void SetBinaryValue(string key, string valueName, object value)
        {
            Registry.SetValue(key, valueName, value, RegistryValueKind.Binary);
        }

        public static object GetValue(string key, string valueName)
        {
            return Registry.GetValue(key, valueName, null);
        }

        public static string[] GetValuesInKey(string key)
        {
            RegistryKey regKey = null;
            if (key.StartsWith("HKEY_CLASSES_ROOT"))
                regKey = Registry.ClassesRoot;
            if (key.StartsWith("HKEY_CURRENT_USER"))
                regKey = Registry.CurrentUser;
            if (key.StartsWith("HKEY_LOCAL_MACHINE"))
                regKey = Registry.LocalMachine;
            if (key.StartsWith("HKEY_USERS"))
                regKey = Registry.Users;

            if (regKey == null)
                throw new ApplicationException("Invalid Registry Key");
            try
            {
                return regKey.OpenSubKey(key.Substring(key.IndexOf("\\") + 1), true).GetValueNames();
            }
            catch (Exception) { return null; }
        }

        public static void DeleteEntry(string key, string valueName)
        {
            RegistryKey regKey = null;
            if (key.StartsWith("HKEY_CLASSES_ROOT"))
                regKey = Registry.ClassesRoot;
            if (key.StartsWith("HKEY_CURRENT_USER"))
                regKey = Registry.CurrentUser;
            if (key.StartsWith("HKEY_LOCAL_MACHINE"))
                regKey = Registry.LocalMachine;
            if (key.StartsWith("HKEY_USERS"))
                regKey = Registry.Users;

            if (regKey == null)
                throw new ApplicationException("Invalid Registry Key");
            try
            {
                regKey.OpenSubKey(key.Substring(key.IndexOf("\\") + 1), true).DeleteValue(valueName);
            }
            catch (Exception) { }
        }
    }

}

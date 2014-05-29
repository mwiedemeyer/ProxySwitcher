using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ProxySwitcher.Common
{
    [Serializable]
    public sealed class SettingsBag
    {
        private List<string> keys = new List<string>();
        private List<string> values = new List<string>();

        public string[] Keys
        {
            get { return keys.ToArray(); }
            set { keys = new List<string>(value); }
        }

        public string[] Values
        {
            get { return values.ToArray(); }
            set { values = new List<string>(value); }
        }

        public string this[string key]
        {
            get
            {
                if (ContainsKey(key))
                    return values[keys.IndexOf(key)];
                return null;
            }
            set
            {
                if (keys.Contains(key))
                    values[keys.IndexOf(key)] = value;
                else
                {
                    keys.Add(key);
                    values.Add(value);
                }
            }
        }

        public bool ContainsKey(string key)
        {
            return (keys.IndexOf(key) >= 0);
        }

        public void Remove(string key)
        {
            if (!ContainsKey(key))
                return;

            values.RemoveAt(keys.IndexOf(key));
            keys.Remove(key);
        }
    }
}

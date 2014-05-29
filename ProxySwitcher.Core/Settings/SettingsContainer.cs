using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ProxySwitcher.Core
{
    [Serializable]
    public class SettingsContainer<T> where T : class
    {
        private List<string> keys = new List<string>();
        private List<T> values = new List<T>();

        public string[] Keys
        {
            get { return keys.ToArray(); }
            set { keys = new List<string>(value); }
        }

        public T[] Values
        {
            get { return values.ToArray(); }
            set { values = new List<T>(value); }
        }

        public T this[string key]
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

        public void MoveTo(string key, int newIndex)
        {
            int oldIndex = GetIndexOf(key);

            if (oldIndex == newIndex)
                return;

            T oldValue = Values.ElementAt(oldIndex);

            this.keys.Insert(newIndex, key);
            this.values.Insert(newIndex, oldValue);

            int removeIndex = 0;
            if (oldIndex > newIndex)
                removeIndex = oldIndex + 1;
            else
                removeIndex = oldIndex;

            this.keys.RemoveAt(removeIndex);
            this.values.RemoveAt(removeIndex);
        }

        public int GetIndexOf(string key)
        {
            return this.keys.IndexOf(key);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxySwitcher.Actions.ProxyBase
{
    [Serializable]
    public class ProxySettingWithScheme<T>
    {
        private List<ProxyScheme> storeScheme = new List<ProxyScheme>();
        private List<T> storeValue = new List<T>();

        public ProxyScheme[] Keys
        {
            get { return this.storeScheme.ToArray(); }
        }

        public T[] Values
        {
            get { return this.storeValue.ToArray(); }
        }

        public bool IsAllSet
        {
            get { return this.ContainsKey(ProxyScheme.All); }
        }

        public T this[string schemeKey]
        {
            get
            {
                return this[(ProxyScheme)Enum.Parse(typeof(ProxyScheme), schemeKey, true)];
            }
            set
            {
                this[(ProxyScheme)Enum.Parse(typeof(ProxyScheme), schemeKey, true)] = value;
            }
        }

        public T this[ProxyScheme scheme]
        {
            get
            {
                if (ContainsKey(scheme))
                    return storeValue[storeScheme.IndexOf(scheme)];
                return default(T);
            }
            set
            {
                if (storeScheme.Contains(scheme))
                    storeValue[storeScheme.IndexOf(scheme)] = value;
                else
                {
                    storeScheme.Add(scheme);
                    storeValue.Add(value);
                }
            }
        }

        public bool ContainsKey(ProxyScheme scheme)
        {
            return (storeScheme.IndexOf(scheme) >= 0);
        }

        public void Remove(ProxyScheme scheme)
        {
            if (!ContainsKey(scheme))
                return;

            storeValue.RemoveAt(storeScheme.IndexOf(scheme));
            storeScheme.Remove(scheme);
        }

        public ProxyScheme FirstScheme()
        {
            if (IsAllSet)
                return ProxyScheme.All;

            if (storeScheme.Count > 0)
                return storeScheme.First();

            return ProxyScheme.Unknown;
        }

        public T FirstEntry()
        {
            if (IsAllSet)
                return this[ProxyScheme.All];

            if (storeValue.Count > 0)
                return storeValue.First();

            return default(T);
        }

        public void RemoveAllExceptSchemeAll()
        {
            T oldValue = this[ProxyScheme.All];
            
            this.storeValue = new List<T>();
            this.storeScheme = new List<ProxyScheme>();

            this[ProxyScheme.All] = oldValue;
        }
    }
}

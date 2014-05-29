using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxySwitcher.Core;
using ProxySwitcher.Core.Resources;

namespace ProxySwitcher.UI
{
    public class NetworkConfigurationUIItem
    {
        private NetworkConfigurationMethod item;

        public NetworkConfigurationUIItem(NetworkConfigurationMethod item)
        {
            this.item = item;
        }

        public NetworkConfigurationMethod Method
        {
            get { return this.item; }
        }

        public override string ToString()
        {
            return LanguageResources.ResourceManager.GetString("NetworkConfigurationMethod_" + this.item.ToString());
        }
    }
}

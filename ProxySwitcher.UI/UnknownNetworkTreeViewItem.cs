using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxySwitcher.Core;
using ProxySwitcher.Common;
using ProxySwitcher.Core.Resources;

namespace ProxySwitcher.UI
{
    public class UnknownNetworkTreeViewItem : NetworkTreeViewItem
    {
        public UnknownNetworkTreeViewItem(NetworkConfiguration networkConfiguration, SwitcherActionBase[] actions)
            : base(networkConfiguration, actions)
        {
            this.Margin = new System.Windows.Thickness(0, 10, 0, 0);
            this.Header = LanguageResources.DeactivateNode_Name;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using ProxySwitcher.Common;

namespace ProxySwitcher.UI
{
    public class ActionTreeViewItem : TreeViewItem
    {
        public ActionTreeViewItem()
            : base()
        {
        }

        public ActionTreeViewItem(SwitcherActionBase action, NetworkTreeViewItem parentNetworkItem)
            : base()
        {
            if (action == null)
                throw new ArgumentNullException("action", "Action cannot be null");

            this.ActionId = action.Id;
            this.Header = action.Name;
            this.ParentNetworkItem = parentNetworkItem;
        }

        public Guid ActionId { get; set; }

        public NetworkTreeViewItem ParentNetworkItem { get; set; }

        public Guid NetworkId
        {
            get { return ParentNetworkItem.NetworkConfiguration.Id; }
        }

        public string NetworkName
        {
            get { return ParentNetworkItem.NetworkConfiguration.Name; }
        }
        
    }
}

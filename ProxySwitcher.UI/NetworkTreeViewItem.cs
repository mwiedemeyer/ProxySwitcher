using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using ProxySwitcher.Core;
using ProxySwitcher.Common;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Documents;

namespace ProxySwitcher.UI
{
    public class NetworkTreeViewItem : TreeViewItem
    {
        public NetworkTreeViewItem() : base() { }

        public NetworkTreeViewItem(NetworkConfiguration networkConfiguration, SwitcherActionBase[] actions)
            : base()
        {
            this.NetworkConfiguration = networkConfiguration;
            this.Header = CreateHeader();

            if (actions != null)
            {
                foreach (var item in actions)
                {
                    this.Items.Add(new ActionTreeViewItem(item, this));
                }
            }

            if (!this.NetworkConfiguration.Active)
                this.FontStyle = FontStyles.Italic;
        }

        private object CreateHeader()
        {
            StackPanel pan = new StackPanel();

            string blank = string.Empty;
            if (!String.IsNullOrEmpty(this.NetworkConfiguration.IconPath) && System.IO.File.Exists(this.NetworkConfiguration.IconPath))
            {
                pan.Orientation = Orientation.Horizontal;
                Image image = new Image();
                image.Height = 16;
                image.Source = new BitmapImage(new Uri(this.NetworkConfiguration.IconPath));
                pan.Children.Add(image);
                blank = "  ";
            }

            pan.Children.Add(new TextBlock(new Run(blank + this.NetworkConfiguration.Name)));
            return pan;
        }

        public NetworkConfiguration NetworkConfiguration { get; set; }

        public Guid NetworkId
        {
            get { return this.NetworkConfiguration.Id; }
        }

        public bool IsActive
        {
            get
            {
                if (this.NetworkConfiguration == null)
                    return false;

                return this.NetworkConfiguration.Active;
            }
        }

        public void SelectAction(Guid actionId)
        {
            this.ExpandSubtree();

            foreach (ActionTreeViewItem item in this.Items)
            {
                if (item.ActionId == actionId)
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }
    }
}

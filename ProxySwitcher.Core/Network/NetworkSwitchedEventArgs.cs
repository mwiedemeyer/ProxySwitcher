using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxySwitcher.Core
{
    public class NetworkSwitchedEventArgs : EventArgs
    {
        public NetworkSwitchedEventArgs(NetworkConfiguration network)
            : base()
        {
            this.Network = network;
        }

        public NetworkConfiguration Network { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxySwitcher.Core
{    
    public class RedetectNetworkStatusChangeEventArgs : EventArgs
    {
        public RedetectNetworkStatusChangeEventArgs(NetworkChangeStatus status)
        {
            Status = status;
        }

        public NetworkChangeStatus Status { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxySwitcher.Core
{
    public enum NetworkChangeStatus : int
    {
        Unknown = 0,
        Detecting = 1,
        Completed = 2,
        Error = 3
    }
}

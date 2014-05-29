using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxySwitcher.Actions.ProxyBase
{
    public enum ProxyScheme : int
    {
        Unknown = 0,
        All = 1,
        HTTP = 2,
        HTTPS = 3,
        FTP = 4,
        SOCKS = 5
    }
}

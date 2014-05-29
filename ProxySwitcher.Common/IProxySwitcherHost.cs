using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxySwitcher.Common
{
    public interface IProxySwitcherHost
    {
        void SetStatusText(SwitcherActionBase ownerAction, string message, bool isError = false);
    }
}

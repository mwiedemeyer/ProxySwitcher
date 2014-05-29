using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChangeHomepageAction
{
    [Flags]
    internal enum Browsers : int
    {
        Unknown = 0,
        IE = 1,
        Opera = 2,
        Firefox = 4
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace ProxySwitcher.Common
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SwitcherActionAddInAttribute : ExportAttribute
    {
        public SwitcherActionAddInAttribute()
            : base(typeof(SwitcherActionBase))
        {
        }
    }
}

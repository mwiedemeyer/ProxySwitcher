using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace ProxySwitcher.Common
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class SwitcherActionMetaDataAttribute : ExportAttribute
    {
        public SwitcherActionMetaDataAttribute()
            : base(typeof(SwitcherActionBase))
        {
        }
    }
}

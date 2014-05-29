using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxySwitcher.Core
{
    [Serializable]
    public class PSPolicy
    {
        public bool IsDisabled { get; set; }

        public bool NetworkSettingsLocked { get; set; }

        public bool ApplicationSettingsLocked { get; set; }

        public bool HasMessage
        {
            get { return !String.IsNullOrWhiteSpace(Message); }
        }

        public string Message { get; set; }

        public string MessageLink { get; set; }
    }
}

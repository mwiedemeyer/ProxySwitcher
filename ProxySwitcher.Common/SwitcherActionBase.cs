using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Xml.Serialization;

namespace ProxySwitcher.Common
{
    public abstract class SwitcherActionBase
    {
        public static readonly Guid DeactivateNetworkId = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAA1");

        private SettingsBag settings = new SettingsBag();

        protected SwitcherActionBase() { }

        public event EventHandler SettingsChanged;

        public SettingsBag Settings
        {
            get { return this.settings; }
            set { this.settings = value; }
        }

        public IProxySwitcherHost HostApplication { get; set; }

        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract Stream IconResourceStream { get; }

        public abstract Guid Id { get; }

        public abstract string Group { get; }

        public abstract void Activate(Guid networkId, string networkName);

        public virtual UserControl GetWindowControl(Guid networkId, string networkName)
        {
            return null;
        }

        public void OnSettingsChanged()
        {
            if (SettingsChanged != null)
                SettingsChanged(this, EventArgs.Empty);
        }
    }
}

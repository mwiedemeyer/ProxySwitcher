using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxySwitcher.Common;
using ProxySwitcher.Actions.DefaultActions.ChangePrinter;
using System.Windows.Controls;
using System.IO;
using System.Reflection;

namespace ProxySwitcher.Actions.DefaultActions
{
    [SwitcherActionAddIn]
    public class ChangePrinterAction : SwitcherActionBase
    {
        public override string Name
        {
            get { return DefaultResources.ChangePrinter_Name; }
        }

        public override string Group
        {
            get { return "Printer"; }
        }

        public override string Description
        {
            get { return string.Empty; }
        }

        public override Stream IconResourceStream
        {
            get { return Assembly.GetExecutingAssembly().GetManifestResourceStream("ProxySwitcher.Actions.DefaultActions.Images.printer.png"); }
        }

        public override UserControl GetWindowControl(Guid networkId, string networkName)
        {
            return new ChangePrinterConfig(networkId, this);
        }

        public override Guid Id
        {
            get { return new Guid("D823AE0A-C772-41C8-9672-6F4EC862025A"); }
        }

        public override void Activate(Guid networkId, string networkName)
        {
            string printer = Settings[networkId + "_PrinterName"];
            if (String.IsNullOrWhiteSpace(printer))
                return;

            PrinterManager.SetNewDefaultPrinter(printer);
        }

        internal void SavePrinter(Guid networkId, string printer)
        {
            this.Settings[networkId + "_PrinterName"] = printer;

            this.OnSettingsChanged();

            if (HostApplication != null)
                HostApplication.SetStatusText(this, DefaultResources.Saved_Status);
        }
    }
}

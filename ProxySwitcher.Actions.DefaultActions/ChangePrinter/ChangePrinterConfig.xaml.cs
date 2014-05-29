using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProxySwitcher.Actions.DefaultActions.ChangePrinter
{
    /// <summary>
    /// Interaction logic for ChangePrinterConfig.xaml
    /// </summary>
    public partial class ChangePrinterConfig : UserControl
    {
        private Guid networkId;
        private ChangePrinterAction changePrinterAction;

        public ChangePrinterConfig()
        {
            InitializeComponent();
        }

        public ChangePrinterConfig(Guid networkId, ChangePrinterAction changePrinterAction)
            : this()
        {
            this.networkId = networkId;
            this.changePrinterAction = changePrinterAction;

            InitUI();
        }

        private void InitUI()
        {
            PopulateComboBox(true);

            string printer = this.changePrinterAction.Settings[this.networkId + "_PrinterName"];
            if (String.IsNullOrWhiteSpace(printer))
            {
                if (comboBoxPrinter.Items.Count > 0)
                    comboBoxPrinter.SelectedIndex = 0;
                return;
            }

            comboBoxPrinter.SelectedItem = printer;
        }

        private void PopulateComboBox(bool fromCache)
        {
            comboBoxPrinter.Items.Clear();

            foreach (var item in PrinterManager.GetAllPrinters(fromCache))
            {
                comboBoxPrinter.Items.Add(item);
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxPrinter.SelectedItem != null)
                this.changePrinterAction.SavePrinter(this.networkId, comboBoxPrinter.SelectedItem.ToString());
            else
                this.changePrinterAction.SavePrinter(this.networkId, string.Empty);
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            PopulateComboBox(false);
        }
    }
}

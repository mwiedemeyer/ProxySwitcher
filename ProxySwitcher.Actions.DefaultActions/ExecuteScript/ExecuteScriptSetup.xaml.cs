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
using Microsoft.Win32;

namespace ProxySwitcher.Actions.DefaultActions.ExecuteScript
{
    /// <summary>
    /// Interaction logic for ExecuteScriptSetup.xaml
    /// </summary>
    public partial class ExecuteScriptSetup : UserControl
    {
        private Guid networkId;
        private string networkName;
        private ExecuteScriptAction executeScriptAction;

        public ExecuteScriptSetup()
        {
            InitializeComponent();
        }

        public ExecuteScriptSetup(ExecuteScriptAction executeScriptAction, Guid networkId, string networkName, string script, bool withParameter, bool withParameterNameInsteadOfId, bool runAsAdmin)
            : this()
        {
            this.executeScriptAction = executeScriptAction;
            this.networkId = networkId;
            this.networkName = networkName;

            textBoxScript.Text = script;
            checkBoxWithParameter.IsChecked = withParameter;
            checkBoxWithParameterName.IsChecked = withParameterNameInsteadOfId;
            checkBoxRunAsAdmin.IsChecked = runAsAdmin;

            UpdateExampleText();
        }

        public ExecuteScriptSetup(ExecuteScriptAction executeScriptAction, Guid networkId, string networkName)
            : this()
        {
            this.executeScriptAction = executeScriptAction;
            this.networkId = networkId;
            this.networkName = networkName;

            UpdateExampleText();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            this.executeScriptAction.Save(this.networkId, textBoxScript.Text, checkBoxWithParameter.IsChecked.Value, checkBoxWithParameterName.IsChecked.Value, checkBoxRunAsAdmin.IsChecked.Value);
        }

        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.AddExtension = false;
            fd.CheckFileExists = true;
            fd.CheckPathExists = true;
            fd.Filter = "*.*|*.*";
            fd.Multiselect = false;
            fd.ShowReadOnly = false;
            fd.Title = "";
            fd.ShowDialog();

            if (!String.IsNullOrWhiteSpace(fd.FileName))
                textBoxScript.Text = fd.FileName;
        }

        private void UpdateExampleText()
        {
            string example = string.Empty;

            if (checkBoxWithParameter.IsChecked.Value)
            {
                example = this.networkId.ToString();
                if (checkBoxWithParameterName.IsChecked.Value)
                    example = this.networkName;
            }

            labelExample.Content = String.Format(DefaultResources.ExecuteScript_ExampleText, example);
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateExampleText();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxySwitcher.Common;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;

namespace ProxySwitcher.Actions.DefaultActions
{
    [SwitcherActionAddIn]
    public class ExecuteScriptAction : SwitcherActionBase
    {
        public override string Name
        {
            get { return DefaultResources.ExecuteScript_Name; }
        }

        public override string Group
        {
            get { return "Script"; }
        }

        public override string Description
        {
            get { return string.Empty; }
        }

        public override Stream IconResourceStream
        {
            get { return null; }
        }

        public override Guid Id
        {
            get { return new Guid("38F54865-A9E8-4787-87BF-DCB60A26F863"); }
        }

        public override UserControl GetWindowControl(Guid networkId, string networkName)
        {
            string script = this.Settings[networkId.ToString() + "_ScriptPath"];
            if (String.IsNullOrEmpty(script))
            {
                return new ExecuteScript.ExecuteScriptSetup(this, networkId, networkName);
            }
            else
            {
                bool withParameter = false;
                bool.TryParse(this.Settings[networkId.ToString() + "_ScriptWithParameter"], out withParameter);

                bool withParameterNameInsteadOfId = false;
                bool.TryParse(this.Settings[networkId.ToString() + "_ScriptWithParameterName"], out withParameterNameInsteadOfId);

                bool asAdmin = false;
                bool.TryParse(this.Settings[networkId.ToString() + "_ScriptAsAdmin"], out asAdmin);

                return new ExecuteScript.ExecuteScriptSetup(this, networkId, networkName, script, withParameter, withParameterNameInsteadOfId, asAdmin);
            }
        }

        internal void Save(Guid networkId, string script, bool withParameter, bool withParameterNameInsteadOfId, bool runAsAdmin)
        {
            this.Settings[networkId.ToString() + "_ScriptPath"] = script;
            this.Settings[networkId.ToString() + "_ScriptWithParameter"] = withParameter.ToString();
            this.Settings[networkId.ToString() + "_ScriptWithParameterName"] = withParameterNameInsteadOfId.ToString();
            this.Settings[networkId.ToString() + "_ScriptAsAdmin"] = runAsAdmin.ToString();

            OnSettingsChanged();

            if (HostApplication != null)
                HostApplication.SetStatusText(this, DefaultResources.Saved_Status);
        }

        public override void Activate(Guid networkId, string networkName)
        {
            string script = this.Settings[networkId.ToString() + "_ScriptPath"];
            if (String.IsNullOrEmpty(script))
                return;

            bool withParameter = false;
            bool.TryParse(this.Settings[networkId.ToString() + "_ScriptWithParameter"], out withParameter);

            bool asAdmin = false;
            bool.TryParse(this.Settings[networkId.ToString() + "_ScriptAsAdmin"], out asAdmin);

            ProcessStartInfo startInfo = new ProcessStartInfo(Environment.ExpandEnvironmentVariables(script));

            if (withParameter)
            {
                bool withNetworkNameInsteadofId = false;
                bool.TryParse(this.Settings[networkId.ToString() + "_ScriptWithParameterName"], out withNetworkNameInsteadofId);

                if (withNetworkNameInsteadofId)
                    startInfo.Arguments = networkName;
                else
                    startInfo.Arguments = networkId.ToString();
            }

            if (asAdmin)
                startInfo.Verb = "runas";

            Process.Start(startInfo);
        }
    }
}

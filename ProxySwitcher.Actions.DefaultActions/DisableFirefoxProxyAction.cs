using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxySwitcher.Common;
using System.IO;
using System.Reflection;
using System.Windows.Controls;

namespace ProxySwitcher.Actions.DefaultActions
{
    [SwitcherActionAddIn]
    public class DisableFirefoxProxyAction : SwitcherActionBase
    {
        public override string Name
        {
            get { return DefaultResources.DisableFirefox; }
        }

        public override string Group
        {
            get { return "Firefox"; }
        }

        public override string Description
        {
            get { return string.Empty; }
        }

        public override Stream IconResourceStream
        {
            get { return Assembly.GetExecutingAssembly().GetManifestResourceStream("ProxySwitcher.Actions.DefaultActions.Images.firefox.png"); }
        }

        public override Guid Id
        {
            get { return new Guid("2959D741-EFEE-48A0-88ED-7CE687A06641"); }
        }

        public override UserControl GetWindowControl(Guid networkId, string networkName)
        {
            return new Firefox.DisableFirefoxConfig(this, networkId);
        }

        public void SaveData(Guid networkId, string profileToSwitch, string profileFolder)
        {
            Settings[networkId.ToString() + "_ProfileToSwitch"] = profileToSwitch;
            Settings[networkId.ToString() + "_ProfileFolder"] = profileFolder;

            OnSettingsChanged();
        }

        public string GetProfileToSwitch(Guid networkId)
        {
            if (this.Settings.ContainsKey(networkId.ToString() + "_ProfileToSwitch"))
                return Settings[networkId.ToString() + "_ProfileToSwitch"].ToString();
            return String.Empty;
        }

        public string GetProfileFolder(Guid networkId)
        {
            if (Settings.ContainsKey(networkId.ToString() + "_ProfileFolder"))
                return Settings[networkId.ToString() + "_ProfileFolder"].ToString();
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles");
        }

        public List<string> GetAllProfiles(Guid networkId)
        {
            try
            {
                string path = GetProfileFolder(networkId);
                List<string> lst = new List<string>();
                foreach (string dir in Directory.GetDirectories(path))
                {
                    lst.Add(new DirectoryInfo(dir).Name);
                }
                return lst;
            }
            catch { return new List<string>(); }
        }

        public override void Activate(Guid networkId, string networkName)
        {
            string file = GetProfileFolder(networkId);
            // all
            if (String.IsNullOrEmpty(GetProfileToSwitch(networkId)))
            {
                foreach (string profPath in GetAllProfiles(networkId))
                {
                    file = Path.Combine(file, profPath);
                    file = Path.Combine(file, "prefs.js");
                    DisableProxy(file);
                }
            }
            else
            {
                file = Path.Combine(file, GetProfileToSwitch(networkId));
                file = Path.Combine(file, "prefs.js");
                DisableProxy(file);
            }
        }

        private void DisableProxy(string file)
        {
            string content = "";
            using (StreamReader sr = new StreamReader(file))
            {
                content = sr.ReadToEnd();
            }

            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                using (StringReader sr = new StringReader(content))
                {
                    while (true)
                    {
                        string line = sr.ReadLine();
                        if (line == null)
                            break;
                        if (line.StartsWith("user_pref(\"network.proxy.type"))
                            continue;

                        sw.WriteLine(line);
                    }
                }

                sw.WriteLine("user_pref(\"network.proxy.type\", 0);");
            }

            using (StreamWriter sw = new StreamWriter(file))
            {
                sw.Write(sb.ToString());
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxySwitcher.Common;
using System.Reflection;
using System.Windows.Controls;
using System.IO;

namespace ChangeHomepageAction
{
    [SwitcherActionAddIn]
    public class ChangeHomepageAction : SwitcherActionBase
    {
        public override string Name
        {
            get { return DefaultResources.ChangeHomepageAction_Name; }
        }

        public override string Description
        {
            get { return string.Empty; }
        }

        public override System.IO.Stream IconResourceStream
        {
            get { return Assembly.GetExecutingAssembly().GetManifestResourceStream("ChangeHomepageAction.Homepage.png"); }
        }

        public override Guid Id
        {
            get { return new Guid("76C892C1-D05B-4EED-AC6A-C411385EDCAE"); }
        }

        public override string Group
        {
            get { return "Homepage"; }
        }

        public override void Activate(Guid networkId, string networkName)
        {
            string homepageUrl = Settings[networkId + "_Homepage"];
            if (String.IsNullOrWhiteSpace(homepageUrl))
                return;

            Browsers browsers = (Browsers)Enum.Parse(typeof(Browsers), this.Settings[networkId + "_Browsers"], true);

            if (browsers.HasFlag(Browsers.IE))
            {
                try
                {
                    if (!homepageUrl.Contains("|"))
                    {
                        RegistryHelper.SetStringValue(@"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main", "Start Page", homepageUrl);
                        RegistryHelper.DeleteEntry(@"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main", "Secondary Start Pages");
                    }
                    else
                    {
                        string[] urls = homepageUrl.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        // Set first
                        RegistryHelper.SetStringValue(@"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main", "Start Page", urls[0]);

                        var list = urls.ToList();
                        list.RemoveAt(0);
                        //RegistryHelper.SetMultiStringValue(@"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main", "Secondary Start Pages", list.ToArray());
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main", "Secondary Start Pages", list.ToArray(), Microsoft.Win32.RegistryValueKind.MultiString);
                    }
                }
                catch { }
            }
            if (browsers.HasFlag(Browsers.Opera))
            {
                try
                {
                    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Opera\\Opera");
                    path = Path.Combine(path, "operaprefs.ini");
                    IniHelper helper = new IniHelper(path);
                    helper.SetValue("User Prefs", "Home URL", homepageUrl);
                    helper.Save();
                }
                catch { }
            }
            if (browsers.HasFlag(Browsers.Firefox))
            {
                try
                {
                    SetForAllFirefoxProfiles(homepageUrl);
                }
                catch { }
            }
        }

        public override UserControl GetWindowControl(Guid networkId, string networkName)
        {
            return new ChangeHomepageConfig(networkId, this);
        }

        internal void Save(Guid networkId, string homepageUrl, Browsers browsers)
        {
            this.Settings[networkId + "_Homepage"] = homepageUrl;
            this.Settings[networkId + "_Browsers"] = browsers.ToString();

            this.OnSettingsChanged();

            if (HostApplication != null)
                HostApplication.SetStatusText(this, DefaultResources.Saved_Status);
        }

        #region Firefox

        private void SetForAllFirefoxProfiles(string homepageUrl)
        {
            string file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles");
            foreach (string profPath in GetAllProfiles())
            {
                file = Path.Combine(file, profPath);
                file = Path.Combine(file, "prefs.js");

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
                            if (line.StartsWith("user_pref(\"browser.startup.homepage\""))
                                continue;

                            sw.WriteLine(line);
                        }
                    }

                    sw.WriteLine("user_pref(\"browser.startup.homepage\", \"{0}\");", homepageUrl);
                }

                using (StreamWriter sw = new StreamWriter(file))
                {
                    sw.Write(sb.ToString());
                }
            }
        }

        private List<string> GetAllProfiles()
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles");
                List<string> lst = new List<string>();
                foreach (string dir in Directory.GetDirectories(path))
                {
                    lst.Add(new DirectoryInfo(dir).Name);
                }
                return lst;
            }
            catch { return new List<string>(); }
        }

        #endregion
    }
}

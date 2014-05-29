using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using ProxySwitcher.Common;
using System.ComponentModel.Composition;
using System.Collections.ObjectModel;

namespace ProxySwitcher.Core
{
    public sealed class AddInManager
    {
        private string addinsDir = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Addins");

        public IProxySwitcherHost HostApplication { get; set; }

        [ImportMany(typeof(SwitcherActionBase))]
        private List<SwitcherActionBase> actions = null;

        public AddInManager(IProxySwitcherHost hostApplication)
        {
            HostApplication = hostApplication;
            this.LoadAddIns();
        }

        public SwitcherActionBase[] Actions
        {
            get
            {
                return (from p in this.actions
                        orderby p.Group, p.Name
                        select p).ToArray();
            }
        }

        private void LoadAddIns()
        {
            try
            {
                // Add any DLL's in the addins folder (if it exists)
                if (!Directory.Exists(addinsDir))
                {
                    Directory.CreateDirectory(addinsDir);
                }

                using (var catalog = new DirectoryCatalog(addinsDir, "*.dll"))
                {
                    using (var container = new CompositionContainer(catalog))
                    {
                        container.ComposeParts(this);
                    }
                }

                LoadAddInSettings();
            }
            catch (Exception ex)
            {
                Logger.Log("Error loading addin(s)", ex);
                if (ex is System.Reflection.ReflectionTypeLoadException)
                {
                    var loaderEx = ex as System.Reflection.ReflectionTypeLoadException;
                    foreach (var item in loaderEx.LoaderExceptions)
                    {
                        Logger.Log("Loader: " + item.Message, item);
                    }
                }
                throw new AddInLoaderException("Error loading addins.", ex);
            }
        }

        private void LoadAddInSettings()
        {
            foreach (var item in this.actions)
            {
                item.HostApplication = HostApplication;

                try
                {
                    item.SettingsChanged += new EventHandler(AddIn_SettingsChanged);
                    item.Settings = SettingsManager.Instance.LoadAddInSettings(item.Id);
                }
                catch (Exception ex) { Logger.Log("Loading AddIn settings failed", ex); }
            }
        }

        public void ReloadAddInSettings()
        {
            foreach (var item in this.actions)
            {
                try
                {
                    item.Settings = SettingsManager.Instance.LoadAddInSettings(item.Id);
                }
                catch (Exception ex) { Logger.Log("Reloading AddIn settings failed", ex); }
            }
        }

        void AddIn_SettingsChanged(object sender, EventArgs e)
        {
            var action = sender as SwitcherActionBase;
            if (action == null)
                return;

            SaveAddInSettings(action.Id);
        }

        public void SaveAddInSettings(Guid id)
        {
            var action = actions.FirstOrDefault(p => p.Id == id);
            try
            {
                SettingsManager.Instance.SaveAddInSettings(id, action.Settings);
            }
            catch (Exception ex) { Logger.Log("Save AddIn settings failed", ex); }
        }

        public SwitcherActionBase GetActionById(Guid id)
        {
            return this.actions.FirstOrDefault(p => p.Id == id);
        }

        public SwitcherActionBase[] GetActionsByIds(Guid[] guids)
        {
            if (guids == null)
                return new SwitcherActionBase[0];

            return this.actions.Where(p => guids.Contains(p.Id)).ToArray();
        }
    }
}

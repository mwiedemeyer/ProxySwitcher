using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shell;
using System.Windows;
using ProxySwitcher.Core.Resources;
using System.Windows.Media.Imaging;

namespace ProxySwitcher.Core
{
    public class Windows7Helper
    {
        private Application currentApp;

        public static bool IsWindows7
        {
            get
            {
                if (Environment.OSVersion.Version.Major > 6 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 1))
                    return true;

                return false;
            }
        }

        public Windows7Helper(Application app)
        {
            this.currentApp = app;
        }

        public void ClearJumpList()
        {
            JumpList jumpList = JumpList.GetJumpList(this.currentApp);
            if (jumpList == null)
                return;

            jumpList.JumpItems.Clear();
            jumpList.Apply();
        }

        public void AddJumpTask(NetworkConfiguration nc)
        {
            JumpTask jumpTask1 = new JumpTask();

            string appPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

            jumpTask1.ApplicationPath = appPath;
            jumpTask1.IconResourcePath = appPath;
            jumpTask1.Arguments = "/activate " + nc.Id.ToString();
            jumpTask1.Title = nc.Name;
            jumpTask1.Description = string.Empty;
            jumpTask1.CustomCategory = LanguageResources.Networks_Label;

            JumpList jumpList = JumpList.GetJumpList(this.currentApp);
            if (jumpList == null)
            {
                jumpList = new JumpList();
                JumpList.SetJumpList(this.currentApp, jumpList);
            }

            jumpList.JumpItems.Add(jumpTask1);
        }

        public void ApplyUpdates()
        {
            JumpList jumpList = JumpList.GetJumpList(this.currentApp);
            if (jumpList == null)
                return;

            jumpList.Apply();
        }

        public void RemoveOverlayIcon()
        {
            SetOverlayIcon(null);
        }

        public void SetOverlayIcon(string imagePath)
        {
            if (String.IsNullOrEmpty(imagePath) || !System.IO.File.Exists(imagePath))
                this.currentApp.MainWindow.TaskbarItemInfo.Overlay = null;
            else
                this.currentApp.MainWindow.TaskbarItemInfo.Overlay = new BitmapImage(new Uri(imagePath));
        }
    }
}

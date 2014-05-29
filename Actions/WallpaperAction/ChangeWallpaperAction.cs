using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using ProxySwitcher.Common;
using System.IO;
using System.Reflection;
using System.Windows.Controls;

namespace WallpaperAction
{
    [SwitcherActionAddIn]
    public class ChangeWallpaperAction : SwitcherActionBase
    {
        public override string Name
        {
            get { return DefaultResources.ChangeWallpaper_Name; }
        }

        public override string Description
        {
            get { return string.Empty; }
        }

        public override Stream IconResourceStream
        {
            get { return Assembly.GetExecutingAssembly().GetManifestResourceStream("WallpaperAction.wallpaper.png"); }
        }

        public override Guid Id
        {
            get { return new Guid("D2018EFE-CD59-4D29-BAE8-263F25A0BCB0"); }
        }

        public override string Group
        {
            get { return "Wallpaper"; }
        }

        public override void Activate(Guid networkId, string networkName)
        {
            string wallpaperFile = Settings[networkId + "_WallpaperFile"];
            if (String.IsNullOrWhiteSpace(wallpaperFile))
                return;

            string style = Settings[networkId + "_WallpaperStyle"];
            var wpStyle = (WallpaperStyle)Enum.Parse(typeof(WallpaperStyle), style, true);

            Wallpaper.SetWallpaper(wallpaperFile, wpStyle);
        }

        public override UserControl GetWindowControl(Guid networkId, string networkName)
        {
            return new ChangeWallpaperConfig(networkId, this);
        }

        internal void SaveWallpaperFile(Guid networkId, string wallpaperFile, WallpaperStyle style)
        {
            this.Settings[networkId + "_WallpaperFile"] = wallpaperFile;
            this.Settings[networkId + "_WallpaperStyle"] = style.ToString();

            this.OnSettingsChanged();
        }
    }

    internal enum WallpaperStyle : int
    {
        Tile, Center, Stretch, Fill, Fit
    }

    internal class Wallpaper
    {
        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public static void SetWallpaper(string filename, WallpaperStyle style)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", true);

            switch (style)
            {
                case WallpaperStyle.Tile:
                    key.SetValue(@"WallpaperStyle", "0");
                    key.SetValue(@"TileWallpaper", "1");
                    break;
                case WallpaperStyle.Center:
                    key.SetValue(@"WallpaperStyle", "0");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
                case WallpaperStyle.Stretch:
                    key.SetValue(@"WallpaperStyle", "2");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
                case WallpaperStyle.Fit:
                    key.SetValue(@"WallpaperStyle", "6");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
                case WallpaperStyle.Fill:
                    key.SetValue(@"WallpaperStyle", "10");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, filename, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }
}

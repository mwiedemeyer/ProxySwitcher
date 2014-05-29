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

namespace WallpaperAction
{
    /// <summary>
    /// Interaction logic for ChangeWallpaperConfig.xaml
    /// </summary>
    public partial class ChangeWallpaperConfig : UserControl
    {
        private Guid networkId;
        private ChangeWallpaperAction changeWallpaperAction;

        public ChangeWallpaperConfig()
        {
            InitializeComponent();
        }

        public ChangeWallpaperConfig(Guid networkId, ChangeWallpaperAction changeWallpaperAction)
            : this()
        {
            this.networkId = networkId;
            this.changeWallpaperAction = changeWallpaperAction;

            InitUI();
        }

        private void InitUI()
        {
            textBoxWallpaperFile.Text = this.changeWallpaperAction.Settings[this.networkId + "_WallpaperFile"];
            comboBoxWallpaperStyle.SelectedIndex = GetSelectedIndex(this.changeWallpaperAction.Settings[this.networkId + "_WallpaperStyle"]);
        }

        private int GetSelectedIndex(string style)
        {
            for (int i = 0; i < comboBoxWallpaperStyle.Items.Count; i++)
            {
                var m = comboBoxWallpaperStyle.Items[i] as ComboBoxItem;
                if (m.Tag.ToString() == style)
                    return i;
            }

            return 0;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            string wpFile = textBoxWallpaperFile.Text;
            string stvalue = (comboBoxWallpaperStyle.SelectedItem as ComboBoxItem).Tag.ToString();
            WallpaperStyle wpStyle = (WallpaperStyle)Enum.Parse(typeof(WallpaperStyle), stvalue, true);

            this.changeWallpaperAction.SaveWallpaperFile(this.networkId, wpFile, wpStyle);
        }

        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog();
            od.CheckFileExists = true;
            od.CheckPathExists = true;
            od.Filter = "*.*|*.*";
            od.Multiselect = false;
            od.ShowReadOnly = false;
            od.Title = DefaultResources.ChangeWallpaper_SelectFile;
            od.ShowDialog();

            if (!String.IsNullOrWhiteSpace(od.FileName))
                textBoxWallpaperFile.Text = od.FileName;
        }
    }
}

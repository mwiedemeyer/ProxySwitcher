using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Fluent;
using Hardcodet.Wpf.TaskbarNotification;
using ProxySwitcher.Common;
using ProxySwitcher.Core;
using ProxySwitcher.Core.Resources;
using ProxySwitcher.UI;

namespace ProxySwitcher
{
    /// <summary>
    /// Main Window Class
    /// </summary>
    public partial class MainWindowRibbon : RibbonWindow, IProxySwitcherHost
    {
        private AddInManager addInManager;
        private Guid selectedActionIdOnNextRefreshInTreeView;
        private Guid selectedNetworkIdOnNextRefreshInTreeView;
        private NetworkManager networkManager;
        private PSPolicy policy;
        private TaskbarIcon taskbarIcon;
        private bool shutdownInitiatedFromApplication;
        Windows7Helper win7Helper = new Windows7Helper(App.Current);

        public NetworkManager NetworkManager
        {
            get { return this.networkManager; }
        }

        public MainWindowRibbon()
        {
            LocalizationManager.SetLanguage(SettingsManager.Instance.GetApplicationSetting<int>(SettingsManager.App_Language, SettingsManager.Default_Language));

            InitializeComponent();

            policy = PolicyManager.ValidatePolicies();

            if (policy != null && policy.HasMessage)
            {
                if (policy.IsDisabled)
                {
                    MessageBox.Show(LanguageResources.PolicyDisabled, this.Title, MessageBoxButton.OK, MessageBoxImage.Stop);
                    shutdownInitiatedFromApplication = true;
                    Application.Current.Shutdown(500);
                    return;
                }

                SetStatus(policy.Message, policy.MessageLink);
            }
        }

        #region Window Events

        private void Window_Initialized(object sender, EventArgs e)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            string build = Assembly.GetExecutingAssembly().GetName().Version.Revision.ToString("0000");

            labelAboutVersion.Content = "Version " + version;
            AboutVersionLabel.Content = "Version " + version;
            AboutVersionBuildLabel.Content = "Build " + build;

            if (Logger.LogDebug)
            {
                AboutVersionBuildLabel.Visibility = System.Windows.Visibility.Visible;
                this.Title += " DEBUG";
            }

            try
            {
                this.addInManager = new AddInManager(this);
                this.networkManager = new NetworkManager(this.addInManager);
                this.networkManager.NetworkSwitched += new EventHandler<NetworkSwitchedEventArgs>(networkManager_NetworkSwitched);
                this.networkManager.RedetectNetworkStatusChanged += new EventHandler<RedetectNetworkStatusChangeEventArgs>(networkManager_RedetectNetworkStatusChanged);

                InitTaskbarIconAndContextMenu();
                InitActions();

                UpdateJumplist();
            }
            catch (AddInLoaderException)
            {
                MessageBox.Show(String.Format(LanguageResources.AddInLoadError, version), "Proxy Switcher", MessageBoxButton.OK, MessageBoxImage.Stop);
                Application.Current.Shutdown(501);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SettingsManager.Instance.NetworkConfigurationChangedEvent += new EventHandler(SettingsManager_NetworkConfigurationChangedEvent);
            SettingsManager.Instance.ApplicationConfigurationChangedEvent += new EventHandler(SettingsManager_ApplicationConfigurationChangedEvent);
            SettingsManager.Instance.AddInConfigurationChangedEvent += new EventHandler<AddInConfigurationChangedEventArgs>(SettingsManager_AddInConfigurationChangedEvent);

            LoadNetworkTreeView();

            ApplyUISettingsAndPolicies();

            this.networkManager.RedetectNetwork();

            CheckForUpdates(false);

            SetStatus(LanguageResources.Status_Ready);
        }

        private void RibbonWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                HideWindow();
            }
            else
            {
                ShowWindow();
            }
        }

        private void RibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (shutdownInitiatedFromApplication)
                return;

            // Prevent from quit the application. Instead minimize it.
            this.WindowState = System.Windows.WindowState.Minimized;
            e.Cancel = true;
        }

        #endregion

        #region Events

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = e.Source as Fluent.Button;
            switch (button.Tag.ToString())
            {
                case "New":
                    AddNewNetwork();
                    break;
                case "DeleteNetwork":
                    DeleteSelectedNetwork();
                    break;
                case "DeleteAction":
                    DeleteSelectedAction();
                    break;
                case "UpdateApp":
                    CheckForUpdates(true);
                    break;
                case "OpenSettings":
                    LoadSettingsInContentPanel();
                    break;
                case "RedetectNetwork":
                    this.networkManager.RedetectNetwork(true);
                    break;
                case "Exit":
                    shutdownInitiatedFromApplication = true;
                    Application.Current.Shutdown();
                    break;
                case "Donate":
                    Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=58DQTSK6JXBJE");
                    break;
                case "OpenSite":
                    Process.Start("http://proxyswitcher.net");
                    break;
                default:
                    break;
            }
        }

        private void addActionCommand_Click(object sender, RoutedEventArgs e)
        {
            var button = e.Source as Fluent.Button;
            Guid actionId = (Guid)button.Tag;
            AddAction(actionId);
        }

        private void networkManager_NetworkSwitched(object sender, NetworkSwitchedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(delegate
            {
                if (e.Network.IsUnknownNetwork)
                {
                    var message = LanguageResources.UnknownNetwork_Bubble;
                    if (SettingsManager.Instance.GetApplicationSetting<bool>(SettingsManager.App_NoNewNetwork))
                        message = LanguageResources.UnknownNetwork_Bubble_NoNew;

                    SetTaskbarTextAndIcon(LanguageResources.DeactivateNode_Name, message, e.Network);
                    if (Windows7Helper.IsWindows7)
                        win7Helper.RemoveOverlayIcon();
                }
                else
                {
                    SetTaskbarTextAndIcon(e.Network.Name, string.Format(LanguageResources.IsNowActivated, e.Network.Name), e.Network);
                    if (Windows7Helper.IsWindows7)
                        win7Helper.SetOverlayIcon(e.Network.IconPath);
                }
            }));
        }

        private void networkManager_RedetectNetworkStatusChanged(object sender, RedetectNetworkStatusChangeEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(delegate
            {
                switch (e.Status)
                {
                    case NetworkChangeStatus.Detecting:
                        SetStatus(LanguageResources.Status_Detecting);
                        break;
                    case NetworkChangeStatus.Completed:
                        if (labelAboutStatus.Tag == null || String.IsNullOrEmpty(labelAboutStatus.Tag.ToString()) || labelAboutStatus.Tag.ToString() != "FROM_ACTION")
                            SetStatus(LanguageResources.Status_Ready);
                        break;
                    case NetworkChangeStatus.Error:
                        SetStatus(LanguageResources.Status_DetectionFailed);
                        break;
                }
            }));
        }

        private void SettingsManager_NetworkConfigurationChangedEvent(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(delegate
            {
                LoadNetworkTreeView();
                ReBuildContextMenu();
                UpdateJumplist();

                SetStatus(LanguageResources.SettingsSaved_Status);
            }));
        }

        private void SettingsManager_AddInConfigurationChangedEvent(object sender, AddInConfigurationChangedEventArgs e)
        {
            if (e.UserInitiated)
                return;

            this.Dispatcher.BeginInvoke(new Action(delegate
            {
                this.selectedActionIdOnNextRefreshInTreeView = Guid.Empty;
                this.selectedNetworkIdOnNextRefreshInTreeView = Guid.Empty;
                SetContentControl(null);

                this.addInManager.ReloadAddInSettings();
                LoadNetworkTreeView();
            }));
        }

        private void SettingsManager_ApplicationConfigurationChangedEvent(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(delegate
            {
                this.selectedActionIdOnNextRefreshInTreeView = Guid.Empty;
                this.selectedNetworkIdOnNextRefreshInTreeView = Guid.Empty;
                SetContentControl(null);

                LoadNetworkTreeView();

                SetStatus(LanguageResources.SettingsSaved_Status);
            }));
        }

        private void treeViewNetworks_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (treeViewNetworks.SelectedItem is UnknownNetworkTreeViewItem)
            {
                SetContentControl(Environment.NewLine + Environment.NewLine + "\t" + LanguageResources.UnknownNetworkDescription);
            }
            else if (treeViewNetworks.SelectedItem is NetworkTreeViewItem)
            {
                var item = treeViewNetworks.SelectedItem as NetworkTreeViewItem;

                SetContentControl(new NetworkConfigurationControl(item.NetworkConfiguration));
            }
            else if (treeViewNetworks.SelectedItem is ActionTreeViewItem)
            {
                var item = treeViewNetworks.SelectedItem as ActionTreeViewItem;

                var action = this.addInManager.GetActionById(item.ActionId);

                if (action == null)
                {
                    SetContentControl(Environment.NewLine + Environment.NewLine + "\t" + LanguageResources.ActionMissing);
                    return;
                }

                var control = action.GetWindowControl(item.NetworkId, item.NetworkName);

                if (control == null)
                {
                    SetContentControl(Environment.NewLine + Environment.NewLine + "\t" + LanguageResources.ActionHasNoConfig);
                    return;
                }

                SetContentControl(control);
            }
            else
            {
                SetContentControl(null);
            }
        }

        private void treeViewNetworks_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(NetworkConfiguration)))
            {
                NetworkConfiguration sourceConfig = (NetworkConfiguration)e.Data.GetData(typeof(NetworkConfiguration));
                NetworkConfiguration targetConfig = GetItemAtLocation(e.GetPosition(treeViewNetworks));

                SettingsManager.Instance.MoveNetworkConfigurationAfter(sourceConfig, targetConfig);
            }
        }

        private void treeViewNetworks_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(NetworkConfiguration)))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void treeViewNetworks_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                NetworkTreeViewItem item = treeViewNetworks.SelectedItem as NetworkTreeViewItem;
                if (item == null)
                    return;

                if (treeViewNetworks.SelectedItem is UnknownNetworkTreeViewItem)
                    return;

                DragDrop.DoDragDrop(treeViewNetworks, item.NetworkConfiguration, DragDropEffects.Move);
            }
        }

        private void labelAboutStatus_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (labelAboutStatus.Tag == null)
                return;

            if (String.IsNullOrWhiteSpace(labelAboutStatus.Tag.ToString()))
                return;

            if (labelAboutStatus.Tag.ToString() == "FROM_ACTION")
                return;

            Process.Start(labelAboutStatus.Tag.ToString());
        }

        private void taskbarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            ShowWindow();
        }

        private void taskbarIcon_TrayBalloonTipClicked(object sender, RoutedEventArgs e)
        {
            if (taskbarIcon.Tag == null)
                return;

            var nc = taskbarIcon.Tag as NetworkConfiguration;
            if (nc == null)
                return;

            if (policy != null && policy.NetworkSettingsLocked)
                return;

            if (SettingsManager.Instance.GetApplicationSetting<bool>(SettingsManager.App_NoNewNetwork))
                return;

            if (nc.IsUnknownNetwork)
            {
                ShowWindow();
                AddNewNetwork();
                Focus();
            }
        }

        private void contextMenu_Click(object sender, RoutedEventArgs e)
        {
            var item = e.Source as System.Windows.Controls.MenuItem;
            if (item == null)
                return;

            if (item.Tag.ToString() == "SHOW")
            {
                this.Show();
                this.WindowState = System.Windows.WindowState.Normal;
                this.Focus();
            }
            if (item.Tag.ToString() == "AUTOSWITCH")
            {
                SettingsManager.Instance.SaveApplicationSetting(SettingsManager.App_AutoSwitchKey, item.IsChecked.ToString());
            }
            if (item.Tag.ToString() == "REDETECT")
            {
                this.networkManager.RedetectNetwork(true);
            }
            if (item.Tag.ToString() == "EXIT")
            {
                shutdownInitiatedFromApplication = true;
                Application.Current.Shutdown();
            }
            if (item.Tag is Guid)
            {
                this.networkManager.ActivateNetwork((Guid)item.Tag);
            }
        }

        #endregion

        #region Private Methods

        private NetworkConfiguration GetItemAtLocation(Point location)
        {
            NetworkConfiguration foundItem = null;
            HitTestResult hitTestResults = VisualTreeHelper.HitTest(treeViewNetworks, location);

            if (hitTestResults.VisualHit is FrameworkElement)
            {
                if ((hitTestResults.VisualHit as FrameworkElement).Parent == null)
                    return null;

                NetworkTreeViewItem dataObject = ((hitTestResults.VisualHit as FrameworkElement).Parent as FrameworkElement).Parent as NetworkTreeViewItem;
                if (dataObject == null)
                    return null;

                NetworkTreeViewItem ntvItem = null;

                foreach (var item in treeViewNetworks.Items)
                {
                    if ((item as NetworkTreeViewItem) != null)
                    {
                        if ((item as NetworkTreeViewItem).NetworkConfiguration.Name == dataObject.NetworkConfiguration.Name)
                        {
                            ntvItem = item as NetworkTreeViewItem;
                            break;
                        }
                    }
                }

                if (ntvItem != null)
                    foundItem = ntvItem.NetworkConfiguration;
            }

            return foundItem;
        }

        private void SetStatus(string message, string link = null)
        {
            if (policy != null && policy.HasMessage && message == LanguageResources.Status_Ready)
            {
                message = policy.Message;
                link = policy.MessageLink;
            }

            labelAboutStatus.Content = message;
            labelAboutStatus.Tag = string.Empty;

            if (!String.IsNullOrWhiteSpace(link))
            {
                labelAboutStatus.Tag = link;
                labelAboutStatus.Foreground = new SolidColorBrush(Colors.Blue);
                labelAboutStatus.Cursor = Cursors.Hand;
                labelAboutStatus.Content = new TextBlock() { Text = message, TextDecorations = TextDecorations.Underline };
            }
            else
            {
                labelAboutStatus.Foreground = new SolidColorBrush(Colors.Black);
                labelAboutStatus.Cursor = Cursors.Arrow;
            }
        }

        private void LoadSettingsInContentPanel()
        {
            SetContentControl(new SettingsConfigurationControl(this));
        }

        private void SetContentControl(object content)
        {
            this.contentControl1.Content = content;
            SetStatus(LanguageResources.Status_Ready);
        }

        private BitmapImage GetBitmapImageFromStream(Stream stream)
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = stream;
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();
            return img;
        }

        private void InitActions()
        {
            AddActionDropDownGallery.Items.Clear();

            foreach (var action in this.addInManager.Actions)
            {
                var rb = new Fluent.Button();
                rb.Click += new RoutedEventHandler(addActionCommand_Click);
                rb.Header = action.Name;
                rb.Tag = action.Id;
                if (action.IconResourceStream == null)
                    rb.LargeIcon = new BitmapImage(new Uri(@"pack://application:,,,/Images/action_nologo.png", UriKind.RelativeOrAbsolute));
                else
                    rb.LargeIcon = GetBitmapImageFromStream(action.IconResourceStream);

                AddActionDropDownGallery.Items.Add(rb);
            }

            // Find more Action AddIns Button
            var button = new Fluent.Button();
            button.Click += new RoutedEventHandler(FindMoreAddInsClick);
            button.Header = LanguageResources.FindMoreActions_Button;
            button.LargeIcon = new BitmapImage(new Uri(@"pack://application:,,,/Images/findmore.png", UriKind.RelativeOrAbsolute));
            AddActionDropDownGallery.Items.Add(button);
        }

        private void FindMoreAddInsClick(object sender, RoutedEventArgs e)
        {
            Process.Start("http://projects2.mwiedemeyer.de/ProxySwitcher/SitePages/MoreAddIns.aspx");
        }

        private void InitTaskbarIconAndContextMenu()
        {
            this.taskbarIcon = new TaskbarIcon();

            ReBuildContextMenu();

            SetTaskbarIcon(this.Icon);
            this.taskbarIcon.ToolTipText = LanguageResources.Startup_ToolTipText;
            this.taskbarIcon.TrayMouseDoubleClick += new RoutedEventHandler(taskbarIcon_TrayMouseDoubleClick);
            this.taskbarIcon.TrayBalloonTipClicked += new RoutedEventHandler(taskbarIcon_TrayBalloonTipClicked);
        }

        private void SetTaskbarIcon(System.Drawing.Icon icon)
        {
            this.taskbarIcon.Icon = icon;
        }

        private void SetTaskbarIcon(ImageSource imageSource)
        {
            this.taskbarIcon.IconSource = imageSource;
        }

        private void ReBuildContextMenu()
        {
            var contextMenu = new System.Windows.Controls.ContextMenu();

            // Show Window
            var showMenu = new System.Windows.Controls.MenuItem() { Header = LanguageResources.Show_Menu, Tag = "SHOW", FontWeight = FontWeights.Bold };
            showMenu.Click += new RoutedEventHandler(contextMenu_Click);
            contextMenu.Items.Add(showMenu);

            // Separator
            contextMenu.Items.Add(new Separator());

            // AutoSwitch
            var autoSwitchingMenu = new System.Windows.Controls.MenuItem() { Header = LanguageResources.AutoSwitching_Menu, IsCheckable = true, StaysOpenOnClick = true, Tag = "AUTOSWITCH" };
            autoSwitchingMenu.Click += new RoutedEventHandler(contextMenu_Click);
            bool isAutoSwitch = SettingsManager.Instance.GetApplicationSetting(SettingsManager.App_AutoSwitchKey, SettingsManager.Default_AutoSwitch);
            autoSwitchingMenu.IsChecked = isAutoSwitch;
            contextMenu.Items.Add(autoSwitchingMenu);

            // Redetect
            var redetectMenu = new System.Windows.Controls.MenuItem() { Header = LanguageResources.ReDetect_Menu, Tag = "REDETECT" };
            redetectMenu.Click += new RoutedEventHandler(contextMenu_Click);
            contextMenu.Items.Add(redetectMenu);

            // Separator
            contextMenu.Items.Add(new Separator());

            // Manual Switch
            foreach (var nc in SettingsManager.Instance.GetNetworkConfigurations(false))
            {
                if (nc.IsUnknownNetwork)
                    continue;

                var networkMenuItem = new System.Windows.Controls.MenuItem() { Header = nc.Name, Tag = nc.Id };
                networkMenuItem.Click += new RoutedEventHandler(contextMenu_Click);
                networkMenuItem.IsChecked = (nc.Id == NetworkManager.CurrentNetworkConfigurationId);
                contextMenu.Items.Add(networkMenuItem);
            }

            // Separator
            contextMenu.Items.Add(new Separator());

            // Exit
            var exitMenu = new System.Windows.Controls.MenuItem() { Header = LanguageResources.Exit_Menu, Tag = "EXIT" };
            exitMenu.Click += new RoutedEventHandler(contextMenu_Click);
            contextMenu.Items.Add(exitMenu);

            this.taskbarIcon.ContextMenu = contextMenu;
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = System.Windows.WindowState.Normal;
        }

        private void HideWindow()
        {
            var useWin7Mode = false;
            if (Windows7Helper.IsWindows7)
                useWin7Mode = SettingsManager.Instance.GetApplicationSetting(SettingsManager.App_UseWin7Mode, SettingsManager.Default_UseWin7Mode);

            if (useWin7Mode)
                UpdateJumplist();
            else
            {
                this.WindowState = System.Windows.WindowState.Minimized;
                this.Hide();
            }
        }

        private void UpdateJumplist()
        {
            if (!Windows7Helper.IsWindows7 || !SettingsManager.Instance.GetApplicationSetting<bool>(SettingsManager.App_UseWin7Mode, SettingsManager.Default_UseWin7Mode))
                return;

            try
            {
                this.win7Helper.ClearJumpList();

                foreach (var nc in SettingsManager.Instance.GetNetworkConfigurations(false))
                {
                    if (nc.IsUnknownNetwork)
                        continue;

                    this.win7Helper.AddJumpTask(nc);
                }

                this.win7Helper.ApplyUpdates();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, ex);
            }
        }

        private void SetTaskbarTextAndIcon(string tooltipText, string ballonText, NetworkConfiguration network = null)
        {
            this.taskbarIcon.Tag = network;
            this.taskbarIcon.ToolTipText = tooltipText + " - Proxy Switcher";

            // highlight active network in context menu
            foreach (object item in this.taskbarIcon.ContextMenu.Items)
            {
                if (!(item is System.Windows.Controls.MenuItem))
                    continue;

                var mitem = item as System.Windows.Controls.MenuItem;

                if (mitem.Tag.ToString() == "AUTOSWITCH")
                    continue;

                if (network != null && mitem.Tag is Guid && (Guid)mitem.Tag == network.Id)
                    mitem.IsChecked = true;
                else
                    mitem.IsChecked = false;
            }

            // highlight tree item
            foreach (var item in this.treeViewNetworks.Items)
            {
                var nItem = item as NetworkTreeViewItem;
                if (nItem == null)
                    continue;

                if (network != null && nItem.NetworkId != SwitcherActionBase.DeactivateNetworkId && nItem.NetworkId == network.Id)
                    nItem.FontWeight = FontWeights.Bold;
                else
                    nItem.FontWeight = FontWeights.Normal;
            }

            if (network != null && !String.IsNullOrEmpty(network.IconPath) && network.IconPath.EndsWith(".ico"))
            {
                try
                {
                    SetTaskbarIcon(new System.Drawing.Icon(network.IconPath));
                }
                catch
                {
                    SetTaskbarIcon(new System.Drawing.Icon(Application.GetResourceStream(new Uri(this.Icon.ToString())).Stream));
                }
            }
            else
            {
                SetTaskbarIcon(new System.Drawing.Icon(Application.GetResourceStream(new Uri(this.Icon.ToString())).Stream));
            }

            this.taskbarIcon.ShowBalloonTip("Proxy Switcher", ballonText, BalloonIcon.Info);
        }

        private void CheckForUpdates(bool fromUI)
        {
            bool autoCheck = SettingsManager.Instance.GetApplicationSetting(SettingsManager.App_CheckForUpdates, SettingsManager.Default_CheckForUpdates);
            if (!autoCheck && !fromUI)
                return;

            CheckForUpdatesButton.Header = LanguageResources.UpdateInProgress_Button;
            CheckForUpdatesButton.IsEnabled = false;

            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object state)
            {
                Thread.Sleep(3000);

                try
                {
                    WebClient wc = new WebClient();

                    string version = wc.DownloadString("http://mwiedemeyer.de/downloads/ps/version.txt");

                    Version v = new Version(version);
                    if (v > Assembly.GetExecutingAssembly().GetName().Version)
                    {
                        this.Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBoxResult dr = MessageBox.Show(string.Format(LanguageResources.UpdateAvailable, v.ToString(3)), "Proxy Switcher", MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if (dr == MessageBoxResult.Yes)
                            {
                                Process.Start("http://proxyswitcher.net");
                            }
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Check for update failed", ex);
                }
                finally
                {
                    this.Dispatcher.Invoke(new Action(delegate
                    {
                        CheckForUpdatesButton.Header = LanguageResources.Update_Button;
                        CheckForUpdatesButton.IsEnabled = true;
                    }));
                }
            }));
        }

        private void ApplyUISettingsAndPolicies()
        {
            bool startMinimized = SettingsManager.Instance.GetApplicationSetting(SettingsManager.App_StartMinimized, SettingsManager.Default_StartMinimized);
            if (startMinimized)
            {
                this.WindowState = WindowState.Minimized;
                HideWindow();
            }

            // Policies
            if (policy != null && policy.NetworkSettingsLocked)
            {
                treeViewNetworks.IsEnabled = false;
                ButtonNewNetwork.IsEnabled = false;
                ButtonDeleteNetwork.IsEnabled = false;
                AddActionDropDownButton.IsEnabled = false;
                ButtonDeleteAction.IsEnabled = false;
            }
            if (policy != null && policy.ApplicationSettingsLocked)
            {
                ButtonOpenSettings.IsEnabled = false;
            }
        }

        private void LoadNetworkTreeView()
        {
            treeViewNetworks.Items.Clear();

            foreach (var item in SettingsManager.Instance.GetNetworkConfigurations())
            {
                NetworkTreeViewItem ni = null;

                if (item.Id == SwitcherActionBase.DeactivateNetworkId)
                    ni = new UnknownNetworkTreeViewItem(item, this.addInManager.GetActionsByIds(item.Actions));
                else
                    ni = new NetworkTreeViewItem(item, this.addInManager.GetActionsByIds(item.Actions));

                if (!(ni is UnknownNetworkTreeViewItem) && ni.NetworkId == NetworkManager.CurrentNetworkConfigurationId)
                    ni.FontWeight = FontWeights.Bold;
                else
                    ni.FontWeight = FontWeights.Normal;

                treeViewNetworks.Items.Add(ni);

                if (item.Id == this.selectedNetworkIdOnNextRefreshInTreeView)
                {
                    ni.IsSelected = true;
                    if (this.selectedActionIdOnNextRefreshInTreeView != Guid.Empty)
                        ni.SelectAction(this.selectedActionIdOnNextRefreshInTreeView);
                }
            }
        }

        private void DeleteSelectedAction()
        {
            if (treeViewNetworks.SelectedItem == null)
                return;

            if (treeViewNetworks.SelectedItem is ActionTreeViewItem)
            {
                var item = treeViewNetworks.SelectedItem as ActionTreeViewItem;

                var nc = SettingsManager.Instance.GetNetworkConfiguration(item.NetworkId);
                nc.DeleteAction(item.ActionId);

                SettingsManager.Instance.SaveNetworkConfiguration(nc);
            }
        }

        private void DeleteSelectedNetwork()
        {
            if (treeViewNetworks.SelectedItem == null)
                return;

            if (treeViewNetworks.SelectedItem is NetworkTreeViewItem)
            {
                var item = treeViewNetworks.SelectedItem as NetworkTreeViewItem;

                if (item.NetworkId == SwitcherActionBase.DeactivateNetworkId)
                {
                    MessageBox.Show(LanguageResources.UnknownNetwork_CannotDelete, this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                MessageBoxResult result = MessageBox.Show(String.Format(LanguageResources.DeleteNetworkAreYouSure, item.NetworkConfiguration.Name), this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    SettingsManager.Instance.DeleteNetworkConfiguration(item.NetworkId);
                }
            }
        }

        private void AddNewNetwork()
        {
            var nc = NetworkConfiguration.CreateNewNetworkConfiguration();

            this.selectedActionIdOnNextRefreshInTreeView = Guid.Empty;
            this.selectedNetworkIdOnNextRefreshInTreeView = nc.Id;

            SettingsManager.Instance.SaveNetworkConfiguration(nc);
        }

        private void AddAction(Guid actionId)
        {
            if (treeViewNetworks.SelectedItem == null)
                return;

            NetworkTreeViewItem item = null;
            if (treeViewNetworks.SelectedItem is NetworkTreeViewItem)
                item = treeViewNetworks.SelectedItem as NetworkTreeViewItem;
            else if (treeViewNetworks.SelectedItem is ActionTreeViewItem)
                item = (treeViewNetworks.SelectedItem as ActionTreeViewItem).ParentNetworkItem;

            var nc = SettingsManager.Instance.GetNetworkConfiguration(item.NetworkId);
            nc.AddAction(actionId);

            this.selectedActionIdOnNextRefreshInTreeView = actionId;
            this.selectedNetworkIdOnNextRefreshInTreeView = item.NetworkId;

            SettingsManager.Instance.SaveNetworkConfiguration(nc);
        }

        #endregion

        #region IProxySwitcherHost Members

        public void SetStatusText(SwitcherActionBase ownerAction, string message, bool isError = false)
        {
            if (ownerAction == null)
                return;

            this.Dispatcher.Invoke(new Action(delegate
            {
                labelAboutStatus.Tag = "FROM_ACTION";
                labelAboutStatus.Content = ownerAction.Name + ": " + message;

                if (isError)
                    labelAboutStatus.Foreground = new SolidColorBrush(Colors.Red);
                else
                    labelAboutStatus.Foreground = new SolidColorBrush(Colors.Black);

                labelAboutStatus.Cursor = Cursors.Arrow;
            }));
        }

        #endregion
    }
}

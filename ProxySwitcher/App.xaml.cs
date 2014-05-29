using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using ProxySwitcher.Core;
using System.Threading;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Ipc;

namespace ProxySwitcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Mutex mutex;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // BETA/CTP code
            //if (DateTime.Today > new DateTime(2012, 2, 28))
            //{
            //    MessageBox.Show("This TEST version has expired. Please visit http://proxyswitcher.net for a newer version.", "Proxy Switcher", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //    Application.Current.Shutdown(600);
            //    return;
            //}

            try
            {
                string versionGuid = "35B89795-EA7A-4098-90B7-1B88EF5F50FF";
                string mutexName = @"Global\ProxySwitcher" + versionGuid;

                bool firstInstance;

                mutex = new Mutex(true, mutexName, out firstInstance);

                Logger.Log("IsFirstInstance: " + firstInstance.ToString());

                if (!firstInstance)
                {
                    CallRunningInstance(e.Args);
                    Application.Current.Shutdown();
                    return;
                }

                try
                {
                    InitRemoting();
                }
                catch (Exception remoteEx)
                {
                    Logger.Log("Initializing remoting failed. Calls from Windows 7 Superbar Jumplist and from a command line will not work correctly.", remoteEx);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unhandled exception during startup has occured. Please send a bug report to support@proxyswitcher.net with the following details:" + Environment.NewLine + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine + "More details can be found in Log.txt in " + SettingsManager.GetSettingsFolder(), "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                Logger.Log("Unhandled Startup Exception: " + ex.Message, ex);

                if (mutex != null)
                    mutex.Close();

                Application.Current.Shutdown(700);
            }
        }

        private static void InitRemoting()
        {
            ChannelServices.RegisterChannel(new IpcChannel("localhost:62982"), false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ProxySwitcher.RemotingService),
                                           "ProxySwitcherService.rem", WellKnownObjectMode.Singleton);
        }

        private static void CallRunningInstance(string[] args)
        {
            RemotingService rs = (RemotingService)RemotingServices.Connect(typeof(ProxySwitcher.RemotingService), "ipc://localhost:62982/ProxySwitcherService.rem");

            if (args.Length < 1)
            {
                //rs.BringToFront();
                return;
            }

            if (args[0] == "/activate")
            {
                rs.ChangeProxy(args[1]);
            }
            else if (args[0] == "/deactivate")
            {
                //rs.DisableProxies();
            }
            else if (args[0] == "/redetect")
            {
                rs.RedetectNetwork();
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception == null)
                return;

            Logger.Log("Unhandled Exception", e.Exception);

            if (mutex != null)
                mutex.Close();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (mutex != null)
                mutex.Close();
        }
    }

    public class RemotingService : MarshalByRefObject
    {
        public RemotingService() { }

        //public void DisableProxies()
        //{
        //    Program.formMain.DisableAllProxies();
        //}

        //public void BringToFront()
        //{
        //    Program.formMain.WindowState = FormWindowState.Normal;
        //    Program.formMain.Focus();
        //}

        public void ChangeProxy(string networkId)
        {
            App.Current.Dispatcher.Invoke(new Action(delegate
            {
                var mainWindow = App.Current.MainWindow as MainWindowRibbon;
                if (mainWindow == null)
                    return;
                try
                {
                    mainWindow.NetworkManager.ActivateNetwork(new Guid(networkId));
                }
                catch (Exception ex)
                {
                    Logger.Log("Activate Network from command line failed: " + ex.Message, ex);
                }
            }));
        }

        public void RedetectNetwork()
        {
            App.Current.Dispatcher.Invoke(new Action(delegate
            {
                var mainWindow = App.Current.MainWindow as MainWindowRibbon;
                if (mainWindow == null)
                    return;
                try
                {
                    mainWindow.NetworkManager.RedetectNetwork(true);
                }
                catch (Exception ex)
                {
                    Logger.Log("Redetect Network from command line failed: " + ex.Message, ex);
                }
            }));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace ProxySwitcher.Actions.DefaultActions.ChangePrinter
{
    internal class PrinterManager
    {
        private static List<string> printerCache = null;

        public static List<string> GetAllPrinters(bool fromCache)
        {
            if (fromCache && printerCache != null)
            {
                return printerCache;
            }
            else
            {
                var printers = GetAllPrintersInternal(0);
                printerCache = printers;
                return printers;
            }
        }

        private static List<string> GetAllPrintersInternal(int failCounter)
        {
            try
            {
                List<string> printers = new List<string>();

                string selectAllQuery = "SELECT * FROM Win32_Printer";

                System.Management.ObjectQuery oq = new System.Management.ObjectQuery(selectAllQuery);

                System.Management.ManagementObjectSearcher query1 = new System.Management.ManagementObjectSearcher(oq);
                System.Management.ManagementObjectCollection queryCollection1 = query1.Get();

                foreach (System.Management.ManagementObject mo in queryCollection1)
                {
                    System.Management.PropertyDataCollection pdc = mo.Properties;
                    printers.Add(mo["Name"].ToString());
                }

                return printers;
            }
            catch
            {
                if (failCounter > 3)
                    throw;

                Thread.Sleep(new TimeSpan(0, 0, 5));
                return GetAllPrintersInternal(++failCounter);
            }
        }

        public static void SetNewDefaultPrinter(string newDefaultPrinterName)
        {
            string selectAllQuery = "SELECT * FROM Win32_Printer";

            System.Management.ObjectQuery oq = new System.Management.ObjectQuery(selectAllQuery);

            System.Management.ManagementObjectSearcher query1 = new System.Management.ManagementObjectSearcher(oq);
            System.Management.ManagementObjectCollection queryCollection1 = query1.Get();
            System.Management.ManagementObject newDefault = null;

            foreach (System.Management.ManagementObject mo in queryCollection1)
            {
                System.Management.PropertyDataCollection pdc = mo.Properties;
                if (mo["Name"].ToString().ToUpper().Trim() == newDefaultPrinterName.ToUpper())
                {
                    newDefault = mo;
                    break;
                }
            }

            if (newDefault != null)
            {
                System.Management.ManagementBaseObject outParams = newDefault.InvokeMethod("SetDefaultPrinter", null, null);
                string returnValue = outParams["returnValue"].ToString();
                if (returnValue != "0")
                    throw new ApplicationException(string.Format("Change Printer '{0}' failed. Return Value: {1}", newDefault.ToString(), returnValue));

                var HWND_BROADCAST = new IntPtr(0xffff);
                uint WM_SETTINGCHANGE = 0x001A;
                UIntPtr innerPinvokeResult;
                var pinvokeResult = SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, UIntPtr.Zero,
                    IntPtr.Zero, SendMessageTimeoutFlags.SMTO_NORMAL, 1000, out innerPinvokeResult);
            }
            else
            {
                throw new ApplicationException(string.Format("Change Printer '{0}' failed. Managemengt object not found", newDefaultPrinterName));
            }
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint Msg,
            UIntPtr wParam,
            IntPtr lParam,
            SendMessageTimeoutFlags fuFlags,
            uint uTimeout,
            out UIntPtr lpdwResult);

        [Flags]
        enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0,
            SMTO_BLOCK = 0x1,
            SMTO_ABORTIFHUNG = 0x2,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x8
        }
    }
}

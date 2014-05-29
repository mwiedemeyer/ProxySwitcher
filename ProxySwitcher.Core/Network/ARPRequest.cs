using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.InteropServices;

namespace ProxySwitcher.Core
{
    internal static class ARPRequest
    {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(uint destIP, uint srcIP, byte[] macAddress, ref uint macAddressLength);

        public static string GetMacAddress(IPAddress address)
        {
            try
            {
                byte[] mac = new byte[6];
                uint len = (uint)mac.Length;
                byte[] addressBytes = address.GetAddressBytes();
                uint dest = ((uint)addressBytes[3] << 24)
                  + ((uint)addressBytes[2] << 16)
                  + ((uint)addressBytes[1] << 8)
                  + ((uint)addressBytes[0]);
                if (SendARP(dest, 0, mac, ref len) != 0)
                {
                    return string.Empty;
                }

                string macString = string.Empty;
                foreach (var m in mac)
                {
                    var s = Convert.ToString(m, 16).ToUpper();
                    s = s.Length == 2 ? s : "0" + s;
                    macString += s + ":";
                }

                return macString.TrimEnd(':');
            }
            catch { return string.Empty; }
        }

    }
}

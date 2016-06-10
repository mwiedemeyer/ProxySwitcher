using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxySwitcher.Common;
using System.Runtime.InteropServices;
using ProxySwitcher.Actions.DefaultActions.InternetExplorer;
using System.IO;
using System.Reflection;
using ProxySwitcher.Actions.ProxyBase;

namespace ProxySwitcher.Actions.DefaultActions
{
    [SwitcherActionAddIn]
    public class InternetExplorer8ProxyAction : ProxySwitcherAction
    {
        #region Update Internet Explorer with P/Invoke

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);

        public static void RefreshIESettings()
        {
            const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
        }

        #endregion

        protected override object GetCustomContent(Guid networkId)
        {
            return new IECustomContent(this, networkId);
        }

        protected override bool IsAuthenticationSupported
        {
            get { return false; }
        }

        public override void ValidateEntry(ProxyEntry proxy)
        {
            if (proxy.Exceptions.Contains("/"))
                throw new ProxyValidationException(DefaultResources.ProxyExceptionsError);
        }

        public override void Activate(ProxyEntry proxy, Guid networkId)
        {
            string prx = string.Empty;
            if (proxy.Url.IsAllSet)
                prx = proxy.Url[ProxyScheme.All] + ":" + proxy.Port[ProxyScheme.All].ToString();
            else
            {
                foreach (var scheme in proxy.Url.Keys)
                {
                    prx += String.Format("{0}={1}:{2};", scheme.ToString().ToLower(), proxy.Url[scheme], proxy.Port[scheme]);
                }
            }

            string sid = System.Security.Principal.WindowsIdentity.GetCurrent().User.Value;

            // 01=disabled, 03=enabled, 05=auto config, 09=auto detect, 0b=auto detect and manual proxy, 0D=auto config und auto detect
            byte enabled = (byte)03;
            if (proxy.IsAutoDetect && !String.IsNullOrEmpty(prx))
                enabled = (byte)11; //11=0b
            else if (proxy.IsAutoConf && proxy.IsAutoDetect)
                enabled = (byte)13; //13=0d
            else if (proxy.IsAutoConf)
                enabled = (byte)05;
            else if (proxy.IsAutoDetect)
                enabled = (byte)09;
            //Convert Proxy Addresses to Bytes
            byte[] proxyBytes = Encoding.Default.GetBytes(prx);
            byte entryLength = (byte)proxyBytes.Length;
            if (proxy.IsAutoConf)
            {
                proxyBytes = new byte[0];
                entryLength = 0;
            }

            byte[] configStart = new byte[] { 70, 00, 00, 00, 01, 00, 00, 03, enabled, 00, 00, 00, entryLength, 00, 00, 00 };

            string byPassString = string.Empty;
            if (proxy.ByPassLocal || !String.IsNullOrEmpty(proxy.Exceptions))
            {
                byPassString = proxy.Exceptions;

                if (proxy.ByPassLocal)
                {
                    if (byPassString.Length != 0)
                        byPassString += ";<local>";
                    else
                        byPassString = "<local>";
                }
            }
            //Convert Exceptions to Bytes
            byte[] exceptions = Encoding.Default.GetBytes(byPassString);

            int autoConfigUrlArrayLength = 0;
            byte[] autoConfigUrl = new byte[0];
            if (proxy.IsAutoConf)
            {
                autoConfigUrl = Encoding.Default.GetBytes(proxy.Url.FirstEntry());
                autoConfigUrlArrayLength = autoConfigUrl.Length;
            }

            int lastPosition = 0;
            //Set Merged
            byte[] merged = new byte[configStart.Length + proxyBytes.Length + 4 + exceptions.Length + 4 + autoConfigUrlArrayLength + 31];
            
            //Add ConfigStart to Merged
            configStart.CopyTo(merged, lastPosition);
            lastPosition = configStart.Length;

            //Add ProxyServers to Merged
            proxyBytes.CopyTo(merged, lastPosition);
            lastPosition += proxyBytes.Length;
            //Convert exceptions string length to Hex Values
            string hexString = exceptions.Length.ToString("x");
            //first character should be 0 if string is not even length
            hexString = (hexString.Length % 2 == 0 ? "" : "0") + hexString;
            //Convert Hex String to Byte Array
            int NumberChars = hexString.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }
            //Reverse Byte Array
            Array.Reverse(bytes);
            Array.Resize(ref bytes, 4);

            bytes.CopyTo(merged, lastPosition);
            lastPosition += 4;

            //Add Exceptions to Merged
            exceptions.CopyTo(merged, lastPosition);
            lastPosition += exceptions.Length;

            
            //Add AutoConfigURL to Merged
            new byte[] { (byte)autoConfigUrl.Length, 0, 0, 0 }.CopyTo(merged, lastPosition);
            lastPosition += 4;

            if (proxy.IsAutoConf)
            {
                autoConfigUrl.CopyTo(merged, lastPosition);
                lastPosition += autoConfigUrl.Length;
            }

            new byte[31].CopyTo(merged, lastPosition);


            if (UseDialUp(networkId))
            {
                string connectionName = GetDialUpName(networkId);
                RegistryHelper.SetBinaryValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Connections", connectionName, merged);
            }
            else
            {
                RegistryHelper.SetBinaryValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Connections", "DefaultConnectionSettings", merged);
            }

            RefreshIESettings();
        }

        public override ProxyEntry GetDefaultProxy()
        {
            byte[] binValue = (byte[])RegistryHelper.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Connections", "DefaultConnectionSettings");

            ProxyEntry proxy = new ProxyEntry();

            proxy.IsAutoConf = (binValue[8] == (byte)05);

            List<byte> proxyUrlPortBytes = new List<byte>();
            var length = binValue[12] + 16;
            for (int i = 16; i < length; i++)
            {
                proxyUrlPortBytes.Add(binValue[i]);
            }

            string val = Encoding.Default.GetString(proxyUrlPortBytes.ToArray());
            int httpIndex = val.IndexOf("http=");
            if (httpIndex > 0)
            {
                int endIndex = val.IndexOf(";", httpIndex + 1);
                val = val.Substring(httpIndex + "http=".Length, endIndex - httpIndex - "http=".Length);
            }
            string[] urlPort = val.Split(':');
            if (urlPort[0].Contains("="))
                return proxy;

            proxy.Url[ProxyScheme.All] = urlPort[0];
            int port;
            if (urlPort.Length > 1 && int.TryParse(urlPort[1], out port))
                proxy.Port[ProxyScheme.All] = port;

            return proxy;
        }

        private bool UseDialUp(Guid networkId)
        {
            bool du = false;
            bool.TryParse(Settings[networkId.ToString() + "_UseDialUp"], out du);
            return du;
        }

        private string GetDialUpName(Guid networkId)
        {
            return Settings[networkId.ToString() + "_UseDialUpName"];
        }

        public override string Name
        {
            get { return "Internet Explorer"; }
        }

        public override string Group
        {
            get { return "IE"; }
        }

        public override string Description
        {
            get { return string.Empty; }
        }

        public override Stream IconResourceStream
        {
            get { return Assembly.GetExecutingAssembly().GetManifestResourceStream("ProxySwitcher.Actions.DefaultActions.Images.ie8.png"); }
        }

        public override Guid Id
        {
            get { return new Guid("6EC4A065-414E-428B-8319-34DAFD318CAA"); }
        }
    }
}

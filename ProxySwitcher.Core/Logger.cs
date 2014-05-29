using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ProxySwitcher.Core
{
    public static class Logger
    {
        private static readonly object lockObject = new object();

        public static bool LogDebug
        {
            get
            {
                string[] commandArgs = Environment.GetCommandLineArgs();
                if (commandArgs.Length > 1)
                    return (commandArgs[1].ToUpper() == "DEBUG");

                bool returnValue = false;
#if DEBUG
                returnValue = true;
#endif
                return returnValue;
            }
        }

        public static void Log(string message, Exception ex = null)
        {
            lock (lockObject)
            {
                if (ex == null && !LogDebug)
                    return;

                try
                {
                    string path = SettingsManager.GetSettingsFolder();
                    using (StreamWriter sw = new StreamWriter(Path.Combine(path, "Log.txt"), true))
                    {
                        sw.WriteLine(string.Format("[{0}] {1}", DateTime.Now.ToString(), message));
                        if (ex != null && LogDebug)
                        {
                            sw.WriteLine(ex.Message);
                            sw.WriteLine(ex.StackTrace);
                            sw.WriteLine("------------");
                            Exception ex2 = ex.InnerException;
                            while (ex2 != null)
                            {
                                sw.WriteLine(ex2.Message);
                                sw.WriteLine(ex2.StackTrace);
                                ex2 = ex2.InnerException;
                            }
                        }
                    }
                }
                catch { }
            }
        }
    }
}

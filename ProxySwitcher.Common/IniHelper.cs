using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ProxySwitcher.Common
{
    public class IniHelper
    {
        private string path;
        private List<string> iniContent;

        public IniHelper(string path)
        {
            iniContent = new List<string>();

            this.path = path;

            using (StreamReader sr = new StreamReader(path))
            {
                while (!sr.EndOfStream)
                {
                    iniContent.Add(sr.ReadLine());
                }
            }
        }

        public string GetValue(string sectionName, string entryName)
        {
            sectionName = "[" + sectionName + "]";
            bool sectionFound = false;
            foreach (var item in iniContent)
            {
                if (item == sectionName)
                {
                    sectionFound = true;
                    continue;
                }
                if (item.Contains(entryName) && sectionFound && item.Contains("="))
                    return item.Split('=')[1];
            }

            return string.Empty;
        }

        public void SetValue(string sectionName, string entryName, string newValue)
        {
            sectionName = "[" + sectionName + "]";
            bool sectionFound = false;
            bool entryFound = false;
            int i = -1;
            int sectionIndex = -1;
            string newContent = string.Empty;
            foreach (var item in iniContent)
            {
                i++;
                if (!sectionFound)
                    sectionIndex++;

                if (item == sectionName)
                {
                    sectionFound = true;
                    continue;
                }

                if ((item.Contains(entryName + "=") || item.Contains(entryName + " =")) && sectionFound)
                {
                    string[] split = item.Split('=');
                    newContent = split[0] + "=" + newValue;
                    entryFound = true;
                    break;
                }
            }

            if (entryFound)
            {
                iniContent[i] = newContent;
            }
            else
            {
                iniContent.Insert(sectionIndex + 1, entryName + "=" + newValue);
            }
        }

        public void Save()
        {
            using (StreamWriter sw = new StreamWriter(path, false))
            {
                foreach (var item in iniContent)
                {
                    sw.WriteLine(item);
                }
            }
        }
    }
}

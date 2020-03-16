using System;
using System.IO;

namespace CryptoTools.Generic
{
    public static class GlobalData
    {
        static string appSessionDate = DateTime.Now.ToString("yyyy-MM-dd @ HH-mm-ss");
        static string logPath = AppDomain.CurrentDomain.BaseDirectory + "logs\\" + appSessionDate + "\\";
        public static int MaxLogVolume = 0x3200000;
        public static bool SocketLogging = false;

        static GlobalData()
        {
            Directory.CreateDirectory(logPath);
        }

        public static string NewFileVolume(string FolderName, string FileName)
        {
            string fpath = logPath + FolderName + "\\";
            Directory.CreateDirectory(fpath);

            fpath += FileName;
            int i = 1;
            while (File.Exists(string.Format(fpath, i.ToString().PadLeft(4, '0'))))
                i++;
            return string.Format(fpath, i.ToString().PadLeft(4, '0'));
        }

        public static string LogFileHeader()
        {
            return
                new string('-', 70) + Environment.NewLine +
                DateTime.Now.ToString("yyyy-MM-dd---HH-mm-ss") + ": Log started" + Environment.NewLine +
                new string('-', 70) + Environment.NewLine;
        }
    }
}

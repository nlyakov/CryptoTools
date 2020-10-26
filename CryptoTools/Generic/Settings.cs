using System;
using System.IO;

namespace CryptoTools.Generic
{
    public static class Settings
    {
        static string AppSessionDate = DateTime.Now.ToString("yyyy-MM-dd @ HH-mm-ss");
        internal static string LogPath = AppDomain.CurrentDomain.BaseDirectory + "logs\\" + AppSessionDate + "\\";
        internal static int MaxLogVolume = 0x3200000;
        internal static bool SocketLogging = false;

        static Settings()
        {
            Directory.CreateDirectory(LogPath);
        }
    }
}

using Newtonsoft.Json;
using System;
using System.IO;

namespace CryptoTools.API.Toolkit
{
    public class Database
    {
        private CryptoCompare dbprovider;
        private string dbcoinfile;
        //private static string dbfile;

        public Database()
        {
            dbprovider = new CryptoCompare();

            dbcoinfile = AppDomain.CurrentDomain.BaseDirectory + "db\\coins.json";
            Directory.CreateDirectory(Path.GetDirectoryName(dbcoinfile));
        }

        private bool NeedUpdate(string dbfile)
        {
            if (File.Exists(dbfile))
            {
                FileInfo fi = new FileInfo(dbfile);
                return (DateTime.Now - fi.LastWriteTime).TotalDays > 2;
            }
            else
                return true;
        }

        private void UpdateDBCoins()
        {
            if (!NeedUpdate(dbcoinfile))
                return;

            string json = dbprovider.GetCoinList();
            dynamic obj = JsonConvert.DeserializeObject(json);
        }

        public void LoadDBCoins()
        {
            if (File.Exists(dbcoinfile))
                using (StreamReader r = new StreamReader(dbcoinfile))
                {
                    string json = r.ReadToEnd();
                    dynamic obj = JsonConvert.DeserializeObject(json);


                }
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Net;

namespace CryptoTools.API.Toolkit
{
    class CryptoCompare
    {
        protected class WebClient : System.Net.WebClient
        {
            public int Timeout { get; set; }

            public WebClient()
            {
                Timeout = 60 * 1000;
            }

            protected override WebRequest GetWebRequest(Uri uri)
            {
                HttpWebRequest request = base.GetWebRequest(uri) as HttpWebRequest; ;
                request.Timeout = Timeout;
                request.ReadWriteTimeout = Timeout;
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                return request;
            }
        }

        private string BaseUrl = "https://min-api.cryptocompare.com/data/";
        private string ImageBaseUrl = "https://www.cryptocompare.com";
        private WebClient authClient;

        private bool authenticated = false;

        public CryptoCompare()
        {
            InitAuthentication("{\"key\":\"3ee73463046a6c27727a376dbaeb4fee1599943c2edaf38cdcc098b81240c2d1\"}");
        }

        public void InitAuthentication(string auth)
        {
            authenticated = false;
            try
            {
                if (auth.Trim().Length > 0)
                {
                    dynamic authObj = JsonConvert.DeserializeObject(auth);

                    string key = authObj.key;

                    authClient = new WebClient();
                    authClient.Headers.Clear();
                    authClient.Headers[HttpRequestHeader.Accept] = "application/json";
                    authClient.Headers[HttpRequestHeader.AcceptEncoding] = "deflate,gzip";
                    authClient.Headers.Add("Apikey ", key);

                    authenticated = true;
                }
            }
            catch
            {
            }
        }

        public string GetCoinList()
        {
            if (authenticated)
                return authClient.DownloadString(BaseUrl + "all/coinlist");
            else
                return "";
        }

    }
}

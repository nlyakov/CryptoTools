using CryptoTools.API.Objects.Coins;
using Newtonsoft.Json;
using System;
using System.Net;

namespace CryptoTools.API.Toolkit
{
    public class CoinMarketCap
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

        private string BaseUrl = "https://sandbox-api.coinmarketcap.com";//"https://pro-api.coinmarketcap.com";
        private WebClient authClient;

        private bool authenticated = false;

        public CoinMarketCap()
        {

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
                    authClient.Headers[HttpRequestHeader.AcceptEncoding] = "deflate,gzip";
                    authClient.Headers[HttpRequestHeader.Accept] = "application/json";
                    authClient.Headers.Add("X-CMC_PRO_API_KEY", key);

                    authenticated = true;
                }
            }
            catch
            {
            }
        }

        public void UpdateCoinObject()
        {
            if (!authenticated)
                return;

            string resp = SendRequest("/v1/cryptocurrency/map");
            dynamic respObj = JsonConvert.DeserializeObject(resp);
            for (int i = 0; i < respObj.data.Count; i++)
            {
                CoinInfo nc = null;
                if (respObj.data[i].platform != null)
                {
                    nc = new CoinInfo() { id = respObj.data[i].platform.id, symbol = respObj.data[i].platform.symbol, fullname = respObj.data[i].platform.name };
                    Coin.AddNew(nc);
                }
                Coin.AddNew(new CoinInfo() { id = respObj.data[i].id, symbol = respObj.data[i].symbol, fullname = respObj.data[i].name, networkcoin = nc });
            }

            Coin.BuildIdList();

            resp = SendRequest("/v1/exchange/map");
            respObj = JsonConvert.DeserializeObject(resp);

            resp = SendRequest("/v1/cryptocurrency/info?id=" + Coin.idlist);
            respObj = JsonConvert.DeserializeObject(resp);
            for (int i = 0; i < respObj.data.Count; i++)
            {
                int id = respObj.data[i].id;
                CoinInfo ci = Coin.Info(id);
                if (ci != null)
                {
                    if (respObj.data[i].category == "coin")
                        ci.type = enCoinType.ctCrypto;
                }
            }

        }

        private string SendRequest(string request)
        {
            return authClient.DownloadString(BaseUrl + request);
        }

    }
}

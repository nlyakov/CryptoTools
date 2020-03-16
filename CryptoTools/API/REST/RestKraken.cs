using CryptoTools.API.Exchanges;
using CryptoTools.API.Interfaces;
using CryptoTools.API.Objects;
using CryptoTools.API.Objects.Coins;
using CryptoTools.API.Objects.OrderBooks;
using CryptoTools.API.Objects.Pairs;
using CryptoTools.Generic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace CryptoTools.API.REST
{
    class RestKraken : RestCommunicator
    {
        private HMACSHA512 hashMaker;
        private SHA256 hash;
        private WebClient authClient;

        public RestKraken(Exchange exchange) : base(exchange)
        {
            BaseUrlAuth = "https://api.kraken.com";
            BaseUrlPub = "https://api.kraken.com";
        }

        public override void InitAuthentication(string auth)
        {
            authenticated = false;
            try
            {
                WorkState = enWorkingState.wsInitialize;

                if (auth.Trim().Length > 0)
                {
                    dynamic authObj = JsonConvert.DeserializeObject(auth);

                    string key = authObj.key;
                    string secret = authObj.secret;

                    hashMaker = new HMACSHA512(Convert.FromBase64String(secret));
                    hash = SHA256Managed.Create();
                    authClient = new WebClient();
                    authClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    authClient.Headers[HttpRequestHeader.Accept] = "application/json";
                    authClient.Headers.Add("API-Key", key);
                    authClient.Headers.Add("API-Sign", "");

                    authenticated = true;
                }

                LastOperatopnTime = DateTime.Now;
                WorkState = enWorkingState.wsCompleted;
            }
            catch
            {
                WorkState = enWorkingState.wsError;
            }
        }

        public override void Initialize()
        {
            try
            {
                string resp = pubClient.DownloadString(BaseUrlPub + "/0/public/Assets");
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                int id;
                foreach (var asset in respObj.result)
                {
                    if ((string)asset.Name == (string)asset.First.altname)
                        continue;

                    if (exObj.CustomCoinNames.ContainsValue((string)asset.Name))
                        continue;

                    id = Coin.AddOrGet((string)asset.First.altname);
                    exObj.CustomCoinNames[id] = (string)asset.Name;
                }

                // Init supported pairs and margin pairs
                resp = pubClient.DownloadString(BaseUrlPub + "/0/public/AssetPairs");
                respObj = JsonConvert.DeserializeObject(resp);

                int coin1, coin2;
                string name1, name2;
                foreach (var assetObj in respObj.result)
                {
                    if ((string)assetObj.First["wsname"] == null)
                        continue;
                    name1 = assetObj.First["base"];
                    coin1 = exObj.CustomCoinNameToCoinID(name1);
                    if (coin1 == -1)
                        coin1 = Coin.AddOrGet(name1);
                    exObj.AddCoin(coin1, name1);

                    name2 = assetObj.First.quote;
                    coin2 = exObj.CustomCoinNameToCoinID(name2);
                    if (coin2 == -1)
                        coin2 = Coin.AddOrGet(name2);
                    exObj.AddCoin(coin2, name2);

                    ExchangePair p = new ExchangePair
                    {
                        coin1 = coin1,
                        coin2 = coin2,
                        name1 = name1,
                        name2 = name2,
                        pair = (string)assetObj.Name,
                        trade_supported = true,
                        margin_supported = (assetObj.First.leverage_sell.Count > 0),
                        price_precision = assetObj.First.pair_decimals,
                    };
                    exObj.pair.Add(p.UniqueID(), p);
                }

                exObj.Initialized = true;
            }
            catch
            {
                //MessageBox.Show(err);
            }
        }

        public override void UpdateTicker()
        {
            try
            {
                WorkState = enWorkingState.wsUpdateTicker;

                string pairlist = "";
                foreach (KeyValuePair<int, ExchangePair> ep in exObj.pair)
                    if (ep.Value.trade_supported)
                        pairlist += ep.Value.pair + ",";
                pairlist = pairlist.Remove(pairlist.Length - 1);
                string resp = pubClient.DownloadString(BaseUrlPub + "/0/public/Ticker?pair=" + pairlist);
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                foreach (var ticker in respObj.result)
                {
                    int idx = exObj.FindPairIdx("{0}", (string)ticker.Name);
                    if (idx < 0)
                        continue;

                    exObj.pair[idx].ticker.ask = ticker.First.a[0];
                    exObj.pair[idx].ticker.bid = ticker.First.b[0];
                    exObj.pair[idx].ticker.last = ticker.First.c[0];
                    exObj.pair[idx].TickerAvailable = true;
                }

                exObj.TickersAvailable = true;
                LastOperatopnTime = DateTime.Now;
                WorkState = enWorkingState.wsCompleted;
            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                WorkState = enWorkingState.wsError;
            }
        }

        public override void UpdateOrderBook(int pairid)
        {
            try
            {
                WorkState = enWorkingState.wsGetOrderBook;

                string resp = pubClient.DownloadString(BaseUrlPub + "/0/public/Depth?pair=" + exObj.pair[pairid].pair);
                dynamic respObj = JsonConvert.DeserializeObject(resp);
                respObj = respObj.result[exObj.pair[pairid].pair];

                ref OrderBook ob = ref exObj.pair[pairid].book;

                ob.bid.Clear();
                int bidcount = Math.Min(OrderBook.MaxCapacity, respObj.bids.Count);
                for (int i = 0; i < bidcount; i++)
                    ob.bid.AddData((double)respObj.bids[i][0], (double)respObj.bids[i][1]);

                ob.ask.Clear();
                int askcount = Math.Min(OrderBook.MaxCapacity, respObj.bids.Count);
                for (int i = 0; i < askcount; i++)
                    ob.ask.AddData((double)respObj.asks[i][0], (double)respObj.asks[i][1]);

                exObj.pair[pairid].OrderBookAvailable = true;
                LastOperatopnTime = DateTime.Now;
                WorkState = enWorkingState.wsCompleted;
            }
            catch
            {
                WorkState = enWorkingState.wsError;
            }
        }

        public override double getBalance(string cryptocoin)
        {
            string resp = SendRequest("Balance");
            dynamic respObj = JsonConvert.DeserializeObject(resp);
            if (respObj[cryptocoin] != null)
                return respObj[cryptocoin];
            else
                return 0;
        }

        public override Int64 OrderCreate(TradeObject obj, enOrderType type, double price)
        {
            try
            {
                WorkState = enWorkingState.wsOrderCreate;

                var paramlist = new ParamDict
                {
                    {"pair", exObj.pair[obj.pairid].pair },
                    {"type", type.ToString() },
                    {"ordertype", "market" },
                    {"volume", obj.amount1.ToString() }
                };

                var resp = SendRequest("AddOrder", paramlist);
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                LastOperatopnTime = DateTime.Now;
                WorkState = enWorkingState.wsCompleted;

                return (Int64)(respObj.txid.Count > 0);
            }
            catch
            {
                WorkState = enWorkingState.wsError;
                return -1;
            }
        }

        public override bool OrderCancel(Int64 orderId)
        {
            try
            {
                WorkState = enWorkingState.wsOrderCancel;

                var paramlist = new ParamDict
                {
                    { "txid", orderId.ToString(CultureInfo.InvariantCulture) }
                };
                var resp = SendRequest("CancelOrder", paramlist);
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                LastOperatopnTime = DateTime.Now;
                WorkState = enWorkingState.wsCompleted;

                return (respObj.id == orderId);
            }
            catch
            {
                WorkState = enWorkingState.wsError;
                return false;
            }
        }

        public override bool OrderCancelAll(TradeObject obj)
        {
            try
            {
                WorkState = enWorkingState.wsOrderCancelAll;

                //throw new Exception("Ami tryabva da se napravi, ama e slojno");

                LastOperatopnTime = DateTime.Now;
                WorkState = enWorkingState.wsCompleted;

                return true;
            }
            catch
            {
                WorkState = enWorkingState.wsError;
                return false;
            }
        }

        private string SendRequest(string request, ParamDict paramlist = null)
        {
            if (paramlist == null)
                paramlist = new ParamDict();

            long nonce = DateTime.UtcNow.Ticks;
            paramlist.Add("nonce", nonce.ToString(CultureInfo.InvariantCulture));

            string postData = paramlist.ToHttpPostString();
            var hash256Bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(nonce + Convert.ToChar(0) + postData));

            string path = "/0/private/" + request;
            var pathBytes = Encoding.UTF8.GetBytes(path);

            var z = new byte[pathBytes.Count() + hash256Bytes.Count()];
            pathBytes.CopyTo(z, 0);
            hash256Bytes.CopyTo(z, pathBytes.Count());

            var signature = hashMaker.ComputeHash(z);

            authClient.Headers.Set("API-Sign", Convert.ToBase64String(signature));

            return authClient.UploadString(BaseUrlAuth + path, "POST", postData);
        }

    }
}

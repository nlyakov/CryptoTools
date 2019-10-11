using CryptoTools.API.Exchanges;
using CryptoTools.API.Interfaces;
using CryptoTools.API.Objects;
using CryptoTools.API.Objects.Coins;
using CryptoTools.API.Objects.OrderBooks;
using CryptoTools.API.Objects.Pairs;
using CryptoTools.Units;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace CryptoTools.API.REST
{
    class RestCex : RestCommunicator
    {
        private HMACSHA256 hashMaker;
        private WebClient authClient;
        private string cxuser;
        private string cxkey;

        public RestCex(Exchange exchange) : base(exchange)
        {
            BaseUrlAuth = "https://cex.io/api/";
            BaseUrlPub = "https://cex.io/api/";
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

                    cxuser = authObj.user;
                    cxkey = authObj.key;
                    string secret = authObj.secret;

                    hashMaker = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
                    authClient = new WebClient();
                    authClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

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
                string resp = pubClient.DownloadString(BaseUrlPub + "currency_limits");
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                // Init supported pairs
                int coin1, coin2;
                string name1, name2;
                for (int i = 0; i < respObj.data.pairs.Count; i++)
                {
                    name1 = respObj.data.pairs[i].symbol1;
                    coin1 = exObj.CustomCoinNameToCoinID(name1);
                    if (coin1 == -1)
                        coin1 = Coin.AddOrGet(name1);
                    exObj.AddCoin(coin1, name1);

                    name2 = respObj.data.pairs[i].symbol2;
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
                        pair = name1 + "/" + name2,
                        trade_supported = true,
                        min_trade_size = respObj.data.pairs[i].minLotSize,
                    };
                    exObj.pair.Add(p.UniqueID(), p);
                }

                /*string resp = SendRequest("get_myfee/"); 
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                // Init supported pairs
                JObject jObj = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(respObj.data));
                int paircount = jObj.Count;
                _trade_pairs = new int[paircount, 2];
                int coin1, coin2, p, i = 0;
                string name1, name2, pairstr;
                foreach (var feeObj in respObj.data)
                {
                    pairstr = feeObj.Name;
                    p = pairstr.IndexOf(':');
                    if (p < 0)
                    {
                        name1 = pairstr.Substring(0, 3);
                        name2 = pairstr.Substring(3);
                    }
                    else
                    {
                        name1 = pairstr.Substring(0, p);
                        name2 = pairstr.Substring(p + 1);
                    }
                    coin1 = CustomCoinNameToCoinID(name1);
                    if (coin1 == -1)
                        coin1 = Coin.Add(name1);
                    coin2 = CustomCoinNameToCoinID(name2);
                    if (coin2 == -1)
                        coin2 = Coin.Add(name2);

                    _trade_pairs[i, 0] = coin1;
                    _trade_pairs[i, 1] = coin2;
                    i++;
                }*/

                exObj.Initialized = true;
            }
            catch
            {

            }
        }

        public override void UpdateTicker()
        {
            try
            {
                WorkState = enWorkingState.wsUpdateTicker;

                string marketlist = "";
                foreach (KeyValuePair<int, ExchangePair> ep in exObj.pair)
                    if (ep.Value.trade_supported)
                        if (!marketlist.Contains("/" + ep.Value.name2))
                            marketlist += "/" + ep.Value.name2;
                string resp = pubClient.DownloadString(BaseUrlPub + "tickers" + marketlist);
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                for (int i = 0; i < respObj.data.Count; i++)
                {
                    string tmpstr = (string)respObj.data[i].pair;
                    int idx = exObj.FindPairIdx("{0}", tmpstr.Replace(':', '/'));
                    if (idx < 0)
                        continue;

                    exObj.pair[idx].ticker.ask = respObj.data[i].ask;
                    exObj.pair[idx].ticker.bid = respObj.data[i].bid;
                    exObj.pair[idx].ticker.last = respObj.data[i].last;
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

                string resp = pubClient.DownloadString(BaseUrlPub + "order_book/" + exObj.pair[pairid].pair + "/");
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                ref OrderBook ob = ref exObj.pair[pairid].book;

                ob.bid.Clear();
                int bidcount = Math.Min(OrderBook.MaxCapacity, respObj.bids.Count);
                for (int i = 0; i < bidcount; i++)
                    ob.bid.AddData((double)respObj.bids[i][0], (double)respObj.bids[i][1]);

                ob.ask.Clear();
                int askcount = Math.Min(OrderBook.MaxCapacity, respObj.asks.Count);
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
            string resp = SendRequest("balance/");
            dynamic respObj = JsonConvert.DeserializeObject(resp);
            double rslt = 0;
            for (int i = 0; i < respObj.Count; i++)
                if (respObj[cryptocoin] != null)
                {
                    if (respObj[cryptocoin].available != null)
                        rslt += (double)respObj[cryptocoin].available;
                    if (respObj[cryptocoin].orders != null)
                        rslt += (double)respObj[cryptocoin].orders;

                    return rslt;
                }

            return 0;
        }

        public override long OrderCreate(TradeObject obj, enOrderType type, double price)
        {
            try
            {
                WorkState = enWorkingState.wsOrderCreate;

                var paramlist = new ParamDict
                {
                    {"type", type.ToString() },
                    {"amount", obj.amount1.ToString() },
                    {"price", price.ToString() }
                };

                string resp = SendRequest("place_order/" + exObj.pair[obj.pairid].pair, paramlist);
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                LastOperatopnTime = DateTime.Now;
                WorkState = enWorkingState.wsCompleted;

                return (Int64)respObj.id;
            }
            catch
            {
                WorkState = enWorkingState.wsError;
                return -1;
            }
        }

        public override bool OrderCancel(long orderId)
        {
            try
            {
                WorkState = enWorkingState.wsOrderCancel;

                var paramlist = new ParamDict
                {
                    {"id", orderId.ToString() }
                };

                string resp = SendRequest("cancel_order/", paramlist);
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                LastOperatopnTime = DateTime.Now;
                WorkState = enWorkingState.wsCompleted;

                return (respObj.ToString() == "True");
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

                string resp = SendRequest("cancel_orders/" + exObj.pair[obj.pairid].pair);
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                LastOperatopnTime = DateTime.Now;
                WorkState = enWorkingState.wsCompleted;

                return (respObj.ok.ToString() == "ok");
            }
            catch
            {
                WorkState = enWorkingState.wsError;
                return false;
            }
        }

        public string SendRequest(string request, ParamDict paramlist = null)
        {
            if (paramlist == null)
                paramlist = new ParamDict();

            string nonce = DateTime.UtcNow.Ticks.ToString();
            string signature = BitConverter.ToString(hashMaker.ComputeHash(Encoding.ASCII.GetBytes(nonce + cxuser + cxkey))).Replace("-", "");
            paramlist.Add("nonce", nonce);
            paramlist.Add("key", cxkey);
            paramlist.Add("signature", signature);

            return authClient.UploadString(BaseUrlAuth + request, paramlist.ToHttpPostString());
        }
    }
}

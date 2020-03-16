using CryptoTools.API.Exchanges;
using CryptoTools.API.Interfaces;
using CryptoTools.API.Objects;
using CryptoTools.API.Objects.Coins;
using CryptoTools.API.Objects.OrderBooks;
using CryptoTools.API.Objects.Pairs;
using CryptoTools.Generic;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace CryptoTools.API.REST
{
    class RestBittrex : RestCommunicator
    {
        private HMACSHA512 hashMaker;
        private HttpClient authClient;
        private string btkey;

        public RestBittrex(Exchange exchange) : base(exchange)
        {
            BaseUrlAuth = "https://bittrex.com/api/v1.1/";
            BaseUrlPub = "https://bittrex.com/api/v1.1/";
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

                    btkey = authObj.key;
                    string secret = authObj.secret;

                    hashMaker = new HMACSHA512(Encoding.ASCII.GetBytes(secret));
                    authClient = new HttpClient();
                    //authClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    //authClient.Headers[HttpRequestHeader.Accept] = "application/json";
                    //authClient.Headers.Add("apisign", "");

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
                string resp = pubClient.DownloadString(BaseUrlPub + "public/getmarkets");
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                // Init supported pairs
                int coin1, coin2;
                string name1, name2;
                for (int i = 0; i < respObj.result.Count; i++)
                {
                    name1 = respObj.result[i].MarketCurrency;
                    name2 = respObj.result[i].BaseCurrency;

                    coin1 = exObj.CustomCoinNameToCoinID(name1);
                    if (coin1 == -1)
                        coin1 = Coin.AddOrGet(name1);
                    exObj.AddCoin(coin1, name1);

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
                        pair = (string)respObj.result[i].MarketName,
                        trade_supported = ((bool)respObj.result[i].IsActive && !(bool)respObj.result[i].IsRestricted),
                        min_trade_size = ((string)respObj.result[i].MinTradeSize).ToDouble(0),
                    };
                    exObj.pair.Add(p.UniqueID(), p);
                }
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

                string resp = pubClient.DownloadString(BaseUrlPub + "public/getmarketsummaries");
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                for (int i = 0; i < respObj.result.Count; i++)
                {
                    int idx = exObj.FindPairIdx("{0}", (string)respObj.result[i].MarketName);
                    if (idx < 0)
                        continue;

                    exObj.pair[idx].ticker.ask = respObj.result[i].Ask;
                    exObj.pair[idx].ticker.bid = respObj.result[i].Bid;
                    exObj.pair[idx].ticker.last = respObj.result[i].Last;
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

                string resp = pubClient.DownloadString(BaseUrlPub + "public/getorderbook?market=" + exObj.pair[pairid].pair + "&type=both");
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                ref OrderBook ob = ref exObj.pair[pairid].book;

                ob.bid.Clear();
                int bidcount = Math.Min(OrderBook.MaxCapacity, respObj.result.buy.Count);
                for (int i = 0; i < bidcount; i++)
                    ob.bid.AddData((double)respObj.result.buy[i].Rate, (double)respObj.result.buy[i].Quantity);

                ob.ask.Clear();
                int askcount = Math.Min(OrderBook.MaxCapacity, respObj.result.sell.Count);
                for (int i = 0; i < askcount; i++)
                    ob.ask.AddData((double)respObj.result.sell[i].Rate, (double)respObj.result.sell[i].Quantity);

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
            var paramlist = new ParamDict
            {
                { "currency", cryptocoin.ToUpper() }
            };
            string resp = SendRequest("account/getbalance", HttpMethod.Get, paramlist);
            dynamic respObj = JsonConvert.DeserializeObject(resp);
            return respObj.result.Balance;
            /*for (int i=0; i<respObj.result.Count; i++)
            {
                if (respObj.result[i].Currency == cryptocoin.ToUpper())
                    return respObj.result[i].Balance;
            }
            return 0;*/
        }

        public override long OrderCreate(TradeObject obj, enOrderType type, double price)
        {
            try
            {
                WorkState = enWorkingState.wsOrderCreate;

                var paramlist = new ParamDict
                {
                    {"market", exObj.pair[obj.pairid].pair },
                    {"quantity", obj.amount1.ToString() },
                    {"rate", price.ToString() }
                };

                string resp = SendRequest("market/" + type.ToString() + "limit", paramlist);
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                LastOperatopnTime = DateTime.Now;
                WorkState = enWorkingState.wsCompleted;

                return (Int64)(respObj.result.uuid.Length > 0);
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
                    {"uuid", orderId.ToString() }
                };
                var resp = SendRequest("market/cancel", paramlist);
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                LastOperatopnTime = DateTime.Now;
                WorkState = enWorkingState.wsCompleted;

                return (respObj.success == 1);
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
            return SendRequest(request, HttpMethod.Post, paramlist);
        }

        private string SendRequest(string request, HttpMethod method, ParamDict paramlist = null)
        {
            if (paramlist == null)
                paramlist = new ParamDict();

            paramlist.Add("apikey", btkey);
            paramlist.Add("nonce", DateTime.UtcNow.Ticks.ToString());

            var Url = BaseUrlAuth + request + "?" + paramlist.ToHttpPostString();
            string signature = BitConverter.ToString(hashMaker.ComputeHash(Encoding.ASCII.GetBytes(Url))).Replace("-", "");

            var req = new HttpRequestMessage(method, Url);
            req.Headers.Add("apisign", signature);

            var resp = authClient.SendAsync(req).Result;
            return resp.Content.ReadAsStringAsync().Result;
        }
    }
}

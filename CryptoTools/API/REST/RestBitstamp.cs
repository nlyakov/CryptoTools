using CryptoTools.API.Exchanges;
using CryptoTools.API.Interfaces;
using CryptoTools.API.Objects;
using CryptoTools.API.Objects.Coins;
using CryptoTools.API.Objects.OrderBooks;
using CryptoTools.API.Objects.Pairs;
using CryptoTools.Generic.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace CryptoTools.API.REST
{
    class RestBitstamp : RestCommunicator
    {
        private HMACSHA256 hashMaker;
        private HttpClient authClient;
        private string bsclientid;
        private string bskey;

        public RestBitstamp(Exchange exchange) : base(exchange)
        {
            BaseUrlAuth = "https://www.bitstamp.net/api/";
            BaseUrlPub = "https://www.bitstamp.net/api/";
        }

        public override void InitAuthentication(string auth)
        {
            authenticated = false;
            try
            {
                WorkState = enWorkingState.wsInitialize;

                if (auth.Trim().Length > 0)
                {
                    dynamic authObj = auth.ToDynamic();

                    bsclientid = authObj.user;
                    bskey = authObj.key;
                    string secret = authObj.secret;

                    hashMaker = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
                    authClient = new HttpClient();
                    authClient.DefaultRequestHeaders.Add("Accept", "application/json");

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
                string resp = pubClient.DownloadString(BaseUrlPub + "v2/trading-pairs-info/");
                dynamic respObj = resp.ToDynamic();

                // Init supported pairs
                int coin1, coin2, k;
                string name1, name2, pairstr;
                for (int i = 0; i < respObj.Count; i++)
                {
                    pairstr = (string)respObj[i].name;
                    k = pairstr.IndexOf('/');
                    if (k < 0)
                    {
                        name1 = pairstr.Substring(0, 3);
                        name2 = pairstr.Substring(3);
                    }
                    else
                    {
                        name1 = pairstr.Substring(0, k);
                        name2 = pairstr.Substring(k + 1);
                    }

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
                        pair = (string)respObj[i].url_symbol,
                        trade_supported = true,
                        qty_precision = respObj[i].base_decimals,
                        price_precision = respObj[i].counter_decimals,
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

                foreach (KeyValuePair<int, ExchangePair> ep in exObj.pair)
                {
                    if (!ep.Value.trade_supported)
                        continue;

                    string resp = pubClient.DownloadString(BaseUrlPub + "v2/ticker/" + ep.Value.pair + "/");
                    dynamic respObj = resp.ToDynamic();

                    ep.Value.ticker.ask = respObj.ask;
                    ep.Value.ticker.bid = respObj.bid;
                    ep.Value.ticker.last = respObj.last;
                    ep.Value.TickerAvailable = true;
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

                string resp = pubClient.DownloadString(BaseUrlPub + "v2/order_book/" + exObj.pair[pairid].pair + "/");
                dynamic respObj = resp.ToDynamic();

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
            string resp = SendRequest("v2/balance/");
            string balkey = cryptocoin.ToLower() + "_available";
            dynamic respObj = resp.ToDynamic();
            if (respObj[balkey] != null)
                return respObj[balkey];
            else
                return 0;
        }

        public override long OrderCreate(TradeObject obj, enOrderType type, double price)
        {
            try
            {
                WorkState = enWorkingState.wsOrderCreate;

                var paramlist = new ParamDict
                {
                    {"amount", obj.amount1.ToString(CultureInfo.InvariantCulture) }
                };

                string resp = SendRequest("v2/" + type.ToString() + "/market/" + exObj.pair[obj.pairid].pair.ToLower() + "/", paramlist);
                dynamic respObj = resp.ToDynamic();

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
                    {"id", orderId.ToString(CultureInfo.InvariantCulture) }
                };
                var resp = SendRequest("cancel_order/", paramlist);
                dynamic respObj = resp.ToDynamic();

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

                string resp = SendRequest("cancel_all_orders/");
                dynamic respObj = resp.ToDynamic();

                LastOperatopnTime = DateTime.Now;
                WorkState = enWorkingState.wsCompleted;

                return (respObj.result == "true");
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
            var msg = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", nonce, bsclientid, bskey);
            string signature = BitConverter.ToString(hashMaker.ComputeHash(Encoding.UTF8.GetBytes(msg))).Replace("-", "").ToUpper();

            paramlist.Add("key", bskey);
            paramlist.Add("signature", signature);
            paramlist.Add("nonce", nonce.ToString(CultureInfo.InvariantCulture));

            var httpContent = new FormUrlEncodedContent(paramlist);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var response = authClient.PostAsync(BaseUrlAuth + request, httpContent).Result;
            return response.Content.ReadAsStringAsync().Result;
        }
    }
}

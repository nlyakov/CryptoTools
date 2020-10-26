using CryptoTools.API.Exchanges;
using CryptoTools.API.Interfaces;
using CryptoTools.API.Objects;
using CryptoTools.API.Objects.Coins;
using CryptoTools.API.Objects.OrderBooks;
using CryptoTools.API.Objects.Pairs;
using CryptoTools.Generic.Helpers;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace CryptoTools.API.REST
{
    class RestPoloniex : RestCommunicator
    {
        private HMACSHA512 hashMaker;
        private WebClient authClient;

        public RestPoloniex(Exchange exchange) : base(exchange)
        {
            BaseUrlAuth = "https://poloniex.com/";
            BaseUrlPub = "https://poloniex.com/";
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

                    string key = authObj.key;
                    string secret = authObj.secret;

                    hashMaker = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
                    authClient = new WebClient();
                    authClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    authClient.Headers[HttpRequestHeader.Accept] = "application/json";
                    authClient.Headers.Add("Key", key);
                    authClient.Headers.Add("Sign", "");

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
                string resp = pubClient.DownloadString(BaseUrlPub + "public?command=returnCurrencies");
                dynamic respObj = resp.ToDynamic();

                int id;
                foreach (var coinObj in respObj)
                {
                    id = exObj.CustomCoinNameToCoinID((string)coinObj.Name);
                    if (id == -1)
                        id = Coin.AddOrGet((string)coinObj.Name);

                    ExchangeCoin ec = exObj.AddCoin(id, (string)coinObj.Name);
                    if (ec != null)
                    {
                        ec.confirmations = coinObj.First.minConf;
                        ec.depositaddr = coinObj.First.depositAddress;
                        ec.fee = coinObj.First.txFee;
                        ec.candeposit = (coinObj.First.disabled == 0 && coinObj.First.delisted == 0 && coinObj.First.frozen == 0);
                        ec.canwithdraw = ec.candeposit;
                    }
                }

                // Init supported pairs
                resp = pubClient.DownloadString(BaseUrlPub + "public?command=returnTicker");
                respObj = resp.ToDynamic();
                int coin1, coin2, k;
                string name1, name2, pairstr;
                foreach (var pairObj in respObj)
                {
                    pairstr = (string)pairObj.Name;
                    k = pairstr.IndexOf('_');
                    if (k < 0)
                    {
                        name2 = pairstr.Substring(0, 3);
                        name1 = pairstr.Substring(3);
                    }
                    else
                    {
                        name2 = pairstr.Substring(0, k);
                        name1 = pairstr.Substring(k + 1);
                    }
                    coin1 = exObj.CustomCoinNameToCoinID(name1);
                    if (coin1 == -1)
                        coin1 = Coin.AddOrGet(name1);

                    coin2 = exObj.CustomCoinNameToCoinID(name2);
                    if (coin2 == -1)
                        coin2 = Coin.AddOrGet(name2);

                    ExchangePair p = new ExchangePair
                    {
                        coin1 = coin1,
                        coin2 = coin2,
                        name1 = name1,
                        name2 = name2,
                        pair = pairstr,
                        trade_supported = (pairObj.First.isFrozen == 0),
                        coin_volume = ((string)pairObj.First.quoteVolume).ToDouble(-1),
                        ref_volume = ((string)pairObj.First.baseVolume).ToDouble(-1),
                    };
                    exObj.pair.Add(p.UniqueID(), p);
                }

                exObj.Initialized = true;
            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }

        public override void UpdateTicker()
        {
            try
            {
                WorkState = enWorkingState.wsUpdateTicker;

                string resp = pubClient.DownloadString(BaseUrlPub + "public?command=returnTicker");
                dynamic respObj = resp.ToDynamic();

                foreach (var pairObj in respObj)
                {
                    int idx = exObj.FindPairIdx("{0}", (string)pairObj.Name);
                    if (idx < 0)
                        continue;

                    exObj.pair[idx].ticker.ask = pairObj.First.lowestAsk;
                    exObj.pair[idx].ticker.bid = pairObj.First.highestBid;
                    exObj.pair[idx].ticker.last = pairObj.First.last;
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

                string resp = pubClient.DownloadString(BaseUrlPub + "public?command=returnOrderBook&currencyPair=" + exObj.pair[pairid].pair);
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
            string resp = SendRequest("returnBalances");
            dynamic respObj = resp.ToDynamic();
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
                    {"currencyPair", exObj.pair[obj.pairid].pair },
                    {"rate", price.ToString() },
                    {"amount", obj.amount1.ToString() }
                };

                string resp = SendRequest(type.ToString(), paramlist);
                dynamic respObj = resp.ToDynamic();

                LastOperatopnTime = DateTime.Now;
                WorkState = enWorkingState.wsCompleted;

                return (Int64)respObj.orderNumber;
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
                    {"orderNumber", orderId.ToString() }
                };
                var resp = SendRequest("cancelOrder", paramlist);
                dynamic respObj = resp.ToDynamic();


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
            if (paramlist == null)
                paramlist = new ParamDict();

            paramlist.Add("nonce", DateTime.UtcNow.Ticks.ToString());
            paramlist.Add("command", request);

            var data = paramlist.ToHttpPostString();
            string signature = BitConverter.ToString(hashMaker.ComputeHash(Encoding.UTF8.GetBytes(data))).Replace("-", "").ToLower();

            authClient.Headers.Set("Sign", signature);

            return authClient.UploadString(BaseUrlAuth + "tradingApi", data);
        }
    }
}

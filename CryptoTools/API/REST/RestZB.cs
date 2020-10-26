using CryptoTools.API.Exchanges;
using CryptoTools.API.Interfaces;
using CryptoTools.API.Objects;
using CryptoTools.API.Objects.Coins;
using CryptoTools.API.Objects.OrderBooks;
using CryptoTools.API.Objects.Pairs;
using CryptoTools.Generic.Helpers;
using System;

namespace CryptoTools.API.REST
{
    class RestZB : RestCommunicator
    {
        public RestZB(Exchange exchange) : base(exchange)
        {
            BaseUrlAuth = "http://api.zb.cn/data/";
            BaseUrlPub = "http://api.zb.cn/data/";
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

                    //authenticated = true;
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
                string resp = pubClient.DownloadString(BaseUrlPub + "v1/markets");
                dynamic respObj = resp.ToDynamic();

                // Init supported pairs and margin pairs
                int k;
                int coin1, coin2;
                string name1, name2, pairstr;
                foreach (var assetObj in respObj)
                {
                    pairstr = (string)assetObj.Name;
                    k = pairstr.IndexOf('_');
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
                        pair = pairstr,
                        trade_supported = true,
                        price_precision = assetObj.First.priceScale,
                        qty_precision = assetObj.First.amountScale,
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

                string resp = pubClient.DownloadString(BaseUrlPub + "v1/allTicker");
                dynamic respObj = resp.ToDynamic();

                foreach (var pairObj in respObj)
                {
                    int idx = exObj.FindPairIdx("{1}{2}", ((string)pairObj.Name).ToUpper());
                    if (idx < 0)
                        continue;

                    exObj.pair[idx].ticker.ask = pairObj.First.sell;
                    exObj.pair[idx].ticker.askqty = 0;
                    exObj.pair[idx].ticker.bid = pairObj.First.buy;
                    exObj.pair[idx].ticker.bidqty = 0;
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

                string resp = pubClient.DownloadString(BaseUrlPub + "v1/depth?market=" + exObj.pair[pairid].pair);
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
            return 0;
        }

        public override Int64 OrderCreate(TradeObject obj, enOrderType type, double price)
        {
            try
            {
                WorkState = enWorkingState.wsOrderCreate;

                //ToDo

                LastOperatopnTime = DateTime.Now;
                WorkState = enWorkingState.wsCompleted;

                return 0;
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

                //ToDo

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

        public override bool OrderCancelAll(TradeObject obj)
        {
            try
            {
                WorkState = enWorkingState.wsOrderCancelAll;

                //ToDo

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
            return "";
        }

        private string SendRequest(string request, string method, ParamDict paramlist = null)
        {
            return "";
        }
    }
}

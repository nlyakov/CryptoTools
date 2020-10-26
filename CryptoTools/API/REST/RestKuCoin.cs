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
    class RestKuCoin : RestCommunicator
    {
        public RestKuCoin(Exchange exchange) : base(exchange)
        {
            BaseUrlAuth = "https://api.kucoin.com/api/";
            BaseUrlPub = "https://api.kucoin.com/api/";
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
                string resp = pubClient.DownloadString(BaseUrlPub + "v1/symbols");
                dynamic respObj = resp.ToDynamic();

                // Init supported pairs and margin pairs
                int coin1, coin2;
                string name1, name2;
                for (int i = 0; i < respObj.data.Count; i++)
                {
                    name1 = respObj.data[i].baseCurrency;
                    coin1 = exObj.CustomCoinNameToCoinID(name1);
                    if (coin1 == -1)
                        coin1 = Coin.AddOrGet(name1);
                    exObj.AddCoin(coin1, name1);

                    name2 = respObj.data[i].quoteCurrency;
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
                        pair = (string)respObj.data[i].symbol,
                        trade_supported = ((bool)respObj.data[i].enableTrading),
                        min_trade_size = respObj.data[i].quoteMinSize,
                        max_trade_size = respObj.data[i].quoteMaxSize,
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

                string resp = pubClient.DownloadString(BaseUrlPub + "v1/market/allTickers");
                dynamic respObj = resp.ToDynamic();

                for (int i = 0; i < respObj.data.ticker.Count; i++)
                {
                    int idx = exObj.FindPairIdx("{0}", (string)respObj.data.ticker[i].symbol);
                    if (idx < 0)
                        continue;

                    exObj.pair[idx].ticker.ask = respObj.data.ticker[i].sell;
                    exObj.pair[idx].ticker.askqty = 0;
                    exObj.pair[idx].ticker.bid = respObj.data.ticker[i].buy;
                    exObj.pair[idx].ticker.bidqty = 0;
                    exObj.pair[idx].ticker.last = respObj.data.ticker[i].last;
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

                string resp = pubClient.DownloadString(BaseUrlPub + "v2/market/orderbook/level2?symbol=" + exObj.pair[pairid].pair);
                dynamic respObj = resp.ToDynamic();

                ref OrderBook ob = ref exObj.pair[pairid].book;

                ob.bid.Clear();
                int bidcount = Math.Min(OrderBook.MaxCapacity, respObj.data.bids.Count);
                for (int i = 0; i < bidcount; i++)
                    ob.bid.AddData((double)respObj.data.bids[i][0], (double)respObj.data.bids[i][1]);

                ob.ask.Clear();
                int askcount = Math.Min(OrderBook.MaxCapacity, respObj.data.asks.Count);
                for (int i = 0; i < askcount; i++)
                    ob.ask.AddData((double)respObj.data.asks[i][0], (double)respObj.data.asks[i][1]);

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

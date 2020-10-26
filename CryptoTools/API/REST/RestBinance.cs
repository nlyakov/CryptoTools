using CryptoTools.API.Exchanges;
using CryptoTools.API.Interfaces;
using CryptoTools.API.Objects;
using CryptoTools.API.Objects.Coins;
using CryptoTools.API.Objects.OrderBooks;
using CryptoTools.API.Objects.Pairs;
using CryptoTools.Generic.Helpers;
using System;
using System.Windows;

namespace CryptoTools.API.REST
{
    class RestBinance : RestCommunicator
    {
        public RestBinance(Exchange exchange) : base(exchange)
        {
            BaseUrlAuth = "https://api.binance.com/";
            BaseUrlPub = "https://api.binance.com/";
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
            int i=0;
            try
            {
                string resp = pubClient.DownloadString(BaseUrlPub + "api/v1/exchangeInfo");
                dynamic respObj = resp.ToDynamic();

                // Init supported pairs and margin pairs
                int coin1, coin2;
                string name1, name2;
                for (i = 0; i < respObj.symbols.Count; i++)
                {
                    if ((string)respObj.symbols[i].status == "BREAK")
                        continue;

                    name1 = respObj.symbols[i].baseAsset;
                    name2 = respObj.symbols[i].quoteAsset;

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
                        pair = (string)respObj.symbols[i].symbol,
                        qty_precision = respObj.symbols[i].baseAssetPrecision,
                        price_precision = respObj.symbols[i].quotePrecision,
                        trade_supported = ((bool)respObj.symbols[i].isSpotTradingAllowed && (string)respObj.symbols[i].status == "TRADING"),
                        margin_supported = (bool)respObj.symbols[i].isMarginTradingAllowed,
                        iceberg_supported = (bool)respObj.symbols[i].icebergAllowed,
                    };
                    exObj.pair.Add(p.UniqueID(), p);
                }

                exObj.Initialized = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(i.ToString() + ": " + e.Message);
            }
        }

        public override void UpdateTicker()
        {
            try
            {
                WorkState = enWorkingState.wsUpdateTicker;

                string resp = pubClient.DownloadString(BaseUrlPub + "api/v3/ticker/bookTicker");
                dynamic respObj = resp.ToDynamic();

                for (int i = 0; i < respObj.Count; i++)
                {
                    int idx = exObj.FindPairIdx("{0}", (string)respObj[i].symbol);
                    if (idx < 0)
                        continue;

                    exObj.pair[idx].ticker.ask = respObj[i].askPrice;
                    exObj.pair[idx].ticker.askqty = respObj[i].askQty;
                    exObj.pair[idx].ticker.bid = respObj[i].bidPrice;
                    exObj.pair[idx].ticker.bidqty = respObj[i].bidQty;
                    exObj.pair[idx].ticker.last = 0;
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

                string resp = pubClient.DownloadString(BaseUrlPub + "api/v1/depth?symbol=" + exObj.pair[pairid].pair);
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

        public override long OrderCreate(TradeObject obj, enOrderType type, double price)
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

        public override bool OrderCancel(long orderID)
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

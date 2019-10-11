using CryptoTools.API.Exchanges;
using CryptoTools.API.Interfaces;
using CryptoTools.API.Objects;
using CryptoTools.API.Objects.Coins;
using CryptoTools.API.Objects.OrderBooks;
using CryptoTools.API.Objects.Pairs;
using Newtonsoft.Json;
using System;

namespace CryptoTools.API.REST
{
    class RestIdax : RestCommunicator
    {
        public RestIdax(Exchange exchange) : base(exchange)
        {
            BaseUrlAuth = "https://openapi.idax.pro/api/";
            BaseUrlPub = "https://openapi.idax.pro/api/";
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
                string resp = pubClient.DownloadString(BaseUrlPub + "v2/pairList");
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                // Init supported pairs and margin pairs
                int coin1, coin2;
                string name1, name2;
                for (int i = 0; i < respObj.pairList.Count; i++)
                {
                    name1 = respObj.pairList[i].baseCoinCode;
                    coin1 = exObj.CustomCoinNameToCoinID(name1);
                    if (coin1 == -1)
                        coin1 = Coin.AddOrGet(name1);
                    exObj.AddCoin(coin1, name1);

                    name2 = respObj.pairList[i].quoteCoinCode;
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
                        pair = (string)respObj.pairList[i].pairName,
                        trade_supported = ((string)respObj.pairList[i].status == "Open"),
                        min_trade_size = respObj.pairList[i].minAmount,
                        max_trade_size = respObj.pairList[i].maxAmount,
                        price_precision = respObj.pairList[i].priceDecimalPlace,
                        qty_precision = respObj.pairList[i].qtyDecimalPlace,
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

                string resp = pubClient.DownloadString(BaseUrlPub + "v2/depth?pair=" + exObj.pair[pairid].pair);
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

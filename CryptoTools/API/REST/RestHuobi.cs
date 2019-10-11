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
    class RestHuobi : RestCommunicator
    {
        public RestHuobi(Exchange exchange) : base(exchange)
        {
            BaseUrlAuth = "https://api.huobi.pro";
            BaseUrlPub = "https://api.huobi.pro";
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
                string resp = pubClient.DownloadString(BaseUrlPub + "/v1/common/symbols");
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                // Init supported pairs and margin pairs
                int coin1, coin2;
                string name1, name2;
                for (int i = 0; i < respObj.data.Count; i++)
                {
                    name1 = (string)respObj.data[i]["base-currency"];
                    coin1 = exObj.CustomCoinNameToCoinID(name1);
                    if (coin1 == -1)
                        coin1 = Coin.AddOrGet(name1);
                    exObj.AddCoin(coin1, name1);

                    name2 = (string)respObj.data[i]["quote-currency"];
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
                        trade_supported = ((string)respObj.data[i].state == "online"),
                        margin_supported = (respObj.data[i]["leverage-ratio"] != null),
                        min_trade_size = respObj.data[i]["min-order-amt"],
                        max_trade_size = respObj.data[i]["max-order-amt"],
                        min_trade_value = respObj.data[i]["min-order-value"],
                        price_precision = respObj.data[i]["price-precision"],
                        qty_precision = respObj.data[i]["amount-precision"],
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
                string resp = pubClient.DownloadString(BaseUrlPub + "/market/depth?symbol=" + exObj.pair[pairid].pair + "&type=step0");
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                ref OrderBook ob = ref exObj.pair[pairid].book;

                ob.bid.Clear();
                int bidcount = Math.Min(OrderBook.MaxCapacity, respObj.tick.bids.Count);
                for (int i = 0; i < bidcount; i++)
                    ob.bid.AddData((double)respObj.tick.bids[i][0], (double)respObj.tick.bids[i][1]);

                ob.ask.Clear();
                int askcount = Math.Min(OrderBook.MaxCapacity, respObj.tick.asks.Count);
                for (int i = 0; i < askcount; i++)
                    ob.ask.AddData((double)respObj.tick.asks[i][0], (double)respObj.tick.asks[i][1]);

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

using CryptoTools.API.Exchanges;
using CryptoTools.API.Interfaces;
using CryptoTools.API.Objects;
using CryptoTools.API.Objects.Coins;
using CryptoTools.API.Objects.OrderBooks;
using CryptoTools.API.Objects.Pairs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace CryptoTools.API.REST
{
    class RestBitfinex : RestCommunicator
    {
        private HMACSHA384 hashMaker;
        private WebClient authClient;

        public RestBitfinex(Exchange exchange) : base(exchange)
        {
            BaseUrlAuth = "https://api.bitfinex.com";
            BaseUrlPub = "https://api-pub.bitfinex.com/v2";
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

                    hashMaker = new HMACSHA384(Encoding.UTF8.GetBytes(secret));
                    authClient = new WebClient();
                    authClient.Headers.Clear();
                    authClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                    authClient.Headers[HttpRequestHeader.Accept] = "application/json";
                    authClient.Headers.Add("X-BFX-APIKEY", key);
                    authClient.Headers.Add("X-BFX-PAYLOAD", "");
                    authClient.Headers.Add("X-BFX-SIGNATURE", "");

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
                string resp = pubClient.DownloadString(BaseUrlPub + "/conf/pub:map:currency:sym,pub:list:pair:exchange,pub:list:pair:margin,pub:map:currency:pool");
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                int id;
                string symbol;
                for (int i = 0; i < respObj[0].Count; i++)
                {
                    symbol = (string)respObj[0][i][1];
                    id = Coin.AddOrGet(symbol);
                    exObj.CustomCoinNames[id] = (string)respObj[0][i][0];
                }

                // Init supported pairs
                int coin1, coin2, k;
                string name1, name2, pairstr;
                for (int i = 0; i < respObj[1].Count; i++)
                {
                    pairstr = (string)respObj[1][i];
                    k = pairstr.IndexOf(':');
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
                    };
                    exObj.pair.Add(p.UniqueID(), p);
                }

                // Init supported margin pairs
                for (int i = 0; i < respObj[2].Count; i++)
                {
                    pairstr = (string)respObj[2][i];
                    foreach (KeyValuePair<int, ExchangePair> p in exObj.pair)
                        if (p.Value.pair == pairstr)
                            p.Value.margin_supported = true;
                }

                exObj.Initialized = true;
            }
            catch
            {
                WorkState = enWorkingState.wsError;
            }
        }

        public override void UpdateTicker()
        {
            try
            {
                WorkState = enWorkingState.wsUpdateTicker;

                string resp = pubClient.DownloadString(BaseUrlPub + "/tickers?symbols=ALL");
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                for (int i = 0; i < respObj.Count; i++)
                {
                    int idx = exObj.FindPairIdx("t{0}", (string)respObj[i][0]);
                    if (idx < 0)
                        continue;

                    exObj.pair[idx].ticker.ask = respObj[i][3];
                    exObj.pair[idx].ticker.bid = respObj[i][1];
                    exObj.pair[idx].ticker.last = respObj[i][7];
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
                System.Threading.Thread.Sleep(2000 - (int)Math.Min(2000, Math.Max(0, (DateTime.Now - LastOperatopnTime).TotalMilliseconds)));
                WorkState = enWorkingState.wsGetOrderBook;

                string resp = pubClient.DownloadString(BaseUrlPub + "/book/t" + exObj.pair[pairid].pair + "/P0?len=" + OrderBook.MaxCapacity.ToString());
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                ref OrderBook ob = ref exObj.pair[pairid].book;
                ob.bid.Clear();
                ob.ask.Clear();
                int bidcount = 0;
                int askcount = 0;
                for (int i = 0; i < respObj.Count; i++)
                {
                    if ((double)respObj[i][2] > 0)
                    {
                        if (bidcount == OrderBook.MaxCapacity)
                            continue;

                        ob.bid.AddData(Math.Abs((double)respObj[i][0]), Math.Abs((double)respObj[i][2]));
                        bidcount++;
                    }
                    else
                    {
                        if (askcount == OrderBook.MaxCapacity)
                            continue;

                        ob.ask.AddData(Math.Abs((double)respObj[i][0]), Math.Abs((double)respObj[i][2]));
                        askcount++;
                    }
                }

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
            string resp = SendRequest("/balances");
            dynamic respObj = JsonConvert.DeserializeObject(resp);
            for (int i = 0; i < respObj.Count; i++)
                if (respObj[i].type == "exchange" && respObj[i].currency == cryptocoin.ToLower())
                    return (double)respObj[i].available;

            return 0;
        }

        public override long OrderCreate(TradeObject obj, enOrderType type, double price)
        {
            try
            {
                WorkState = enWorkingState.wsOrderCreate;

                var paramlist = new ParamDict
                {
                    {"symbol", exObj.pair[obj.pairid].pair },
                    {"amount", obj.amount1.ToString() },
                    {"price", price.ToString() },
                    {"exchange", "bitfinex" },
                    {"side", type.ToString() },
                    {"type", "exchange market" }
                };

                string resp = SendRequest("/v1/order/new", paramlist);
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                LastOperatopnTime = DateTime.Now;
                WorkState = enWorkingState.wsCompleted;

                return (Int64)respObj.order_id;
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

                string resp = SendRequest("/v1/order/cancel", paramlist);
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                LastOperatopnTime = DateTime.Now;
                WorkState = enWorkingState.wsCompleted;

                return (!(bool)respObj.is_live && (bool)respObj.is_cancelled);
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

                string resp = SendRequest("/v1/order/cancel/all", "GET");
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                LastOperatopnTime = DateTime.Now;
                WorkState = enWorkingState.wsCompleted;

                return (respObj.result == "All orders cancelled");
            }
            catch
            {
                WorkState = enWorkingState.wsError;
                return false;
            }
        }

        /*public bool Withdraw(int coin)
        {
            try
            {
                WorkState = enWorkingState.wsWithdraw;

                var paramlist = new ParamDict
                {
                    { "withdraw_type", "bgold" },
                    { "walletselected", "exchange" },
                    { "amount", "89.819" },
                    { "address", "AS5pbHcwtAt5J3mJ3zbrQjyUHvz97VfXLU" }
                };
                string resp = SendRequest("/v1/withdraw", paramlist);
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                LastOperatopnTime = DateTime.Now;
                WorkState = enWorkingState.wsCompleted;

                return (respObj.withdrawal_id != 0);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                WorkState = enWorkingState.wsError;
                return false;
            }
        }

        public bool ApiKeyInfo()
        {
            try
            {
                WorkState = enWorkingState.wsWithdraw;

                string resp = SendRequest("/v1/key_info");
                dynamic respObj = JsonConvert.DeserializeObject(resp);

                LastOperatopnTime = DateTime.Now;
                WorkState = enWorkingState.wsCompleted;

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                WorkState = enWorkingState.wsError;
                return false;
            }
        }*/

        private string SendRequest(string request, ParamDict paramlist = null)
        {
            return SendRequest(request, "POST", paramlist);
        }

        private string SendRequest(string request, string method, ParamDict paramlist = null)
        {
            if (paramlist == null)
                paramlist = new ParamDict();

            paramlist.Add("nonce", DateTime.UtcNow.Ticks.ToString());
            paramlist.Add("request", request);

            string httpContent = JsonConvert.SerializeObject(paramlist);
            string payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(httpContent));
            string signature = BitConverter.ToString(hashMaker.ComputeHash(Encoding.UTF8.GetBytes(payload))).Replace("-", "").ToLower();

            authClient.Headers.Set("X-BFX-PAYLOAD", payload);
            authClient.Headers.Set("X-BFX-SIGNATURE", signature);
            return authClient.UploadString(BaseUrlAuth + request, method, httpContent);
        }

    }
}

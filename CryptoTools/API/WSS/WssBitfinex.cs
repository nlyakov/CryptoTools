using CryptoTools.API.Exchanges;
using CryptoTools.API.Interfaces;
using CryptoTools.API.Objects.OrderBooks;
using CryptoTools.API.Objects.Pairs;
using CryptoTools.Generic.Helpers;
using System;
using System.Security.Cryptography;
using System.Text;

namespace CryptoTools.API.WSS
{
    class WssBitfinex : WssCommunicator
    {
        private HMACSHA384 hashMaker;
        private dynamic authobj;

        public WssBitfinex(Exchange exchange) : base(exchange)
        {
            SubscriptionsPerSocket = 40;
            BaseUrlAuth = "wss://api.bitfinex.com/ws/2";
            BaseUrlPub = "wss://api-pub.bitfinex.com/ws/2";
            SocketConnectedHandler += InitSubscriptions;
        }

        public override void InitAuthentication(string auth)
        {
            authinfo = false;
            try
            {
                WorkState = enWorkingState.wsInitialize;
                if (auth.Trim().Length > 0)
                {
                    dynamic authObj = auth.ToDynamic();

                    string key = authObj.key;
                    string secret = authObj.secret;

                    hashMaker = new HMACSHA384(Encoding.UTF8.GetBytes(secret));

                    authobj = new JsonObject()
                    {
                        { "apiKey", key },
                        { "event", "auth" },
                    };

                    authinfo = true;
                }

                LastOperatopnTime = DateTime.Now;
                WorkState = enWorkingState.wsCompleted;
            }
            catch
            {
                WorkState = enWorkingState.wsError;
            }
        }

        internal override string BuildAuthenticationRequest()
        {
            long nonce = DateTime.UtcNow.Ticks;
            string payload = "AUTH" + nonce.ToString();
            string signature = BitConverter.ToString(hashMaker.ComputeHash(Encoding.UTF8.GetBytes(payload))).Replace("-", "").ToLower();

            authobj.authNonce = nonce;
            authobj.authSig = signature;
            authobj.authPayload = payload;

            return JsonHelper.ToJson(authobj);
        }

        protected override string BuildSubscriptionRequest(ExchangePair ep)
        {
            dynamic pairObj = new JsonObject()
            {
                { "event", "subscribe" },
                { "channel", "book" },
                { "symbol", "t" + ep.pair },
                { "prec", "P0" },
                { "freq", "F0" },
                { "length", "50" },
            };

            return JsonHelper.ToJson(pairObj);
        }

        protected override string BuildPongRequest()
        {
            return "";
        }

        protected override void SubscriptionDoneReceived(sbyte sockid, dynamic resp, string msg)
        {
            int handle = resp.data.chanId;
            OrderBook ob;

            int pid = exObj.FindPairIdx("{0}", (string)resp.data.pair);
            if (pid == -1)
                return;

            ob = exObj.pair[pid].book;

            OrderBookSubscribed(sockid, handle, ob);
        }

        protected override void OrderBookDataReceived(sbyte sockid, dynamic resp, string msg)
        {
            int handle = (int)resp.data[0];
            OrderBookReceive(sockid, handle, resp, msg);
        }

        protected override DateTime OrderBookResponseParser(dynamic respObj, string msg, out OrderBookStruct[] data)
        {
            if (JsonHelper.IsJsonArray(respObj.data[1][0]))
            {
                data = new OrderBookStruct[respObj.data[1].Count];
                for (int i = 0; i < respObj.data[1].Count; i++)
                {
                    data[i] = new OrderBookStruct()
                    {
                        price = (double)respObj.data[1][i][0],
                        remove = ((int)respObj.data[1][i][1] == 0),
                        amount = (double)respObj.data[1][i][2],
                    };
                }
            }
            else
            {
                data = new OrderBookStruct[1];
                data[0] = new OrderBookStruct()
                {
                    price = (double)respObj.data[1][0],
                    remove = ((int)respObj.data[1][1] == 0),
                    amount = (double)respObj.data[1][2],
                };
            }

            return DateTime.FromBinary((long)respObj.received);
        }

        protected override enResponseType ParseResponse(sbyte sockid, dynamic resp)
        {
            if (JsonHelper.IsJsonArray(resp.data))
            {
                if (resp.data.Count > 1)
                    if (JsonHelper.IsJsonArray(resp.data[1]))
                        if (resp.data[1].Count > 0)
                            return enResponseType.rtOrderBookData;
            }
            else if (JsonHelper.IsJsonObject(resp.data))
            {
                if (resp.data["event"] != null)
                    if (resp.data["event"] == "auth")
                    {
                        if (resp.data["status"] != null)
                            if (resp.data["status"] == "OK")
                                return enResponseType.rtAuthenticated;
                    }
                    else if (resp.data["event"] == "info")
                    {
                        if (sockid != -1)
                            return enResponseType.rtConnected;
                    }
                    else if (resp.data["event"] == "subscribed")
                    {
                        return enResponseType.rtSubscribed;
                    }
            }
            return enResponseType.rtUnknown;
        }
    }
}

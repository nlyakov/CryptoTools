using CryptoTools.API.Exchanges;
using CryptoTools.API.Interfaces;
using CryptoTools.API.Objects.OrderBooks;
using CryptoTools.API.Objects.Pairs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Security.Cryptography;
using System.Text;

namespace CryptoTools.API.WSS
{
    class WssCex : WssCommunicator
    {
        private HMACSHA256 hashMaker;
        private dynamic authobj;

        public WssCex(Exchange exchange) : base(exchange)
        {
            BaseUrlAuth = "wss://ws.cex.io/ws/";
            BaseUrlPub = "wss://ws.cex.io/ws/";
            SocketAuthenticatedHandler += InitSubscriptions;
            AlwaysAuthenticate = true;
        }

        public override void InitAuthentication(string auth)
        {
            authinfo = false;
            try
            {
                WorkState = enWorkingState.wsInitialize;
                if (auth.Trim().Length > 0)
                {
                    dynamic authObj = JsonConvert.DeserializeObject(auth);

                    string user = authObj.user;
                    string key = authObj.key;
                    string secret = authObj.secret;

                    hashMaker = new HMACSHA256(Encoding.UTF8.GetBytes(secret));

                    authobj = new JObject
                    {
                        { "e", "auth" },
                        {
                            "auth", new JObject
                            {
                                { "key", key },
                                { "timestamp", (new DateTimeOffset(DateTime.Now)).ToUnixTimeSeconds() },
                                { "signature", "" }
                            }
                        }
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
            authobj.auth.timestamp = (new DateTimeOffset(DateTime.Now)).ToUnixTimeSeconds();
            string signature = BitConverter.ToString(hashMaker.ComputeHash(Encoding.ASCII.GetBytes((string)authobj.auth.timestamp + (string)authobj.auth.key))).Replace("-", "").ToLower();
            authobj.auth.signature = signature;

            return JsonConvert.SerializeObject(authobj);
        }

        protected override string BuildSubscriptionRequest(ExchangePair ep)
        {
            dynamic pairObj = new JObject
            {
                { "e", "order-book-subscribe" },
                {
                    "data",  new JObject
                    {
                        {
                            "pair", new JArray
                            {
                                ep.name1,
                                ep.name2
                            }
                        },
                        { "subscribe", true },
                        { "depth", OrderBook.MaxCapacity }
                    }
                },
                { "oid", ep.UniqueID() }
            };

            return JsonConvert.SerializeObject(pairObj);
        }

        protected override string BuildPongRequest()
        {
            dynamic pongobj = new JObject
            {
                { "e", "pong" },
            };

            return JsonConvert.SerializeObject(pongobj);
        }

        protected override void SubscriptionDoneReceived(sbyte sockid, dynamic resp, string msg)
        {
            int handle = resp.data.oid;
            OrderBook ob = exObj.pair[handle].book;

            OrderBookSubscribed(sockid, handle, ob);
            OrderBookReceive(sockid, handle, msg);
            //ob.queue.Enqueue(msg);
            //ob.ResumeReceive();
        }

        protected override void OrderBookDataReceived(sbyte sockid, dynamic resp, string msg)
        {
            int handle = exObj.FindPairIdx("{1}:{2}", (string)resp.data.data.pair);
            OrderBookReceive(sockid, handle, msg);
        }

        protected override DateTime OrderBookResponseParser(string msg, out OrderBookStruct[] data)
        {
            dynamic respObj = JsonConvert.DeserializeObject(msg);
            data = new OrderBookStruct[respObj.data.data.bids.Count + respObj.data.data.asks.Count];

            int idx = 0;
            for (int i = 0; i < respObj.data.data.bids.Count; i++)
            {
                data[idx] = new OrderBookStruct()
                {
                    price = (double)respObj.data.data.bids[i][0],
                    amount = (double)respObj.data.data.bids[i][1],
                };
                if (data[idx].amount == 0)
                {
                    data[idx].remove = true;
                    data[idx].amount = 1;
                }
                idx++;
            }
            for (int i = 0; i < respObj.data.data.asks.Count; i++)
            {
                data[idx] = new OrderBookStruct()
                {
                    price = (double)respObj.data.data.asks[i][0],
                    amount = -(double)respObj.data.data.asks[i][1],
                };
                if (data[idx].amount == 0)
                {
                    data[idx].remove = true;
                    data[idx].amount = -1;
                }
                idx++;
            }

            return DateTime.FromBinary((long)respObj.received);
        }

        protected override enResponseType ParseResponse(sbyte sockid, dynamic resp)
        {
            if (resp.data.e == "ping")
                return enResponseType.rtPing;
            else if (resp.data.e == "auth")
            {
                if (resp.data.data.ok == "ok")
                    return enResponseType.rtAuthenticated;
            }
            else if (resp.data.e == "connected")
                return enResponseType.rtConnected;
            else if (resp.data.e == "order-book-subscribe")
                return enResponseType.rtSubscribed;
            else if (resp.data.e == "md_update")
                return enResponseType.rtOrderBookData;
            else if (resp.data.e == "order")
                return enResponseType.rtOrderInfo;
            else if (resp.data.e == "tx")
                return enResponseType.rtOrderCompleted;
            else if (resp.data.e == "balance")
                return enResponseType.rtBalanceInfo;
            else if (resp.data.e == "obalance")
                return enResponseType.rtOrderBalanceInfo;

            return enResponseType.rtUnknown;
        }
    }
}

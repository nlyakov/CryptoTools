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
    class WssBitfinex : WssCommunicator
    {
        private HMACSHA384 hashMaker;
        private dynamic authobj;

        public WssBitfinex(Exchange exchange) : base(exchange)
        {
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
                    dynamic authObj = JsonConvert.DeserializeObject(auth);

                    string key = authObj.key;
                    string secret = authObj.secret;

                    hashMaker = new HMACSHA384(Encoding.UTF8.GetBytes(secret));

                    authobj = new JObject();
                    authobj.apiKey = key;
                    authobj["event"] = "auth";

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

            return JsonConvert.SerializeObject(authobj);
        }

        protected override string BuildSubscriptionRequest(ExchangePair ep)
        {
            dynamic pairObj = new JObject();
            pairObj["event"] = "subscribe";
            pairObj.channel = "book";
            pairObj.symbol = "t" + ep.pair;
            pairObj.prec = "P0";
            pairObj.freq = "F0";
            pairObj.length = "50";

            return JsonConvert.SerializeObject(pairObj);
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
            if (respObj.data[1][0].GetType().Name.ToString() == "JArray")
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
            //if (resp.data.GetType().Name.ToString() == "JArray")
            if (resp.data is Array)
            {
                if (resp.data.Count > 1)
                    //if (resp.data[1].GetType().Name.ToString() == "JArray")
                    if (resp.data[1] is Array)
                        if (resp.data[1].Count > 0)
                            return enResponseType.rtOrderBookData;
            }
            //else if (resp.data.GetType().Name.ToString() == "JObject")
            else if (resp.data is Object)
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

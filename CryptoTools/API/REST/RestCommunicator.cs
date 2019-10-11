using CryptoTools.API.Exchanges;
using CryptoTools.API.Interfaces;
using CryptoTools.API.Objects;
using System;
using System.Net;

namespace CryptoTools.API.REST
{
    internal abstract class RestCommunicator : IApiCommunicator
    {
        protected class WebClient : System.Net.WebClient
        {
            public int Timeout { get; set; }

            public WebClient()
            {
                Timeout = 60 * 1000;
            }

            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest lWebRequest = base.GetWebRequest(uri);
                lWebRequest.Timeout = Timeout;
                ((HttpWebRequest)lWebRequest).ReadWriteTimeout = Timeout;
                return lWebRequest;
            }
        }

        protected Exchange exObj;

        protected string BaseUrlAuth;
        protected string BaseUrlPub;
        protected WebClient pubClient;

        public bool authenticated = false;
        public enWorkingState WorkState = enWorkingState.wsCompleted;
        public DateTime LastOperatopnTime = DateTime.Now;

        public RestCommunicator(Exchange exchange)
        {
            exObj = exchange;
            pubClient = new WebClient();
            pubClient.Timeout = 30 * 1000;
        }

        public abstract void InitAuthentication(string auth);
        public abstract void Initialize();
        public abstract void UpdateTicker();
        public abstract void UpdateOrderBook(int pairid);
        public abstract double getBalance(string cryptocoin);
        public abstract long OrderCreate(TradeObject obj, enOrderType type, double price);
        public abstract bool OrderCancel(long orderId);
        public abstract bool OrderCancelAll(TradeObject obj);
    }
}

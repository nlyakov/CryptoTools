using CryptoTools.API.Exchanges;
using CryptoTools.API.Interfaces;
using CryptoTools.API.Objects.OrderBooks;
using CryptoTools.API.Objects.Pairs;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoTools.API.WSS
{
    enum enResponseType { rtUnknown, rtConnected, rtAuthenticated, rtSubscribed, rtOrderBookData, rtPing, rtOrderCompleted, rtOrderInfo, rtBalanceInfo, rtOrderBalanceInfo };

    delegate void MessageHandler(string newmessage);
    delegate void SocketReady(sbyte sockid);
    internal abstract class WssCommunicator //: IApiCommunicator
    {
        static int SubscriptionsPerSocket = 30;

        protected Exchange exObj;

        internal string BaseUrlAuth;
        internal string BaseUrlPub;
        private WssSocketObject authsocket;
        private WssSocketObject[] socket;
        public SocketReady SocketConnectedHandler = null;
        public SocketReady SocketAuthenticatedHandler = null;
        public SocketReady SocketPongHandler = null;
        internal ConcurrentQueue<byte[]> QBufer;
        private Task ForwardTask;
        public CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private SemaphoreSlim ssReconnect = new SemaphoreSlim(1, 1);
        private SemaphoreSlim ssForward = new SemaphoreSlim(1, 1);
        private int[] pidlist = null;

        public bool authinfo = false;
        public bool AlwaysAuthenticate = false;
        public enWorkingState WorkState = enWorkingState.wsCompleted;
        public DateTime LastOperatopnTime = DateTime.Now;

        public WssCommunicator(Exchange exchange)
        {
            exObj = exchange;
            QBufer = new ConcurrentQueue<byte[]>();
            SocketPongHandler += SendPong;
        }

        public abstract void InitAuthentication(string auth);
        internal abstract string BuildAuthenticationRequest();
        protected abstract string BuildSubscriptionRequest(ExchangePair ep);
        protected abstract string BuildPongRequest();

        protected abstract void SubscriptionDoneReceived(sbyte sockid, dynamic resp, string msg);
        protected abstract void OrderBookDataReceived(sbyte sockid, dynamic resp, string msg);
        protected abstract DateTime OrderBookResponseParser(string msg, out OrderBookStruct[] data);
        protected abstract enResponseType ParseResponse(sbyte sockid, dynamic resp);

        public void InitSockets()
        {
            int count = (pidlist.Length + SubscriptionsPerSocket - 1) / SubscriptionsPerSocket;

            authsocket = new WssSocketObject(this, -1);

            ForwardTask = null;
            socket = new WssSocketObject[count];
            for (sbyte i = 0; i < count; i++)
            {
                socket[i] = new WssSocketObject(this, i);
                socket[i].Connect();
            }
        }

        public async void ReConnect()
        {
            async Task WaitNull()
            {
                bool alldone;
                do
                {
                    alldone = true;
                    for (sbyte i = 0; i < socket.Length; i++)
                        foreach (KeyValuePair<int, OrderBook> vp in socket[i].subscriptions)
                            if (vp.Value.ReceiveTask != null)
                            {
                                alldone = false;
                                break;
                            }
                } while (!alldone);
                await Task.CompletedTask;
            }

            if (!await ssReconnect.WaitAsync(0))
                return;

            try
            {
                int tc = 0;
                for (sbyte i = 0; i < socket.Length; i++)
                    if (socket[i].ReceiveTask != null)
                        tc++;
                int idx = 0;
                Task[] tasks = new Task[tc];
                for (sbyte i = 0; i < socket.Length; i++)
                    if (socket[i].ReceiveTask != null)
                        tasks[idx++] = socket[i].ReceiveTask;

                cancellationTokenSource.Cancel();
                await Task.WhenAll(tasks);
                if (ForwardTask != null)
                    await Task.WhenAll(ForwardTask);

                QBufer = new ConcurrentQueue<byte[]>();
                await Task.WhenAll(WaitNull());

                for (sbyte i = 0; i < socket.Length; i++)
                {
                    foreach (KeyValuePair<int, OrderBook> vp in socket[i].subscriptions)
                    {
                        vp.Value.bid.Clear();
                        vp.Value.ask.Clear();
                        vp.Value.pair.OrderBookAvailable = false;
                    }
                    socket[i].Dispose();
                }

                // Re init
                cancellationTokenSource = new CancellationTokenSource();
                for (sbyte i = 0; i < socket.Length; i++)
                {
                    socket[i] = new WssSocketObject(this, i);
                    socket[i].Connect();
                }
            }
            finally
            {
                ssReconnect.Release();
            }

        }

        private void SendPong(sbyte sockid)
        {
            socket[sockid].Send(BuildPongRequest());
        }

        internal async void ResumeForward()
        {
            if (!await ssForward.WaitAsync(0))
                return;
            try
            {
                if (ForwardTask != null)
                    return;

                ForwardTask = Forward();
            }
            finally
            {
                ssForward.Release();
            }
        }

        private async Task Forward()
        {
            byte[] message;
            while (!QBufer.IsEmpty)
                if (QBufer.TryDequeue(out message))
                {
                    sbyte sockid = (sbyte)message[0];
                    string msg = Encoding.UTF8.GetString(message, 1, message.Length - 1);
                    dynamic respObj = JsonConvert.DeserializeObject(msg);

                    enResponseType rtype = ParseResponse(sockid, respObj);
                    switch (rtype)
                    {
                        case enResponseType.rtOrderBookData:
                            OrderBookDataReceived(sockid, respObj, msg);
                            break;
                        case enResponseType.rtConnected:
                            socket[sockid].sockConnected = true;
                            SocketConnectedHandler?.Invoke(sockid);
                            break;
                        case enResponseType.rtAuthenticated:
                            socket[sockid].sockAuthenticated = true;
                            SocketAuthenticatedHandler?.Invoke(sockid);
                            break;
                        case enResponseType.rtSubscribed:
                            SubscriptionDoneReceived(sockid, respObj, msg);
                            break;
                        case enResponseType.rtPing:
                            SocketPongHandler?.Invoke(sockid);
                            break;
                    }
                }

            await ssForward.WaitAsync();
            try { ForwardTask = null; }
            finally { ssForward.Release(); }
        }

        public void Send(sbyte sockid, string command)
        {
            socket[sockid].Send(command);
        }

        public void SetSubscriptionList(int[] pairidlist)
        {
            pidlist = new int[pairidlist.Length];
            int idx = 0;
            for (int i = 0; i < pairidlist.Length; i++)
            {
                ExchangePair ep = null;
                if (exObj.pair.TryGetValue(pairidlist[i], out ep))
                    if (ep.trade_supported)
                    {
                        pidlist[idx] = ep.UniqueID();
                        idx++;
                    }
            }
            Array.Resize(ref pidlist, idx);
        }

        internal async void InitSubscriptions(sbyte sockid)
        {
            for (int idx = sockid * SubscriptionsPerSocket; idx < Math.Min((sockid + 1) * SubscriptionsPerSocket, pidlist.Length); idx++)
                await socket[sockid].Send(BuildSubscriptionRequest(exObj.pair[pidlist[idx]]));
        }

        internal void OrderBookSubscribed(sbyte sockid, int handle, OrderBook ob)
        {
            socket[sockid].subscriptions.Add(handle, ob);
            ob.ParseSocketDataHandler = OrderBookResponseParser;
        }

        internal async void OrderBookReceive(sbyte sockid, int handle, string msg)
        {
            while (!socket[sockid].subscriptions.ContainsKey(handle))
                await Task.Delay(100);
            OrderBook ob = socket[sockid].subscriptions[handle];

            OrderBookStruct[] data;
            ob.LastUpdate = ob.ParseSocketDataHandler(msg, out data);
            lock (ob.bid.raw)
            {
                lock (ob.ask.raw)
                {
                    foreach (OrderBookStruct order in data)
                        ob.OBUpdate(order);
                }
            }

            if (!ob.pair.OrderBookAvailable)
                ob.pair.OrderBookAvailable = true;

            ob.ProcessChangeEvents();

        }

    }
}

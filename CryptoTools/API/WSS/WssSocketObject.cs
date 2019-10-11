using CryptoTools.API.Objects.OrderBooks;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoTools.API.WSS
{
    class WssSocketObject : IDisposable
    {
        private WssCommunicator wc = null;
        private ClientWebSocket ws;
        private byte[] DataBuffer = new byte[4096];
        public Task ReceiveTask;
        public readonly sbyte SocketID;
        public Uri socket_uri;
        public bool sockConnected = false;
        public bool sockAuthenticated = false;
        public Dictionary<int, OrderBook> subscriptions;
        private SemaphoreSlim ssReceive = new SemaphoreSlim(1, 1);
        private SemaphoreSlim ssSend = new SemaphoreSlim(1, 1);
        //private DateTime dt = DateTime.Now;
        //private bool crashed = false;

        public WssSocketObject(WssCommunicator parent, sbyte id)
        {
            ws = new ClientWebSocket();

            SocketID = id;
            wc = parent;
            if (SocketID == -1)
                socket_uri = new Uri(parent.BaseUrlAuth);
            else
                socket_uri = new Uri(parent.BaseUrlPub);
            subscriptions = new Dictionary<int, OrderBook>();
            ReceiveTask = null;
        }

        public async Task Connect()
        {
            if (SocketID == -1)
                if (!wc.authinfo)
                    return;

            if (ws.State != WebSocketState.Open && ws.State != WebSocketState.Connecting)
                await ws.ConnectAsync(socket_uri, CancellationToken.None);

            if (SocketID == -1 || (wc.AlwaysAuthenticate && wc.authinfo))
            {
                var request = Encoding.UTF8.GetBytes(wc.BuildAuthenticationRequest());
                await ws.SendAsync(new ArraySegment<byte>(request), WebSocketMessageType.Text, true, CancellationToken.None);
            }

            ResumeReceive();
        }

        private async void ResumeReceive()
        {
            if (!await ssReceive.WaitAsync(0))
                return;

            try
            {
                if (ReceiveTask != null)
                    return;

                ReceiveTask = Receive(wc.cancellationTokenSource.Token);
            }
            finally
            {
                ssReceive.Release();
            }
        }

        internal async Task Receive(CancellationToken cancellationToken)
        {
            while (ws.State == WebSocketState.Open)
            {
                bool makereconnect = false;
                var offset = 0;
                WebSocketReceiveResult response;
                try
                {
                    do
                    {
                        if (offset >= DataBuffer.Length)
                            Array.Resize(ref DataBuffer, DataBuffer.Length + 4096);
                        response = await ws.ReceiveAsync(new ArraySegment<byte>(DataBuffer, offset, DataBuffer.Length - offset), CancellationToken.None);
                        offset += response.Count;
                        /*if (wc is WssCex && (DateTime.Now - dt).TotalSeconds > 30 && !crashed && SocketID==1)
                        {
                            crashed = true;
                            throw new Exception("tralala");
                        }*/
                    }
                    while (!response.EndOfMessage);

                    if (response.MessageType == WebSocketMessageType.Close)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close response received", CancellationToken.None);
                        makereconnect = true;
                    }
                }
                catch
                {
                    makereconnect = true;
                }

                if (makereconnect || cancellationToken.IsCancellationRequested)
                {
                    wc.ReConnect();
                    return;
                }
                else
                {
                    string timestr = "{\"received\":\"" + DateTime.Now.ToBinary().ToString() + "\",\"data\":";
                    byte[] qb = new byte[offset + 2 + timestr.Length];
                    qb[0] = (byte)SocketID;
                    Array.Copy(Encoding.UTF8.GetBytes(timestr), 0, qb, 1, timestr.Length);
                    qb[qb.Length - 1] = (byte)'}';
                    Array.Copy(DataBuffer, 0, qb, 1 + timestr.Length, offset);
                    wc.QBufer.Enqueue(qb);
                    wc.ResumeForward();
                }
            }

            //ReceiveTask = null;
        }

        public async Task Send(string command)
        {
            await ssSend.WaitAsync();
            try
            {
                if (ws.State != WebSocketState.Open)
                    await Connect();

                var request = Encoding.UTF8.GetBytes(command);
                await ws.SendAsync(new ArraySegment<byte>(request), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            finally
            {
                ssSend.Release();
            }
        }

        public void Dispose()
        {
            ws.Dispose();
            GC.SuppressFinalize(this);
        }
    }

}
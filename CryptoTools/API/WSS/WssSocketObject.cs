using CryptoTools.API.Objects.OrderBooks;
using CryptoTools.Generic;
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
        DebugLogger logWriter;

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

            if (GlobalData.SocketLogging)
            {
                logWriter = new DebugLogger();
                logWriter.OpenForWriting(GlobalData.NewFileVolume(wc.exObj.Name, "Socket " + SocketID.ToString() + " [vol{0}].log"));
                logWriter.BufferAdd(GlobalData.LogFileHeader());
            }
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

        async void LogWriteResponce(int BufferSize)
        {
            logWriter.BufferAdd(DateTime.Now.ToString("yyyy-MM-dd---HH-mm-ss") + ": Receive - ");
            logWriter.BufferAdd(DataBuffer, BufferSize);
            logWriter.BufferAdd(Environment.NewLine);

            if (logWriter.BytesWritten < GlobalData.MaxLogVolume)
                return;

            logWriter.Close();
            logWriter = new DebugLogger();
            logWriter.OpenForWriting(GlobalData.NewFileVolume(wc.exObj.Name, "Socket " + SocketID.ToString() + " [vol{0}].log"));
            logWriter.BufferAdd(GlobalData.LogFileHeader());
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
                        if (GlobalData.SocketLogging)
                            logWriter.BufferAdd(DateTime.Now.ToString("yyyy-MM-dd---HH-mm-ss") + ": Closing" + Environment.NewLine);
                    }
                }
                catch (Exception e)
                {
                    if (GlobalData.SocketLogging)
                        logWriter.BufferAdd(DateTime.Now.ToString("yyyy-MM-dd---HH-mm-ss") + ": Exception - " + e.Message + Environment.NewLine);
                    makereconnect = true;
                }

                if (makereconnect || cancellationToken.IsCancellationRequested)
                {
                    if (GlobalData.SocketLogging)
                        logWriter.BufferAdd(DateTime.Now.ToString("yyyy-MM-dd---HH-mm-ss") + ": Reconnecting" + Environment.NewLine);
                    wc.ReConnect();
                    return;
                }
                else
                {
                    if (GlobalData.SocketLogging)
                        LogWriteResponce(offset);
                    string timestr = "{\"received\":\"" + DateTime.Now.ToBinary().ToString() + "\",\"data\":";
                    byte[] qb = new byte[offset + 2 + timestr.Length];
                    qb[0] = (byte)SocketID;
                    Buffer.BlockCopy(Encoding.UTF8.GetBytes(timestr), 0, qb, 1, timestr.Length);
                    qb[qb.Length - 1] = (byte)'}';
                    Buffer.BlockCopy(DataBuffer, 0, qb, 1 + timestr.Length, offset);
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
            if (GlobalData.SocketLogging)
            {
                logWriter.Close();
                logWriter.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }

}
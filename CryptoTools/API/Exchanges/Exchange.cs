using CryptoTools.API.Interfaces;
using CryptoTools.API.Objects;
using CryptoTools.API.Objects.Coins;
using CryptoTools.API.Objects.Pairs;
using CryptoTools.API.REST;
using CryptoTools.API.WSS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoTools.API.Exchanges
{
    public class ParamDict : Dictionary<string, string> { }

    public delegate void UpdatedHandler();

    public delegate void MethodAuthenticate(string auth);
    public delegate void RestMethodInitServerDetails();
    public delegate void RestMethodUpdateTicker();
    public delegate void RestMethodUpdateOrderBook(int pairid);
    public delegate double MethodGetBalance(string cryptocoin);
    public delegate Int64 MethodOrderCreate(TradeObject obj, enOrderType type, double price); // Return OrderID
    public delegate bool MethodOrderCancel(Int64 orderID); // Cancel order by given ID
    public delegate bool MethodOrderCancelAll(TradeObject obj); // Cancel all orders related to pair

    public abstract class Exchange
    {
        public string Name = "empty";
        public ExchangePairDict pair; // List of working pairs
        public Dictionary<int, ExchangeCoin> coin;
        public bool TickersAvailable = false;
        public bool Initialized = false;

        protected internal string pair_pattern;
        internal Dictionary<int, string> CustomCoinNames;

        public MethodAuthenticate Authenticate = null;
        protected RestMethodInitServerDetails RestInitServerDetails = null;
        public RestMethodUpdateTicker RestUpdateTicker = null;
        public RestMethodUpdateOrderBook RestUpdateOrderBook = null;
        public MethodGetBalance GetBalance = null;
        public MethodOrderCreate OrderCreate = null;
        public MethodOrderCancel OrderCancel = null;
        public MethodOrderCancelAll OrderCancelAll = null;

        internal RestCommunicator Rest;
        internal WssCommunicator Wss;

        // Find pair index by pair coins formatted by pattern
        public Exchange()
        {
            coin = new Dictionary<int, ExchangeCoin>();
            pair = new ExchangePairDict();
            CustomCoinNames = new Dictionary<int, string>();
        }

        public async Task InitApi()
        {
            if (Rest != null)
            {
                Authenticate += Rest.InitAuthentication;
                RestInitServerDetails = Rest.Initialize;
                RestUpdateTicker = Rest.UpdateTicker;
                RestUpdateOrderBook = Rest.UpdateOrderBook;
                GetBalance = Rest.getBalance;
                OrderCreate = Rest.OrderCreate;
                OrderCancel = Rest.OrderCancel;
                OrderCancelAll = Rest.OrderCancelAll;
            }

            if (Wss != null)
            {
                Authenticate += Wss.InitAuthentication;
            }

            RestInitServerDetails?.Invoke();
            await Task.CompletedTask;
        }

        internal int FindPairIdx(string pattern, string match)
        {
            foreach (KeyValuePair<int, ExchangePair> ep in pair)
                if (ep.Value.trade_supported)
                    if (match == string.Format(pattern, ep.Value.pair, ep.Value.name1, ep.Value.name2))
                        return ep.Key;
            return -1;
        }

        internal int FindPairIdx(IPair ipair)
        {
            foreach (KeyValuePair<int, ExchangePair> ep in pair)
                if (ep.Value.trade_supported)
                    if (ep.Value.coin1 == ipair.coin1 && ep.Value.coin2 == ipair.coin2)
                        return ep.Key;
            return -1;
        }

        // Fix internal exchange coin name if different
        public string CoinName(int coin)
        {
            string cname;
            if (!CustomCoinNames.TryGetValue(coin, out cname))
                cname = Coin.Symbol(coin);
            return cname.ToUpper();
        }

        internal ExchangeCoin AddCoin(int coinid, string name)
        {
            if (coin.ContainsKey(coinid))
                return null;

            ExchangeCoin ec = new ExchangeCoin()
            {
                available = true,
                id = coinid,
                name = name,
                symbol = Coin.Symbol(coinid),
                type = Coin.Info(coinid).type,
            };
            coin.Add(coinid, ec);
            return ec;
        }

        // Internal exchange to global coin id
        internal int CustomCoinNameToCoinID(string coinname)
        {
            foreach (KeyValuePair<int, string> rec in CustomCoinNames)
                if (rec.Value == coinname)
                    return rec.Key;

            return -1;
        }

        // Build pair string from given pair id
        public string PairName(int pairid)
        {
            return string.Format(pair_pattern, CoinName(pair[pairid].coin1), CoinName(pair[pairid].coin2));
        }
    }
}

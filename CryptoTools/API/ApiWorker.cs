using CryptoTools.API.Exchanges;
using CryptoTools.API.Objects;
using CryptoTools.API.Objects.Coins;
using CryptoTools.API.Objects.Offers;
using CryptoTools.API.Objects.Pairs;
using CryptoTools.Units;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timer = System.Windows.Forms.Timer;

namespace CryptoTools.API
{
    public enum enApiWorkerState { awsCompleted, awsFailed, awsTickerUpdating, awsOrderBookUpdating }

    public delegate void UpdatedHandler(int id);

    public class ApiWorker
    {
        public List<Exchange> exchanges;
        public ConcurrentDictionary<int, PairInfo> pairs;
        public List<Offer> offers;
        public List<TransferDeal> deals;
        private Timer TickerTimer;
        public UpdatedHandler OnTickersUpdated;
        public UpdatedHandler OnOrderBooksUpdated;
        private int[] obPairKeyList;
        private bool obUpdateStop;
        public enApiWorkerState State = enApiWorkerState.awsCompleted;

        public ApiWorker()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            exchanges = new List<Exchange> {
                new Binance(),
                new Bitfinex(),
                new Bitstamp(),
                new Bittrex(),
                new Cex(),
                new Huobi(),
                new Idax(),
                new Kraken(),
                new KuCoin(),
                new Poloniex(),
                new ZB()
            };
            Task[] tasks = new Task[exchanges.Count];
            for (int i = 0; i < exchanges.Count; i++)
            {
                int idx = i;
                tasks[i] = exchanges[idx].InitApi();
            }
            Task.WaitAll(tasks);

            for (int i = exchanges.Count - 1; i >= 0; i--)
                if (!exchanges[i].Initialized)
                    exchanges.RemoveAt(i);

            pairs = new ConcurrentDictionary<int, PairInfo>();
            tasks = new Task[exchanges.Count];
            for (int i = 0; i < exchanges.Count; i++)
            {
                int idx = i;
                tasks[i] = Task.Factory.StartNew(() => AddPairList(exchanges[idx].pair));
            }
            Task.WaitAll(tasks);

            offers = new List<Offer>();
            InitializePairs();

            SetWssOrderBooks(GetOffersPairKeyList());

            deals = new List<TransferDeal>();

            TickerTimer = new Timer();
            TickerTimer.Interval = 10 * 1000;
            TickerTimer.Tick += OnTickerTimer;
            TickerTimer.Enabled = false;
        }

        private protected string FindExchangeAuth(string exname, string auth)
        {
            dynamic authObj = JsonConvert.DeserializeObject(auth);
            for (int i = 0; i < authObj.exchanges.Count; i++)
            {
                if (exname == (string)authObj.exchanges[i].name)
                    return JsonConvert.SerializeObject(authObj.exchanges[i]);
            }
            return "";
        }

        public void InitAuthentication(string auth)
        {
            Task[] tasks = new Task[exchanges.Count];
            for (int i = 0; i < exchanges.Count; i++)
            {
                int idx = i;
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    exchanges[idx].Authenticate?.Invoke(FindExchangeAuth(exchanges[idx].Name, auth));
                });
            }
            Task.WaitAll(tasks);
        }

        public async void OpenSockets()
        {
            Task[] tasks = new Task[exchanges.Count];
            for (int i = 0; i < exchanges.Count; i++)
            {
                int idx = i;
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    exchanges[idx].Wss?.InitSockets();
                });
            }
            await Task.WhenAll(tasks);
        }

        public void SetTickerTimer(bool status)
        {
            TickerTimer.Enabled = status;
        }

        private void OnTickerTimer(Object myObject, EventArgs myEventArgs)
        {
            TickerTimer.Enabled = false;
            UpdateTickers();
            TickerTimer.Enabled = true;
        }

        public void UpdateTickers()
        {
            if (pairs.Count == 0)
                return;

            if (State != enApiWorkerState.awsCompleted)
            {
                State = enApiWorkerState.awsFailed;
                return;
            }

            State = enApiWorkerState.awsTickerUpdating;
            try
            {
                Task[] tasks = new Task[exchanges.Count];
                for (int i = 0; i < exchanges.Count; i++)
                {
                    int idx = i;
                    tasks[i] = Task.Factory.StartNew(() => exchanges[idx].RestUpdateTicker());
                }
                Task.WaitAll(tasks);

                State = enApiWorkerState.awsCompleted;
                OnTickersUpdated?.Invoke(-1);
            }
            catch
            {
                State = enApiWorkerState.awsFailed;
            }
        }

        public void SetWssOrderBooks(int[] keys)
        {
            for (int i = 0; i < exchanges.Count; i++)
                exchanges[i].Wss?.SetSubscriptionList(keys);
        }

        public void OrderBookUpdateStop()
        {
            obUpdateStop = true;
        }

        public async void UpdateOrderBooks(int[] pkeys)
        {
            if (State != enApiWorkerState.awsCompleted)
            {
                State = enApiWorkerState.awsFailed;
                return;
            }

            State = enApiWorkerState.awsOrderBookUpdating;
            if (pkeys == null)
            {
                obPairKeyList = new int[pairs.Count];
                pairs.Keys.CopyTo(obPairKeyList, 0);
            }
            else
            {
                obPairKeyList = new int[pkeys.Length];
                Array.Copy(pkeys, 0, obPairKeyList, 0, pkeys.Length);
            }

            obUpdateStop = false;
            await OrderBookUpdateProcess(0);
        }

        private async Task OrderBookUpdateProcess(int BookIndex)
        {
            if (!obUpdateStop)
                while (BookIndex < obPairKeyList.Length && !pairs[obPairKeyList[BookIndex]].usable)
                    BookIndex++;

            if (BookIndex == obPairKeyList.Length || obUpdateStop)
            {
                State = enApiWorkerState.awsCompleted;
                OnOrderBooksUpdated?.Invoke(-1);

                return;
            }
            try
            {
                int pairid = obPairKeyList[BookIndex];
                Task[] tasks = new Task[exchanges.Count];
                for (int i = 0; i < exchanges.Count; i++)
                {
                    int idx = i;
                    int pid = obPairKeyList[BookIndex];
                    tasks[i] = Task.Factory.StartNew(() =>
                    {
                        if (exchanges[idx].Wss == null)
                            if (exchanges[idx].pair.Supported(pid))
                            {
                                exchanges[idx].RestUpdateOrderBook(pid);
                                exchanges[idx].pair[pid].book.MakeSnapshot();
                                exchanges[idx].pair[pid].ticker.ask = exchanges[idx].pair[pid].book.AskPriceFirst();
                                exchanges[idx].pair[pid].ticker.bid = exchanges[idx].pair[pid].book.BidPriceFirst();
                            }
                    });
                }
                await Task.WhenAll(tasks);
                OnOrderBooksUpdated?.Invoke(pairid);

                BookIndex++;
            }
            catch
            {
                OnOrderBooksUpdated?.Invoke(-1);
                State = enApiWorkerState.awsFailed;
            }

            await Task.Delay(1000);
            OrderBookUpdateProcess(BookIndex);
        }

        public double CoinAnyPrice(int refcoin)
        {
            int pid = -1;
            for (int i = 0; i < Coin.UsdCoins.Length; i++)
            {
                pid = (refcoin << 16) | (Coin.UsdCoinId & 0xFFFF);
                if (!pairs.ContainsKey(pid))
                    continue;

                for (int exid = 0; exid < exchanges.Count; exid++)
                    if (exchanges[exid].pair.Supported(pid))
                        if (exchanges[exid].pair[pid].TickerAvailable)
                            return (exchanges[exid].pair[pid].ticker.ask + exchanges[exid].pair[pid].ticker.bid) / 2;
                        else if (exchanges[exid].pair[pid].OrderBookAvailable)
                            return (exchanges[exid].pair[pid].book.AskPriceFirst() + exchanges[exid].pair[pid].book.BidPriceFirst()) / 2;
            }
            return -1;
        }

        public void RefreshTransferDeals(double TradeAmount)
        {
            deals.Clear();
            for (int ex1 = 0; ex1 < exchanges.Count; ex1++)
                for (int ex2 = 0; ex2 < exchanges.Count; ex2++)
                    if (ex1 != ex2)
                    {
                        double maxval = 0;
                        int maxpairid = -1;
                        foreach (KeyValuePair<int, PairInfo> bp in pairs)
                        {
                            if (bp.Value.coin2 != Coin.UsdCoinId)
                                continue;
                            if (!exchanges[ex1].pair.Supported(bp.Key) || !exchanges[ex2].pair.Supported(bp.Key))
                                continue;

                            double tmp;
                            double ask = exchanges[ex1].pair[bp.Key].ticker.ask;
                            exchanges[ex1].pair[bp.Key].book.ask.MakeSnapshot();
                            if (exchanges[ex1].pair[bp.Key].book.ask.dict.Count > 0)
                                ask = exchanges[ex1].pair[bp.Key].book.AskPriceForValue(TradeAmount, out tmp);
                            double bid = exchanges[ex2].pair[bp.Key].ticker.bid;
                            exchanges[ex2].pair[bp.Key].book.bid.MakeSnapshot();
                            if (exchanges[ex2].pair[bp.Key].book.bid.dict.Count > 0)
                                bid = exchanges[ex2].pair[bp.Key].book.BidPriceForValue(TradeAmount, out tmp);
                            tmp = bid / ask;

                            if (maxpairid > -1)
                                if (tmp < maxval)
                                    continue;
                            maxpairid = bp.Key;
                            maxval = tmp;
                        }

                        if (maxpairid > -1)
                            deals.Add(new TransferDeal()
                            {
                                exch1 = ex1,
                                exch2 = ex2,
                                pairid = maxpairid,
                                coeff = maxval
                            });
                    }
        }

        private protected int[] GetOffersPairKeyList()
        {
            List<int> pklist = new List<int>();
            for (int i = 0; i < offers.Count; i++)
            {
                if (!pklist.Contains(offers[i].pairid1))
                    pklist.Add(offers[i].pairid1);
                if (!pklist.Contains(offers[i].pairid2))
                    pklist.Add(offers[i].pairid2);
            }
            return pklist.ToArray();
        }

        private protected void InitializePairs()
        {
            // Create indexes
            int idx = 0;
            foreach (KeyValuePair<int, PairInfo> vp in pairs)
            {
                vp.Value.index = idx;
                idx++;
            }
            for (int i = 0; i < exchanges.Count; i++)
                foreach (KeyValuePair<int, ExchangePair> vp in exchanges[i].pair)
                    vp.Value.index = pairs[vp.Value.UniqueID()].index;

            // Initialize similar pairs lists
            foreach (KeyValuePair<int, PairInfo> bp in pairs)
            {
                CoinInfo bci = Coin.Info(bp.Value.coin2);
                if (bci.type == enCoinType.ctFiat || bci.type == enCoinType.ctStable)
                    foreach (KeyValuePair<int, PairInfo> rp in pairs)
                        if (bp.Key != rp.Key)
                            if (rp.Value.coin1 == bp.Value.coin1)
                            {
                                CoinInfo rci = Coin.Info(rp.Value.coin2);
                                if (rci.type == enCoinType.ctFiat || rci.type == enCoinType.ctStable)
                                    if (bci.fiatgroup == rci.fiatgroup)
                                        bp.Value.SimilarPairs.Add(rp.Key);
                            }
            }

            // Create offer objects
            offers.Clear();
            foreach (KeyValuePair<int, PairInfo> bp in pairs)
                for (int exid1 = 0; exid1 < exchanges.Count - 1; exid1++)
                    if (exchanges[exid1].pair.Supported(bp.Key))
                        for (int exid2 = exid1 + 1; exid2 < exchanges.Count; exid2++)
                            if (exchanges[exid2].pair.Supported(bp.Key))
                            {
                                Offer offer = new Offer();
                                offer.ex1id = exid1;
                                offer.ex2id = exid2;
                                offer.pairid1 = bp.Key;
                                offer.pairid2 = bp.Key;
                                offer.ex1 = exchanges[exid1];
                                offer.ex2 = exchanges[exid2];
                                offer.ex1pair = exchanges[exid1].pair[bp.Key];
                                offer.ex2pair = exchanges[exid2].pair[bp.Key];
                                offers.Add(offer);

                                bp.Value.usable = true;
                            }
                            else
                                for (int spid = 0; spid < bp.Value.SimilarPairs.Count; spid++)
                                    if (exchanges[exid2].pair.Supported(bp.Value.SimilarPairs[spid]))
                                    {
                                        Offer offer = new Offer();
                                        offer.ex1id = exid1;
                                        offer.ex2id = exid2;
                                        offer.pairid1 = bp.Key;
                                        offer.pairid2 = bp.Value.SimilarPairs[spid];
                                        offer.ex1 = exchanges[exid1];
                                        offer.ex2 = exchanges[exid2];
                                        offer.ex1pair = exchanges[exid1].pair[bp.Key];
                                        offer.ex2pair = exchanges[exid2].pair[bp.Value.SimilarPairs[spid]];
                                        offers.Add(offer);

                                        pairs[offer.pairid1].usable = true;
                                        pairs[offer.pairid2].usable = true;
                                    }
        }

        private protected void AddPairList(Dictionary<int, ExchangePair> expairlist)
        {
            if (expairlist == null)
                return;

            foreach (KeyValuePair<int, ExchangePair> p in expairlist)
            {
                if (pairs.TryAdd(p.Key, null))
                    pairs[p.Key] = new PairInfo(p.Value.coin1, p.Value.coin2);
            }
        }
    }
}

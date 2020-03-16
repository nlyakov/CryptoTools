using CryptoTools.API.Objects.Pairs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CryptoTools.API.Objects.OrderBooks
{
    public delegate void OBChanged(OrderBook ob);
    public delegate DateTime ParseSocketData(dynamic respObj, string msg, out OrderBookStruct[] data);
    delegate void OBUpdate(OrderBookStruct order);

    public class OrderBook
    {
        public ExchangePair pair = null;
        public ParseSocketData ParseSocketDataHandler = null;
        //private OBUpdate OBUpdateHandler;
        public OBChanged OBChangedHandler;
        public OrderBookRecords bid;
        public OrderBookRecords ask;
        private SemaphoreSlim ssEvent = new SemaphoreSlim(1, 1);
        public DateTime LastUpdate;

        public static int MaxCapacity = 100;

        public OrderBook(ExchangePair owner)
        {
            pair = owner;
            bid = new OrderBookRecords(enOrderBookType.obBid);
            ask = new OrderBookRecords(enOrderBookType.obAsk);
            //OBUpdateHandler = OBUpdateMode1;
            OBChangedHandler = null;
        }

        public void OBUpdate(OrderBookStruct order)
        {
            if (order.amount < 0)
            {
                if (order.remove)
                    ask.DelData(order.price);
                else
                    ask.AddData(order.price, -order.amount);
            }
            else
            {
                if (order.remove)
                    bid.DelData(order.price);
                else
                    bid.AddData(order.price, order.amount);
            }
        }

        public async void ProcessChangeEvents()
        {
            if (OBChangedHandler is null)
                return;

            if (!await ssEvent.WaitAsync(0))
                return;
            try
            {
                var eventListeners = OBChangedHandler.GetInvocationList();

                for (int index = 0; index < eventListeners.Count(); index++)
                {
                    var methodToInvoke = (OBChanged)eventListeners[index];
                    methodToInvoke.BeginInvoke(this, EndAsyncEvent, null);
                }
            }
            finally
            {
                ssEvent.Release();
            }
        }

        private void EndAsyncEvent(IAsyncResult iar)
        {
            var ar = (System.Runtime.Remoting.Messaging.AsyncResult)iar;
            var invokedMethod = (OBChanged)ar.AsyncDelegate;

            try
            {
                invokedMethod.EndInvoke(iar);
            }
            catch { }
        }

        // Check what is the avg price for target quantity of coins. Returns also used quantity for calculations (it may not be used entire required quantity)
        public double PriceForQty(OrderBookRecords offers, double qty, out double qty_found)
        {
            double collected = 0;
            double totalvalue = 0;
            foreach (KeyValuePair<double, double> vp in offers.dict)
            {
                totalvalue += Math.Min(vp.Value, qty - collected) * vp.Key;
                collected += vp.Value;
                if (collected >= qty)
                {
                    qty_found = qty;
                    return totalvalue / qty; // Amount is filled
                }
            }

            qty_found = collected;
            return totalvalue / collected;// Amount is not filled
        }

        public double BidPriceForQty(double qty, out double qty_found)
        {
            return PriceForQty(bid, qty, out qty_found);
        }

        public double AskPriceForQty(double qty, out double qty_found)
        {
            return PriceForQty(ask, qty, out qty_found);
        }

        // Get first price (ticker price)
        public double PriceFirst(OrderBookRecords offers)
        {
            if (offers.dict.Count > 0)
                return offers.dict.First().Key;
            else
                return double.NaN;
        }

        public double BidPriceFirst()
        {
            return PriceFirst(bid);
        }

        public double AskPriceFirst()
        {
            return PriceFirst(ask);
        }

        // Check what is the avg coin price for target reference value. Returns also used value for calculations (it may not be used entire required value)
        public double PriceForValue(OrderBookRecords offers, double value, out double value_found)
        {
            double collectedvalue = 0;
            double totalqty = 0;

            foreach (KeyValuePair<double, double> vp in offers.dict)
            {
                totalqty += Math.Min(vp.Value, (value - collectedvalue) / vp.Key);
                collectedvalue += vp.Value * vp.Key;
                if (collectedvalue >= value)
                {
                    value_found = value;
                    return value / totalqty; // Amount is filled
                }
            }

            value_found = collectedvalue;
            return collectedvalue / totalqty; // Amount is not filled
        }

        public double BidPriceForValue(double value, out double value_found)
        {
            return PriceForValue(bid, value, out value_found);
        }

        public double AskPriceForValue(double value, out double value_found)
        {
            return PriceForValue(ask, value, out value_found);
        }

        public bool Empty()
        {
            return (bid.dict.Count == 0 || ask.dict.Count == 0);
        }

        public void MakeSnapshot()
        {
            bid.MakeSnapshot();
            ask.MakeSnapshot();
        }
    }
}

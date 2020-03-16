using System;
using System.Collections.Generic;

namespace CryptoTools.API.Objects.OrderBooks
{
    public enum enOrderBookType { obBid, obAsk };
    class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
    {
        public int Compare(T x, T y)
        {
            return y.CompareTo(x);
        }
    }

    public class OrderBookRecords
    {

        public SortedDictionary<double, double> dict;
        public Dictionary<double, double> raw;
        public readonly enOrderBookType type;

        public OrderBookRecords(enOrderBookType obType)
        {
            type = obType;
            if (type == enOrderBookType.obBid)
                dict = new SortedDictionary<double, double>(new DescendingComparer<double>());
            else
                dict = new SortedDictionary<double, double>();
            raw = new Dictionary<double, double>();
        }

        public void AddData(double price, double amount)
        {
            //if (raw.ContainsKey(price))
            raw[price] = amount;
            //else
            //raw.Add(price, amount);
        }

        public void DelData(double price)
        {
            //            if (raw.ContainsKey(price))
            raw.Remove(price);
        }

        public void Clear()
        {
            raw.Clear();
        }

        public void MakeSnapshot()
        {
            lock (raw)
            {
                if (type == enOrderBookType.obBid)
                    dict = new SortedDictionary<double, double>(raw, new DescendingComparer<double>());
                else
                    dict = new SortedDictionary<double, double>(raw);
            }
        }
    }
}

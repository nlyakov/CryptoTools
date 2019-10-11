using CryptoTools.API.Interfaces;
using CryptoTools.API.Objects.OrderBooks;

namespace CryptoTools.API.Objects.Pairs
{
    public class ExchangePair : IPair
    {
        public int index;
        public int coin1 { get; set; }
        public int coin2 { get; set; }
        public string name1, name2;
        public string pair;
        public bool trade_supported = false;
        public bool margin_supported = false;
        public bool iceberg_supported = false;
        public double min_trade_size = 0;
        public double max_trade_size = 0;
        public double min_trade_value = 0;
        public int price_precision = -1;
        public int qty_precision = -1;
        public double coin_volume = -1;
        public double ref_volume = -1;

        public Ticker ticker;
        public OrderBook book;
        public bool TickerAvailable = false;
        public bool OrderBookAvailable = false;

        public ExchangePair()
        {
            ticker = new Ticker();
            book = new OrderBook(this);
        }

        public int UniqueID()
        {
            return (coin1 << 16) | (coin2 & 0xFFFF);
        }
    }
}

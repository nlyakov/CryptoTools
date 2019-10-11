namespace CryptoTools.API.Objects
{
    public delegate void TickerChange();

    public class Ticker
    {
        public double bid, ask;
        public double bidqty, askqty;
        public double last;

        public TickerChange OnTickerChange;
    }
}

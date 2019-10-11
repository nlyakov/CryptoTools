using CryptoTools.API.REST;

namespace CryptoTools.API.Exchanges
{
    class Bitstamp : Exchange
    {
        public Bitstamp()
        {
            Name = "Bitstamp";
            pair_pattern = "{0}{1}";

            Rest = new RestBitstamp(this);
            Wss = null;
        }
    }
}

using CryptoTools.API.REST;

namespace CryptoTools.API.Exchanges
{
    class Bittrex : Exchange
    {
        public Bittrex()
        {
            Name = "Bittrex";
            pair_pattern = "{1}-{0}";

            Rest = new RestBittrex(this);
            Wss = null;
        }
    }
}

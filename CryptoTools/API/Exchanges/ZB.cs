using CryptoTools.API.Objects.Coins;
using CryptoTools.API.REST;

namespace CryptoTools.API.Exchanges
{
    class ZB : Exchange
    {
        public ZB()
        {
            Name = "ZB";
            pair_pattern = "{0}_{1}";

            CustomCoinNames.Add(Coin.AddOrGet("BCH"), "BCHABC");

            Rest = new RestZB(this);
            Wss = null;
        }
    }
}

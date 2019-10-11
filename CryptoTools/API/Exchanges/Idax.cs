using CryptoTools.API.Objects.Coins;
using CryptoTools.API.REST;

namespace CryptoTools.API.Exchanges
{
    class Idax : Exchange
    {
        public Idax()
        {
            Name = "Idax";
            pair_pattern = "{0}_{1}";

            CustomCoinNames.Add(Coin.AddOrGet("BCH"), "BCHABC");

            Rest = new RestIdax(this);
            Wss = null;
        }
    }
}

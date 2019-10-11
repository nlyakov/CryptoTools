using CryptoTools.API.Objects.Coins;
using CryptoTools.API.REST;

namespace CryptoTools.API.Exchanges
{
    class Kraken : Exchange
    {
        public Kraken()
        {
            Name = "Kraken";
            pair_pattern = "{0}{1}";

            CustomCoinNames.Add(Coin.AddOrGet("BTC"), "XBT");

            Rest = new RestKraken(this);
            Wss = null;
        }
    }
}

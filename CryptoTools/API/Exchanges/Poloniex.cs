using CryptoTools.API.Objects.Coins;
using CryptoTools.API.REST;

namespace CryptoTools.API.Exchanges
{
    class Poloniex : Exchange
    {
        public Poloniex()
        {
            Name = "Poloniex";
            pair_pattern = "{1}_{0}";

            CustomCoinNames.Add(Coin.AddOrGet("XLM"), "STR");
            CustomCoinNames.Add(Coin.AddOrGet("BCH"), "BCHABC");

            Rest = new RestPoloniex(this);
            Wss = null;
        }
    }
}

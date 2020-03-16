using CryptoTools.API.Objects.Coins;

namespace CryptoTools.API.Exchanges
{
    class Binance : Exchange
    {
        public Binance()
        {
            Name = "Binance";
            pair_pattern = "{0}{1}";

            CustomCoinNames.Add(Coin.AddOrGet("BCH"), "BCHABC");
            CustomCoinNames.Add(Coin.AddOrGet("HOT-HOLO"), "HOT");

            Rest = new REST.RestBinance(this);
            Wss = null;
        }
    }
}

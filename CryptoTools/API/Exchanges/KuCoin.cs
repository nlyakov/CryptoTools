using CryptoTools.API.Objects.Coins;
using CryptoTools.API.REST;

namespace CryptoTools.API.Exchanges
{
    class KuCoin : Exchange
    {
        public KuCoin()
        {
            Name = "KuCoin";
            pair_pattern = "{0}-{1}";

            CustomCoinNames.Add(Coin.AddOrGet("BCH"), "BCHABC");

            Rest = new RestKuCoin(this);
            Wss = null;
        }
    }
}

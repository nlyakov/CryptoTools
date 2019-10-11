using CryptoTools.API.Objects.Coins;
using CryptoTools.API.REST;

namespace CryptoTools.API.Exchanges
{
    class Huobi : Exchange
    {
        public Huobi()
        {
            Name = "Huobi";
            pair_pattern = "{0}{1}";

            CustomCoinNames.Add(Coin.AddOrGet("HOT-HYDRO"), "HOT");

            Rest = new RestHuobi(this);
            Wss = null;
        }
    }
}

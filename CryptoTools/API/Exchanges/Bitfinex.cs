using CryptoTools.API.Objects.Coins;
using CryptoTools.API.REST;
using CryptoTools.API.WSS;

namespace CryptoTools.API.Exchanges
{
    class Bitfinex : Exchange
    {
        public Bitfinex()
        {
            Name = "Bitfinex";
            pair_pattern = "{0}{1}";

            CustomCoinNames.Add(Coin.AddOrGet("HOT-HYDRO"), "HOT");

            Rest = new RestBitfinex(this);
            Wss = null;
            Wss = new WssBitfinex(this);
        }
    }
}

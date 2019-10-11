namespace CryptoTools.API.Objects.Coins
{
    public class CoinInfo
    {
        public int id;
        public string symbol;
        public string fullname = "";
        public enCoinType type = enCoinType.ctNone;
        public int fiatgroup = -1;
        public string network = "";
        public CoinInfo networkcoin = null;

        public CoinInfo()
        {
        }
    }
}

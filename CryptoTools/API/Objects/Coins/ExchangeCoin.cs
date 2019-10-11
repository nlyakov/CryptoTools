namespace CryptoTools.API.Objects.Coins
{
    public class ExchangeCoin
    {
        public int id;
        public string symbol;
        public string name = "";
        public double balance = 0;
        public enCoinType type = enCoinType.ctNone;
        public string depositaddr = "";
        public bool available = false;
        public bool candeposit = true;
        public bool canwithdraw = true;
        public int confirmations = 0;
        public double fee = 0;
    }

}

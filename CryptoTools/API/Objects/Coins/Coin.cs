using System.Collections.Concurrent;

namespace CryptoTools.API.Objects.Coins
{
    public enum enCoinType { ctNone, ctFiat, ctStable, ctCrypto };

    public static class Coin
    {
        private static string[] FiatCoins = new string[] { "USD", "EUR", "GPB", "JPY" };
        public static string[] UsdCoins = new string[] { "USD", "USDT", "GUSD", "USDC", "TUSD", "PAX" }; //, "XCHF"
        public static string[] EurCoins = new string[] { "EUR", "EURT", "EURS" };
        public static string[][] StableLists = new string[][] { UsdCoins, EurCoins };

        private static ConcurrentDictionary<int, string> coindict;
        private static ConcurrentDictionary<string, int> revcoindict;
        private static ConcurrentDictionary<int, CoinInfo> coininfo;

        public static int UsdCoinId;
        static Coin()
        {
            coindict = new ConcurrentDictionary<int, string>();
            revcoindict = new ConcurrentDictionary<string, int>();
            coininfo = new ConcurrentDictionary<int, CoinInfo>();

            for (int i = 0; i < FiatCoins.Length; i++)
            {
                CoinInfo ci = Info(AddOrGet(FiatCoins[i]));
                ci.type = enCoinType.ctFiat;
                ci.fiatgroup = i;
            }

            foreach (string[] slist in StableLists)
            {
                CoinInfo maincoin = Info(AddOrGet(slist[0]));
                for (int i = 1; i < slist.Length; i++)
                {
                    CoinInfo subcoin = Info(AddOrGet(slist[i]));
                    subcoin.fiatgroup = maincoin.fiatgroup;
                    subcoin.type = enCoinType.ctStable;
                }
            }

            UsdCoinId = AddOrGet(UsdCoins[0]);
        }

        public static int AddOrGet(string coin)
        {
            coin = coin.ToUpper();

            int newid = revcoindict.Count;
            int coinid = revcoindict.GetOrAdd(coin, newid);
            if (coinid == newid)
            {
                coindict.TryAdd(newid, coin);
                coininfo.TryAdd(newid, new CoinInfo() { id = newid, symbol = coin, type = enCoinType.ctCrypto });
                return newid;
            }
            else
                return coinid;
        }

        public static string Symbol(int id)
        {
            string sym;
            if (coindict.TryGetValue(id, out sym))
                return sym;
            else
                return "";

        }

        public static CoinInfo Info(int id)
        {
            CoinInfo val;
            if (coininfo.TryGetValue(id, out val))
                return val;
            else
                return null;
        }

        public static int Count
        {
            get
            {
                return coindict.Count;
            }
        }

        /*public static int[,] MakePairList(string [,] names)
        {
            int[,] list = new int[names.GetLength(0), names.GetLength(1)];
            for (int i = 0; i < names.GetLength(0); i++)
            {
                list[i, 0] = Key(names[i, 0]);
                list[i, 1] = Key(names[i, 1]);
            }
            return list;
        }*/
    }

}

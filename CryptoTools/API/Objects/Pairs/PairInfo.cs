using CryptoTools.API.Interfaces;
using CryptoTools.API.Objects.Coins;
using System.Collections.Generic;

namespace CryptoTools.API.Objects.Pairs
{
    public class PairInfo : IPair
    {
        public int index;
        public int coin1 { get; set; }
        public int coin2 { get; set; }
        public bool usable = false;

        public List<int> SimilarPairs;

        public PairInfo(int coin1, int coin2)
        {
            this.coin1 = coin1;
            this.coin2 = coin2;

            SimilarPairs = new List<int>();
        }

        public string PairName => Coin.Symbol(coin1) + "/" + Coin.Symbol(coin2);
    }
}


using System.Collections.Generic;

namespace CryptoTools.API.Objects.Pairs
{
    public class ExchangePairDict : Dictionary<int, ExchangePair>
    {
        public ExchangePairDict()
        {
        }

        internal bool Supported(int pairid)
        {
            if (ContainsKey(pairid))
                if (this[pairid].trade_supported)
                    return true;

            return false;
        }

    }
}

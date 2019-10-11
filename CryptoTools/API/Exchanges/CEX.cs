using CryptoTools.API.REST;
using CryptoTools.API.WSS;

namespace CryptoTools.API.Exchanges
{
    public class Cex : Exchange
    {
        public Cex()
        {
            Name = "CEX";
            pair_pattern = "{0}/{1}";

            Rest = new RestCex(this);
            Wss = null;
            Wss = new WssCex(this);
        }
    }
}

using CryptoTools.API.Exchanges;
using CryptoTools.API.Objects.OrderBooks;
using CryptoTools.API.Objects.Pairs;

namespace CryptoTools.API.Objects.PairRelations
{
    public class Relation
    {
        public int ex1id = -1;
        public int ex2id = -1;
        public int pairid1 = -1;
        public int pairid2 = -1;

        public Exchange ex1 = null;
        public Exchange ex2 = null;
        public ExchangePair ex1pair = null;
        public ExchangePair ex2pair = null;

        public Relation()
        {
        }

        public bool TickerAvailable()
        {
            if (ex1pair != null && ex2pair != null)
                return (ex1pair.TickerAvailable && ex2pair.TickerAvailable);
            else
                return false;
        }

        public bool OrderBookAvailable()
        {
            if (ex1pair != null && ex2pair != null)
                return (ex1pair.OrderBookAvailable && ex2pair.OrderBookAvailable);
            else
                return false;
        }

        public void Swap()
        {
            int exid = ex1id;
            ex1id = ex2id;
            ex2id = exid;

            Exchange ex = ex1;
            ex1 = ex2;
            ex2 = ex;

            int pairid = pairid1;
            pairid1 = pairid2;
            pairid2 = pairid;

            ExchangePair pair = ex1pair;
            pair = ex1pair;
            ex1pair = ex2pair;
        }

        public Relation GetReversed()
        {
            return new Relation()
            {
                ex1id = ex2id,
                ex1 = ex2,
                pairid1 = pairid2,
                ex1pair = ex2pair,

                ex2id = ex1id,
                ex2 = ex1,
                pairid2 = pairid1,
                ex2pair = ex1pair,
            };
        }

        public OfferCoefficients OrderBookCalc(OrderBook book1, OrderBook book2, double refamount)
        {
            double found;
            book1.MakeSnapshot();
            book2.MakeSnapshot();
            double book1ask = book1.AskPriceForValue(refamount, out found);
            double book1bid = book1.BidPriceForValue(refamount, out found);
            double ticker1bid = book1.BidPriceFirst();
            double ticker1ask = book1.AskPriceFirst();
            double book2ask = book2.AskPriceForValue(refamount, out found);
            double book2bid = book2.BidPriceForValue(refamount, out found);
            double ticker2bid = book2.BidPriceFirst();
            double ticker2ask = book2.AskPriceFirst();


            return new OfferCoefficients
            {
                taker1taker2 = (book2bid - book1ask) / book1ask,
                maker1taker2 = (book2bid - ticker1bid) / ticker1bid,
                taker1maker2 = (ticker2ask - book1ask) / book1ask,
                maker1maker2 = (ticker2ask - ticker1bid) / ticker1bid,

                taker2taker1 = (book1bid - book2ask) / book2ask,
                maker2taker1 = (book1bid - ticker2bid) / ticker2bid,
                taker2maker1 = (ticker1ask - book2ask) / book2ask,
                maker2maker1 = (ticker1ask - ticker2bid) / ticker2bid,
            };
        }

        public OfferCoefficients TickerCalc(Ticker tick1, Ticker tick2)
        {
            return new OfferCoefficients
            {
                taker1taker2 = (tick2.bid - tick1.ask) / tick1.ask,
                maker1taker2 = (tick2.bid - tick1.bid) / tick1.bid,
                taker1maker2 = (tick2.ask - tick1.ask) / tick1.ask,
                maker1maker2 = (tick2.ask - tick1.bid) / tick1.bid,

                taker2taker1 = (tick1.bid - tick2.ask) / tick2.ask,
                maker2taker1 = (tick1.bid - tick2.bid) / tick2.bid,
                taker2maker1 = (tick1.ask - tick2.ask) / tick2.ask,
                maker2maker1 = (tick1.ask - tick2.bid) / tick2.bid,
            };
        }
    }
}

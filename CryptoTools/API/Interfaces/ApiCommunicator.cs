using CryptoTools.API.Objects;
using System;

namespace CryptoTools.API.Interfaces
{
    public enum enWorkingState { wsCompleted, wsError, wsInitialize, wsGetOrderBook, wsUpdateTicker, wsOrderCreate, wsOrderCancel, wsOrderCancelAll, wsWithdraw };
    public enum enOrderType { buy, sell };

    interface IApiCommunicator
    {
        void InitAuthentication(string auth);

        void UpdateTicker();

        void UpdateOrderBook(int pairid);

        // Return available balance for a given Coin name. USD can be used for dollar balance, or leave empty for default WorkCryptoCoin balance
        double getBalance(string cryptocoin);

        // Return OrderID
        Int64 OrderCreate(TradeObject obj, enOrderType type, double price);

        // Cancel order by given ID
        bool OrderCancel(Int64 orderID);

        // Cancel all orders related to pair
        bool OrderCancelAll(TradeObject obj);

    }
}

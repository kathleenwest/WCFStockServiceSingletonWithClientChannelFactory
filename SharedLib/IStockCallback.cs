using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace SharedLib
{
    /// <summary>
    /// This interface will act as the contract 
    /// that the client must implement to receive callback messages.
    /// </summary>
    public interface IStockCallback
    {
        /// <summary>
        /// This is the interface for the callback of the
        /// server to the client
        /// </summary>
        /// <param name="tx">(StockTransaction) change in a stock object</param>
        [OperationContract(IsOneWay = true)]
        void StockUpdated(StockTransaction tx);

    } // end of interface
} // end of namespace

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace SharedLib
{
    /// <summary>
    /// Interface for Stock Service
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IStockCallback))]
    public interface IStockService
    {
        /// <summary>
        /// Login to the applications
        /// </summary>
        [OperationContract(IsInitiating = true)]
        void Login();

        /// <summary>
        /// Logout from the application
        /// </summary>
        /// <param name="sessionID">(string) session id, default null</param>
        [OperationContract(IsTerminating = true)]
        void Logout(string sessionID = null);

        /// <summary>
        /// Start monitoring Tickers
        /// </summary>
        [OperationContract]
        void StartTickerMonitoring();

        /// <summary>
        /// Stop Monitoring Tickers
        /// </summary>
        [OperationContract]
        void StopTickerMonitoring();

        /// <summary>
        /// Get a stock quote
        /// </summary>
        /// <param name="symbol">(string) symbol of stock to get</param>
        /// <returns>(Stock) objects</returns>
        [OperationContract]
        Stock GetStockQuote(string symbol);

        /// <summary>
        /// Add new stock to the system
        /// </summary>
        /// <param name="symbol">(string) symbol of the stock</param>
        /// <param name="price">(decimal) price of the stock</param>
        [OperationContract]
        void AddNewStock(string symbol, decimal price);

    } // end of interface
} // end of namespace

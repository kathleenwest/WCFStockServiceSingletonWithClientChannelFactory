using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLib;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.Threading;

namespace StockServiceLib
{
    /// <summary>
    /// StockService
    /// This class implements the IStockService interface and has 
    /// the ServcieBehavior attribute applied to it. The ServcieBehavior 
    /// attribute must have its InstanceContextMode property set to 
    /// InstanceContextMode.Single to ensure there will be only one 
    /// service running on the server for all clients.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class StockService : IStockService
    {
        #region Fields

        /// <summary>
        /// Maintains a list of clients connected to the service, keyed by the client session ID
        /// </summary>
        private ConcurrentDictionary<string, ClientContainer> m_Clients;

        /// <summary>
        /// List of Stock objects, keyed by the stock’s ticker symbol
        /// </summary>
        private ConcurrentDictionary<string, Stock> m_Stocks;

        /// <summary>
        /// Random number generator
        /// </summary>
        private Random m_Rnd;

        /// <summary>
        /// Timer that will fire periodically, changing a stock’s value and notifying clients
        /// </summary>
        private Timer m_Timer;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// 
        public StockService()
        {
            // Initialize m_Clients, m_Stocks and m_Rnd to new default objects of their respective types.
            m_Clients = new ConcurrentDictionary<string, ClientContainer>();
            m_Stocks = new ConcurrentDictionary<string, Stock>();
            m_Rnd = new Random();
            //m_Clients = default(ConcurrentDictionary<string, ClientContainer>);
            //m_Stocks = default(ConcurrentDictionary<string, Stock>);
            //m_Rnd = default(Random);

            // Create a set of default stocks
            string[] symbols = { "MSFT", "IBM", "AAPL", "GOOG", "YHOO", "INTC" };
            foreach (string symbol in symbols)
            {
                AddNewStock(symbol, m_Rnd.Next(10, 30)); // AddNewStock will be added later... 
            }

            m_Timer = new Timer(StockTimerCallback, null, 2000, 2000); // StockTimerCallback later...

        } // end of default constructor

        #endregion Constructors

        #region Methods

        /// <summary>
        /// AddNewStock
        /// Adds a new Stock object to the m_Stocks dictionary. 
        /// This is also the method called in the constructor when 
        /// adding the list of default stocks. If the stock symbol 
        /// already exists the new stock is rejected and a fault 
        /// is thrown to the client.
        /// </summary>
        /// <param name="symbol">(string) ticker symbol for the new Stock, 
        /// which also acts as its key in the m_Stocks dictionary.</param>
        /// <param name="price">(decimal) The price parameter is the starting 
        /// price for the new Stock.</param>
        public void AddNewStock(string symbol, decimal price)
        {
            try
            {
                symbol = symbol.ToUpper().Trim();
                if (!m_Stocks.ContainsKey(symbol))
                {
                    Stock stock = new Stock(symbol, price);
                    m_Stocks.TryAdd(symbol, stock);
                }
                else
                {
                    string msg = string.Format("Stock symbol '{0}' already exists.", symbol);
                    Console.WriteLine(msg);
                    throw new Exception(msg);
                }
            } // end of try

            catch (Exception ex)
            {
                Console.WriteLine("An error occurred adding stock info: {0}", ex.Message);
                throw new FaultException(ex.Message);
            } // end of catch statement

        } // end of AddStockMethod

        /// <summary>
        /// GetStockQuote
        /// This method searches for the given Stock in the m_Stocks dictionary 
        /// (remember, the key in the dictionary is the stock symbol). 
        /// If the Stock object exists, return it. Otherwise, throw a 
        /// new FaultException.
        /// </summary>
        /// <param name="symbol">(string) stock ticker symbol</param>
        /// <returns>(Stock) stock object</returns>
        public Stock GetStockQuote(string symbol)
        {
            Stock stock = null;

            try
            {
                symbol = symbol.ToUpper().Trim();
                if (m_Stocks.TryGetValue(symbol, out stock))
                {
                    // Return Stock
                    return stock;
                }
                else
                {
                    string msg = string.Format("Stock symbol '{0}' does not exist.", symbol);
                    Console.WriteLine(msg);
                    throw new Exception(msg);
                }
            } // end of try

            catch (Exception ex)
            {
                Console.WriteLine("An error occurred retrieving stock info: {0}", ex.Message);
                throw new FaultException(ex.Message);
            } // end of catch statement;
        }

        /// <summary>
        /// Login()
        /// Implementation for the Login method. It will require 
        /// retrieving the client’s session ID and the client’s 
        /// callback channel. These values will be used to cache 
        /// the list of clients into the m_Clients field. If the 
        /// user has already logged in, then a fault will be 
        /// thrown to the client.
        /// </summary>
        public void Login()
        {
            string sessionID = OperationContext.Current.SessionId;
            IStockCallback client = OperationContext.Current.GetCallbackChannel<IStockCallback>();
            if (!m_Clients.ContainsKey(sessionID))
            {
                Console.WriteLine("Client '{0}' logged in.", sessionID); 
                // Add new ClientContainer that stores the client callback object and sets 
                // IsActive to false 
                m_Clients.TryAdd(sessionID, new ClientContainer(client, false));
            }
            else
            {
                string msg = string.Format("A client with the token '{0}' has already logged in!", sessionID);
                Console.WriteLine(msg);
                throw new FaultException(msg);
            }
        } // end of Login method

        /// <summary>
        /// Logout
        /// This method performs the necessary logic to logoff a client. 
        /// Essentially, the client with the given session ID is removed 
        /// from m_Clients. If the session ID is not provided (which is 
        /// likely coming from the client) then it is divined using 
        /// the same technique as in Login.
        /// </summary>
        /// <param name="sessionID">(string) session identifier of the client</param>
        public void Logout(string sessionID = null)
        {
            try
            {
                if (string.IsNullOrEmpty(sessionID))
                {
                    sessionID = OperationContext.Current.SessionId;
                } // end of if

                if (m_Clients.ContainsKey(sessionID))
                {
                    Console.WriteLine("Client '{0}' logged off.", sessionID);
                    ClientContainer removedItem;
                    if (!m_Clients.TryRemove(sessionID, out removedItem))
                    {
                        string msg = string.Format("Unable to remove client '{0}'", sessionID);
                    }
                } // end of if

            } // end of try

            catch (Exception ex)
            {
                string msg = string.Format("An error occurred removing client '{0}': {1}", sessionID, ex.Message);
                Console.WriteLine(msg);
                throw new FaultException(msg);
            } // end of catch

        } // end of loggout method

        /// <summary>
        /// StartTickerMonitoring()
        /// This method is called from the client when the client 
        /// wishes to be notified about changes to stocks over time. 
        /// All this method will do is modify the appropriate 
        /// ClientContainer object in m_Clients to set the 
        /// IsActive property to true. The timer looks for this 
        /// value when deciding if a client should be notified.
        /// </summary>
        public void StartTickerMonitoring()
        {
            string sessionID = OperationContext.Current.SessionId;

            if (m_Clients.ContainsKey(sessionID))
            {
                m_Clients[sessionID].IsActive = true;
            }

        } // end of method

        /// <summary>
        /// StopTickerMonitoring()
        /// This method is called from the client when the client 
        /// wishes not to be notified about changes to stocks over time. 
        /// All this method will do is modify the appropriate 
        /// ClientContainer object in m_Clients to set the 
        /// IsActive property to false. The timer looks for this 
        /// value when deciding if a client should be notified.
        /// </summary>
        public void StopTickerMonitoring()
        {
            string sessionID = OperationContext.Current.SessionId;

            if (m_Clients.ContainsKey(sessionID))
            {
                m_Clients[sessionID].IsActive = false;
            }
        } // end of method

        /// <summary>
        /// StockTimerCallback
        /// This method acts as the timer callback event handler 
        /// for m_Timer. Look in the constructor of the class to 
        /// see where this method is attached to the timer. When 
        /// this event is triggered, a Stock object in m_Stocks 
        /// will be randomly modified and notifications will be 
        /// sent to any subscribing clients. The method must be 
        /// very careful not to allow a Stock to have a negative 
        /// price. Once a Stock object has been picked and 
        /// modified, each client with the “IsActive” property 
        /// set to true will be notified of the change.
        /// </summary>
        /// <param name="state">not used</param>
        private void StockTimerCallback(object state)
        {          
            // Create a new random stock transaction 
            Stock stock = m_Stocks[m_Stocks.Keys.ElementAt(m_Rnd.Next(m_Stocks.Count))];

            // Get a random value between -1.00 and 1.00 
            decimal change = ((decimal)m_Rnd.Next(-100, 100)) / 100M;

            // Make sure share price cannot go negative 
            if (stock.Price + change < 0)
            {
                change = -change;
            }

            // Update stock price 
            stock.Price += change;
            StockTransaction tx = new StockTransaction(stock, DateTime.Now, change, m_Rnd.Next(1, 1000));
            
            // Notify subscribed clients 
            foreach (string key in m_Clients.Keys.ToList())
            {

                try
                {
                    if (m_Clients[key].IsActive)
                    {
                        m_Clients[key].ClientCallback.StockUpdated(tx);
                    }
                } // end of try

                catch (Exception ex)
                {
                    Console.WriteLine("Error contacting client '{0}': {1}", key, ex.Message);
                    Logout(key);
                } // end of catch

            } // end of foreach

        } // end of method

        #endregion Methods

        } // end of class
    } // end of namespace

using SharedLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using System.ServiceModel;

namespace StockClient
{
    /// <summary>
    /// Main Program Class for User Interface
    /// </summary>
    class Program
    {
        #region private fields

        // reference to the server proxy for access by all methods in this class
        static IStockService m_Proxy = null;
        // is the object that will receive callback events from the service
        static StockMonitor m_Monitor = null;

        #endregion private field

        /// <summary>
        /// User Interface 
        /// Stock Client Program
        /// </summary>
        /// <param name="args">None used</param>
        static void Main(string[] args)
        {

            try
            {
                MenuChoicesEnum choice = MenuChoicesEnum.Quit;
                m_Monitor = new StockMonitor();
                m_Proxy = ProxyGen.GetChannel<IStockService>(m_Monitor);
                m_Proxy.Login();

                do
                {
                    Console.Clear();
                    choice = ConsoleHelpers.ReadEnum<MenuChoicesEnum>("Enter selection: ");
                    switch (choice)
                    {
                        case MenuChoicesEnum.GetStockQuote:
                            GetStockQuote();
                            break;
                        case MenuChoicesEnum.AddStock:
                            AddStock();
                            break;
                        case MenuChoicesEnum.StartMonitoring:
                            MonitorStocks();
                            break;
                    }
                    Console.Write("Press <ENTER> to continue...");
                    Console.ReadLine();
                } while (choice != MenuChoicesEnum.Quit);

            } // end of try

            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error occurred: {0}", ex.Message);
                Console.ResetColor();
            } // end of catch

            finally
            {
                if (m_Proxy != null)
                {
                    m_Proxy.Logout();
                }
            } // end of finally

            Console.Write("Press <ENTER> to quit...");
            Console.ReadLine();

        } // end of main method

        /// <summary>
        /// This method will do the following:
        ///  Notify the user that stock monitoring 
        /// has begun and that they should press 
        /// enter to stop.
        ///  Call the StartTickerMonitoring method 
        /// of the m_Proxy object.
        ///  Wait with a Console.ReadLine call.
        ///  When the user hits enter and passes the 
        /// Console.ReadLine call from the previous 
        /// step, call StopTickerMonitoring on m_Proxy.
        /// </summary>
        private static void MonitorStocks()
        {
            Console.WriteLine("Stock Monitoring Started...");
            Console.WriteLine("Press <ENTER> to stop monitoring...");
            m_Proxy.StartTickerMonitoring();
            Console.ReadLine();

            m_Proxy.StopTickerMonitoring();

        } // end of method

        /// <summary>
        /// GetStockQuote will prompt the user for a 
        /// string called symbol. Then the GetStockQuote 
        /// method is called upon the m_Proxy object, 
        /// passing in the given variable. If successful 
        /// (no exception) then output the returned Stock 
        /// object. Otherwise, display the error.
        /// </summary>
        private static void GetStockQuote()
        {
            // Prompt for Symbol
            string symbol = ConsoleHelpers.ReadString("Enter the Stock Symbol: ");

            try
            {
                Stock stock = m_Proxy.GetStockQuote(symbol);
                Console.WriteLine($"Stock {stock.Symbol} successfully retrieved.");
                Console.WriteLine($"Symbol: {stock.Symbol}");
                Console.WriteLine($"Price: {stock.Price}");
            }

            catch (FaultException ex)
            {
                Console.WriteLine($"Get Failure: Stock {symbol} was not retrieved. {ex.Reason}");
            }

        } // end of method

        /// <summary>
        /// AddStock will prompt the user for a string 
        /// called symbol and a decimal called price. 
        /// Then the AddNewStock method is called upon 
        /// the m_Proxy object, passing in the given variables. 
        /// If successful (no exception) then notify the user 
        /// that the stock was successfully added. 
        /// Otherwise, display the error.
        /// </summary>
        private static void AddStock()
        {
            // Prompt for Symbol
            string symbol = ConsoleHelpers.ReadString("Enter the Stock Symbol: ");
            decimal price = ConsoleHelpers.ReadDecimal("Enter the Stock price: ",0);

            try
            {
                m_Proxy.AddNewStock(symbol, price);
                Console.WriteLine($"Stock {symbol} successfully added at price {price}.");
            }

            catch (FaultException ex)
            {
                Console.WriteLine($"Add Failure: Stock {symbol} at price {price} was not added. {ex.Reason}");
            }

        } // end of method

        /// <summary>
        /// Menu Choices Enumeration
        /// </summary>
        enum MenuChoicesEnum
        {
            Quit = 0,
            AddStock,
            GetStockQuote,
            StartMonitoring
        } // end of enum

    } // end of class
} // end of namespace

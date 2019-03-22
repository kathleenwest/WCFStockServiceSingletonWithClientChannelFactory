using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLib;

namespace StockClient
{
    /// <summary>
    /// StockMonitor
    /// This class will act as the target for callbacks from the service
    /// </summary>
    class StockMonitor : IStockCallback
    {
        /// <summary>
        /// StockUpdated
        /// This method is called when the service wishes to 
        /// notify the client of a stock price change
        /// </summary>
        /// <param name="tx">(StockTransaction) transaction of the stock change</param>
        public void StockUpdated(StockTransaction tx)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            if (tx.Change < 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (tx.Change > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }

            Console.WriteLine(tx);
            Console.ResetColor();
        } // end of method

    } // end of class
} // end of namespace

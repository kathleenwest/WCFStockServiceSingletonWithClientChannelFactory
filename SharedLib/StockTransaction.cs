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
    /// StockTransaction
    /// Represents a change in a Stock object. 
    /// Basically, when a stock has a price change 
    /// the change is stored in a StockTransaction object 
    /// and it is sent to all clients that are 
    /// monitoring the stocks.
    /// </summary>
    [DataContract]
    public class StockTransaction
    {
        #region Properties

        /// <summary>
        /// Stock Object
        /// </summary>
        [DataMember]
        public Stock Stock { get; set; }

        /// <summary>
        /// Date/time of the transaction
        /// </summary>
        [DataMember]
        public DateTime Time { get; set; }

        /// <summary>
        /// Amount that the stock price changed
        /// </summary>
        [DataMember]
        public decimal Change { get; set; }

        /// <summary>
        /// Number of shares traded at the new price
        /// </summary>
        [DataMember]
        public int Shares { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public StockTransaction()
        {
            Stock = new Stock();
            Time = DateTime.Now;
            Change = default(decimal);
            Shares = default(int);
        } // end of method

        /// <summary>
        /// Parameterized Constructor
        /// </summary>
        /// <param name="stock">(Stock) stock object</param>
        /// <param name="time">(DateTime) time of the transaction</param>
        /// <param name="change">(decimal) amount of change</param>
        /// <param name="shares">(int) number of shares</param>
        public StockTransaction(Stock stock, DateTime time, decimal change, int shares)
        {
            Stock = stock;
            Time = time;
            Change = change;
            Shares = shares;           
        } // end of method

        #endregion Constructors

        #region Methods

        /// <summary>
        /// ToString()
        /// Overrides the object ToString method with
        /// a custom formatted string to detail
        /// the StockTransaction object
        /// </summary>
        /// <returns>(string) formatted string of the StockTransaction object</returns>
        public override string ToString()
        {
            char direction = '=';

            if (Change < 0)
            {
                direction = 'V';
            }
            else if (Change > 0)
            {
                direction = '^';
            }

            return string.Format("{0:yyyy-MM-dd HH:mm:ss} {1} {2} {3,10:N2} [{4,8:N0}]", Time, Stock, direction, Change, Shares);
        } // end of method

        #endregion Methods

    } // end of class
} // end of namespace

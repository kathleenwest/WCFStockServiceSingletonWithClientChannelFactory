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
    /// Stock
    /// Represents a stock object data model with
    /// ticker symbol and current price
    /// </summary>
    [DataContract]
    public class Stock
    {
        #region Properties

        /// <summary>
        /// Stock symbol, like "MSFT" for Microsoft
        /// </summary>
        [DataMember]
        public string Symbol { get; set; }

        /// <summary>
        /// Current Trading Price
        /// </summary>
        [DataMember]
        public decimal Price { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// Stock will be empty and priced as 0M
        /// </summary>
        public Stock()
        {
            Symbol = String.Empty;
            Price = default(decimal);
        } // end of method
        
        /// <summary>
        /// Parameterized Constructor
        /// </summary>
        /// <param name="symbol">(string) stock symbol ex: "IBM"</param>
        /// <param name="price">(decimal) stock price</param>
        public Stock(string symbol, decimal price)
        {
            Symbol = symbol;
            Price = price;
        } // end of method

        #endregion Constructors

        #region Methods

        /// <summary>
        /// ToString()
        /// Overrides the default object ToString method
        /// Provides custo formatting of the Stock object
        /// </summary>
        /// <returns>(string) formatted string of the stock symbol and price</returns>
        public override string ToString()
        {
            return string.Format("{0,-6} {1,10:N2}", Symbol, Price);
        } // end of method

        #endregion Methods

    } // end of class
} // end of namespace

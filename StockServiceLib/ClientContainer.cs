using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLib;

namespace StockServiceLib
{
    /// <summary>
    /// ClientContainer
    /// class will act as a container or “wrapper” 
    /// for the client callback objects that will 
    /// be called when a stock price has changed.
    /// </summary>
    class ClientContainer
    {
        #region Properties

        /// <summary>
        /// Reference to a client callback object (ie. handle to a client object)
        /// </summary>
        public IStockCallback ClientCallback { get; set; }

        /// <summary>
        /// Indicates if the given client is set to receive callback messages.
        /// </summary>
        public bool IsActive { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ClientContainer()
        {
            ClientCallback = null;
            IsActive = false;
        } // end of method

        /// <summary>
        /// Parameterized Constructor
        /// </summary>
        /// <param name="clientCallback">(IStockCallback) client call back</param>
        /// <param name="isActive">(bool) is the client active?</param>
        public ClientContainer(IStockCallback clientCallback, bool isActive)
        {
            ClientCallback = clientCallback;
            IsActive = isActive;
        } // end of method

        #endregion Constructors

    } // end of class
} // end of namespace

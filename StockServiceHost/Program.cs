using StockServiceLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace StockServiceHost
{
    /// <summary>
    /// The StockServiceHost is a typical Console 
    /// Application being used to host the StockService 
    /// service class.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = null;
            try
            {
                Console.WriteLine("Starting service...");
                host = new ServiceHost(typeof(StockService));
                host.Open();
                Console.WriteLine("Service started.");
                Console.WriteLine("Press <ENTER> to stop service...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred starting the service: {0}, {1}", ex.Message, ex.InnerException.Message);
            }

            finally
            {
                if (host != null)
                {
                    host.Close();
                }
            }

            Console.Write("Press <ENTER> to quit...");
            Console.ReadLine();

        } // end of method

    } // end of class

} // end of namespace

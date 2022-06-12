using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyUpdateProducts
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            bool isCanada = args.Any((v) => string.Equals(v, "/canada", StringComparison.OrdinalIgnoreCase));

            Console.WriteLine(string.Format("Store: {0}", isCanada ? "CA" : "US"));

            var app = new App(isCanada);

            var update = new Updater(app);
            update.Run();

            Console.WriteLine("Finished - Press any key to exit.");
            Console.ReadKey();
        }
    }
}

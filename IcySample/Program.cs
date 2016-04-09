using Icy.Container;
using Icy.Database;
using Icy.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IcyApp = Icy.Foundation.Application;

namespace IcySample
{

    class Program
    {
        static void Main(string[] args)
        {
            var app = new IcyApp();

            app.config<ApplicationDatabaseConfig>().defaults = "test";
            app.config<ApplicationDatabaseConfig>().connections["test"] = new ApplicationDatabaseConnectionConfig()
            {
                host = "localhost",
                database = 
            };
        }
    }
}

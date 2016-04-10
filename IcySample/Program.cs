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
        static IcyApp app;
        static DatabaseManager dm;

        static void Main(string[] args)
        {
            // initialize the app
            app = new IcyApp();

            // put some config
            app.config<ApplicationDatabaseConfig>().defaults = "test";
            app.config<ApplicationDatabaseConfig>().connections["test"] = new ApplicationDatabaseConnectionConfig()
            {
                host = "localhost",
                database = "TEST",
                username = "sa",
                password = "" 
            };

            // initialize the database manager
            dm = new DatabaseManager(app, new Icy.Database.Connectors.ConnectionFactory());

            // perform queries! :D
            dm.connection().query().select().from("table").where("column", "=", 1).first();
        }
    }
}

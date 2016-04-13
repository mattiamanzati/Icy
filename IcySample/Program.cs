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

        static void Main(string[] args)
        {
            // initialize the app
            app = new IcyApp();


            app.config<ApplicationDatabaseConfig>().defaults = "bus";
            app.config<ApplicationDatabaseConfig>().connections["bus"] = new ApplicationDatabaseConnectionConfig()
            {
                host = "localhost",
                username = "sa",
                password = "Zuffellat0",
                database = "PRO",
                driver = "sqlsrv"
            };


            app.register(new DatabaseServiceProvider(app));

            var query = app.make<Connection>().query().select().from("anagra").limit(10);
            var sql = query.toSql();
            var res = query.get();
        }
    }
}

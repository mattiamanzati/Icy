using Icy.Container;
using Icy.Database;
using Icy.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IcyApp = Icy.Foundation.Application;
using Icy.Database.Model;

namespace IcySample
{

    public class SampleModel : Model<SampleModel>
    {


        protected void bootSampleModel(){
            Console.WriteLine("Booted sample model!");
        }
    }

    class Program
    {
        static IcyApp app;

        static void Main(string[] args)
        {
            // initialize the app
            app = new IcyApp();

            var config = new ApplicationDatabaseConfig();
            config.defaults = "bus";
            config.connections["bus"] = new ApplicationDatabaseConnectionConfig()
            {
                host = "localhost",
                username = "sa",
                password = "Zuffellat0",
                database = "PRO",
                driver = "sqlsrv"
            };

            app.instance<ApplicationDatabaseConfig>(config);
            
            app.register(new DatabaseServiceProvider(app));

            var query = app.make<Connection>().query().select().from("anagra").limit(10);
            var sql = query.toSql();
            var res = query.get();

            var model1 = app.make<SampleModel>();
            var model2 = app.make<SampleModel>();
            var model3 = app.make<SampleModel>();
        }
    }
}

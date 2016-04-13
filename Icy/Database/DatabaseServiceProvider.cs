using Icy.Database.Connectors;
using Icy.Foundation;
using Icy.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icy.Database
{
    public class DatabaseServiceProvider: ServiceProvider
    {
        public DatabaseServiceProvider(Application app): base(app) { }

        public override void register()
        {
            this.app.singleton<ConnectionFactory>((app, o) => new ConnectionFactory((Application)app));
            this.app.singleton<DatabaseManager>((app, o) => new DatabaseManager((Application)app, app.make<ConnectionFactory>()));
            this.app.bind<Connection, Connection>((app, o) => app.make<DatabaseManager>().connection());
        }

        public override void boot()
        {
            
        }
    }
}

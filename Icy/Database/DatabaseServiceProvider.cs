using Icy.Database.Connectors;
using Icy.Foundation;
using Icy.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Icy.Database.Model;

namespace Icy.Database
{
    public class DatabaseServiceProvider: ServiceProvider
    {
        public DatabaseServiceProvider(Application app): base(app) { }

        public override void register()
        {
            this.app.singleton<ModelStorage>();
            this.app.singleton<ConnectionFactory>();
            this.app.singleton<DatabaseManager>();
            this.app.bind<Connection>((app, o) => app.make<DatabaseManager>().connection());
        }

        public override void boot()
        {
            
        }
    }
}

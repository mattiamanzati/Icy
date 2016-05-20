using Icy.Database.Connectors;
using Icy.Foundation;
using Icy.Support;


namespace Icy.Database
{
    public class DatabaseServiceProvider: ServiceProvider
    {
        public DatabaseServiceProvider(Application app): base(app) { }

        public override void register()
        {
            this.app.singleton<ConnectionFactory>();
            this.app.singleton<DatabaseManager>();
            this.app.bind<Connection>((app, o) => app.make<DatabaseManager>().connection());
        }

        public override void boot()
        {
            
        }
    }
}

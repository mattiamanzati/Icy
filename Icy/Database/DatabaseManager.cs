using System;
using System.Collections.Generic;
using System.Text;
using Icy.Database.Connectors;
using Icy.Foundation;

namespace Icy.Database
{
    // 7310ce4  on 11 Dec 2015
    public class DatabaseManager: ConnectionResolverInterface
    {
            /**
     * The application instance.
     *
     * @var \Illuminate\Foundation\Application
     */
    protected Application _app;
    /**
     * The database connection factory instance.
     *
     * @var \Illuminate\Database\Connectors\ConnectionFactory
     */
    protected ConnectionFactory _factory;
    /**
     * The active connection instances.
     *
     * @var array
     */
    protected Dictionary<string, Connection> _connections = new Dictionary<string, Connection>();
    /**
     * The custom connection resolvers.
     *
     * @var array
     */
    protected Dictionary<string, Func<ApplicationDatabaseConnectionConfig, string, Connection>> _extensions =  new Dictionary<string,Func<ApplicationDatabaseConnectionConfig, string, Connection>>();
    /**
     * Create a new database manager instance.
     *
     * @param  \Illuminate\Foundation\Application  $app
     * @param  \Illuminate\Database\Connectors\ConnectionFactory  $factory
     * @return void
     */
    public DatabaseManager(Application app, ConnectionFactory factory)
    {
        this._app = app;
        this._factory = factory;
    }
    /**
     * Get a database connection instance.
     *
     * @param  string  $name
     * @return \Illuminate\Database\Connection
     */
    public virtual ConnectionInterface connection(string name = null)
    {
        string[] p = this.parseConnectionName(name);
        name = p[0];
        string type = p[1];
        // If we haven't created this connection, we'll create it based on the config
        // provided in the application. Once we've created the connections we will
        // set the "fetch mode" for PDO which determines the query return types.
        if (!this._connections.ContainsKey(name)) {
            Connection connection = this.makeConnection(name);
            this.setPdoForType(connection, type);
            this._connections[name] = this.prepare(connection);
        }
        return this._connections[name];
    }
    /**
     * Parse the connection into an array of the name and read / write type.
     *
     * @param  string  $name
     * @return array
     */
    protected virtual string[] parseConnectionName(string name)
    {
        name = name ?? this.getDefaultConnection();
        return name.EndsWith("::read") || name.EndsWith("::write") ? name.Split(new string[]{ "::" }, StringSplitOptions.None) : new string[]{ name, null };
    }
    /**
     * Disconnect from the given database and remove from local cache.
     *
     * @param  string  $name
     * @return void
     */
    public virtual void purge(string name = null)
    {
        this.disconnect(name);
        this._connections.Remove(name);
    }
    /**
     * Disconnect from the given database.
     *
     * @param  string  $name
     * @return void
     */
    public virtual void disconnect(string name = null)
    {
        name = name ?? this.getDefaultConnection();
        if(this._connections.ContainsKey(name)){
            this._connections[name].disconnect();
        }
    }
    /**
     * Reconnect to the given database.
     *
     * @param  string  $name
     * @return \Illuminate\Database\Connection
     */
    public virtual ConnectionInterface reconnect(string name = null)
    {
        name = name ?? this.getDefaultConnection();
        this.disconnect(name);
        if (!this._connections.ContainsKey(name)) {
            return this.connection(name);
        }
        return this.refreshPdoConnections(name);
    }
    /**
     * Refresh the PDO connections on a given connection.
     *
     * @param  string  $name
     * @return \Illuminate\Database\Connection
     */
    protected virtual Connection refreshPdoConnections(string name)
    {
        Connection fresh = this.makeConnection(name);
        return this._connections[name]
                                .setPdo(fresh.getPdo())
                                .setReadPdo(fresh.getReadPdo());
    }
    /**
     * Make the database connection instance.
     *
     * @param  string  $name
     * @return \Illuminate\Database\Connection
     */
    protected virtual Connection makeConnection(string name)
    {
        ApplicationDatabaseConnectionConfig config = this.getConfig(name);
        // First we will check by the connection name to see if an extension has been
        // registered specifically for that connection. If it has we will call the
        // Closure and pass it the config allowing it to resolve the connection.
        if (this._extensions.ContainsKey(name)) {
            return this._extensions[name](config, name);
        }
        string driver = config.driver;
        // Next we will check to see if an extension has been registered for a driver
        // and will call the Closure if so, which allows us to have a more generic
        // resolver for the drivers themselves which applies to all connections.
        if (this._extensions.ContainsKey(name)) {
            return this._extensions[driver](config, name);
        }
        return this._factory.make(config, name);
    }
    /**
     * Prepare the database connection instance.
     *
     * @param  \Illuminate\Database\Connection  $connection
     * @return \Illuminate\Database\Connection
     */
    protected virtual Connection prepare(Connection connection)
    {
        // TODO? Is there a way to conditionally return DataTable or Dictionary<string, object>[] ?
        // $connection->setFetchMode($this->app['config']['database.fetch']);
        
        // TODO: Event dispatcher on app?
        //if ($this->app->bound('events')) {
        //    $connection->setEventDispatcher($this->app['events']);
        //}
        // Here we'll set a reconnector callback. This reconnector can be any callable
        // so we will set a Closure to reconnect from this manager with the name of
        // the connection, which will allow us to reconnect from the connections.
        connection.setReconnector((connection1) => {
            this.reconnect(connection1.getName());
        });
        return connection;
    }
    /**
     * Prepare the read write mode for database connection instance.
     *
     * @param  \Illuminate\Database\Connection  $connection
     * @param  string  $type
     * @return \Illuminate\Database\Connection
     */
    protected virtual Connection setPdoForType(Connection connection, string type = null)
    {
        if (type == "read") {
            connection.setPdo(connection.getReadPdo());
        } else if (type == "write") {
            connection.setReadPdo(connection.getPdo());
        }
        return connection;
    }
    /**
     * Get the configuration for a connection.
     *
     * @param  string  $name
     * @return array
     *
     * @throws \InvalidArgumentException
     */
    protected virtual ApplicationDatabaseConnectionConfig getConfig(string name)
    {
        name = name ?? this.getDefaultConnection();
        // To get the database connection configuration, we will just pull each of the
        // connection configurations and get the configurations for the given name.
        // If the configuration doesn't exist, we'll throw an exception and bail.

        Dictionary<string, ApplicationDatabaseConnectionConfig> connections = this._app.make<ApplicationDatabaseConfig>().connections;
        if (!connections.ContainsKey(name)) {
            throw new Exception("Database [" + name + "] not configured.");
        }
        return connections[name];
    }
    /**
     * Get the default connection name.
     *
     * @return string
     */
    public virtual string getDefaultConnection()
    {
        return this._app.make<ApplicationDatabaseConfig>().defaults;
    }
    /**
     * Set the default connection name.
     *
     * @param  string  $name
     * @return void
     */
    public virtual void setDefaultConnection(string name)
    {
        this._app.make<ApplicationDatabaseConfig>().defaults = name;
    }

    /**
     * Register an extension connection resolver.
     *
     * @param  string    $name
     * @param  callable  $resolver
     * @return void
     */
    public void extend(string name, Func<ApplicationDatabaseConnectionConfig, string, Connection> resolver)
    {
        this._extensions[name] = resolver;
    }

    /**
     * Return all of the created connections.
     *
     * @return array
     */
    public Dictionary<string, Connection> getConnections()
    {
        return this._connections;
    }
    }
}

using System;
using System.Collections.Generic;

namespace Icy.Database
{
    public class ConnectionResolver
    {
        /**
     * All of the registered connections.
     *
     * @var array
     */
        protected Dictionary<string, ConnectionInterface> _connections = new Dictionary<string, ConnectionInterface>();

        /**
         * The default connection name.
         *
         * @var string
         */
        protected string _default;

        /**
         * Create a new connection resolver instance.
         *
         * @param  array  $connections
         * @return void
         */
        public ConnectionResolver(Dictionary<string, ConnectionInterface> connections)
        {
            foreach (var e in connections)
            {
                this.addConnection(e.Key, e.Value);
            }
        }

        /**
         * Get a database connection instance.
         *
         * @param  string  $name
         * @return \Illuminate\Database\ConnectionInterface
         */
        public ConnectionInterface connection(string name = null)
        {
            if (name == null)
            {
                name = this.getDefaultConnection();
            }

            return this._connections[name];
        }

        /**
         * Add a connection to the resolver.
         *
         * @param  string  $name
         * @param  \Illuminate\Database\ConnectionInterface  $connection
         * @return void
         */
        public void addConnection(string name, ConnectionInterface connection)
        {
            this._connections[name] = connection;
        }

        /**
         * Check if a connection has been registered.
         *
         * @param  string  $name
         * @return bool
         */
        public bool hasConnection(string name)
        {
            return this._connections.ContainsKey(name);
        }

        /**
         * Get the default connection name.
         *
         * @return string
         */
        public string getDefaultConnection()
        {
            return this._default;
        }

        /**
         * Set the default connection name.
         *
         * @param  string  $name
         * @return void
         */
        public void setDefaultConnection(string name)
        {
            this._default = name;
        }
    }
}

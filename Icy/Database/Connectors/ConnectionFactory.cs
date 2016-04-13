using System;
using System.Collections.Generic;
using System.Text;
using Icy.Foundation;
using Icy.Util;

namespace Icy.Database.Connectors
{
    // f706250  5 Gen 2016
    public class ConnectionFactory
    {

        public ConnectionFactory(Application app)
        {

        }

        /**
         * Establish a PDO connection based on the configuration.
         *
         * @param  array   $config
         * @param  string  $name
         * @return \Illuminate\Database\Connection
         */
        public virtual Connection make(ApplicationDatabaseConnectionConfig config1, string name = null)
        {
            ApplicationDatabaseConnectionConfig config = this.parseConfig(config1, name);
            if (config.read != null) {
                return this.createReadWriteConnection(config);
            }
            return this.createSingleConnection(config);
        }
        /**
         * Create a single database connection instance.
         *
         * @param  array  $config
         * @return \Illuminate\Database\Connection
         */
        protected virtual Connection createSingleConnection(ApplicationDatabaseConnectionConfig config)
        {
            Func<object> pdo = () => {
                return this.createConnector(config).connect(config);
            };
            return this.createConnection(config.driver, pdo, config.database, config.prefix, config);
        }
        /**
         * Create a single database connection instance.
         *
         * @param  array  $config
         * @return \Illuminate\Database\Connection
         */
        protected virtual Connection createReadWriteConnection(ApplicationDatabaseConnectionConfig config)
        {
            Connection connection = this.createSingleConnection(this.getWriteConfig(config));
            return connection.setReadPdo(this.createReadPdo(config));
        }
        /**
         * Create a new PDO instance for reading.
         *
         * @param  array  $config
         * @return \PDO
         */
        protected virtual object createReadPdo(ApplicationDatabaseConnectionConfig config)
        {
            ApplicationDatabaseConnectionConfig readConfig = this.getReadConfig(config);
            return this.createConnector(readConfig).connect(readConfig);
        }
        /**
         * Get the read configuration for a read / write connection.
         *
         * @param  array  $config
         * @return array
         */
        protected virtual ApplicationDatabaseConnectionConfig getReadConfig(ApplicationDatabaseConnectionConfig config)
        {
           ApplicationDatabaseConnectionMergeableConfig readConfig = this.getReadWriteConfig(config, "read");
            return this.mergeReadWriteConfig(config, readConfig);
        }
        /**
         * Get the read configuration for a read / write connection.
         *
         * @param  array  $config
         * @return array
         */
        protected virtual ApplicationDatabaseConnectionConfig getWriteConfig(ApplicationDatabaseConnectionConfig config)
        {
            ApplicationDatabaseConnectionMergeableConfig writeConfig = this.getReadWriteConfig(config, "write");
            return this.mergeReadWriteConfig(config, writeConfig);
        }
        /**
         * Get a read / write level configuration.
         *
         * @param  array   $config
         * @param  string  $type
         * @return array
         */
        protected ApplicationDatabaseConnectionMergeableConfig getReadWriteConfig(ApplicationDatabaseConnectionConfig config, string type)
        {
            switch (type.ToLower())
            {
                default:
                case "read":
                    return (ApplicationDatabaseConnectionMergeableConfig)config.read;
                case "write":
                    return (ApplicationDatabaseConnectionMergeableConfig)config.write;
            }
        }
        /**
         * Merge a configuration for a read / write connection.
         *
         * @param  array  $config
         * @param  array  $merge
         * @return array
         */
        protected ApplicationDatabaseConnectionConfig mergeReadWriteConfig(ApplicationDatabaseConnectionConfig config, ApplicationDatabaseConnectionMergeableConfig extendConfig)
        {
            ApplicationDatabaseConnectionConfig newConfig = config;
            if (extendConfig.name != null) newConfig.name = extendConfig.name;
            if (extendConfig.appname != null) newConfig.appname = extendConfig.appname;
            if (extendConfig.driver != null) newConfig.driver = extendConfig.driver;
            if (extendConfig.host != null) newConfig.host = extendConfig.host;
            if (extendConfig.port != null) newConfig.port = extendConfig.port;
            if (extendConfig.prefix != null) newConfig.prefix = extendConfig.prefix;
            if (extendConfig.database != null) newConfig.database = extendConfig.database;
            if (extendConfig.username != null) newConfig.username = extendConfig.username;
            if (extendConfig.password != null) newConfig.password = extendConfig.password;
            newConfig.read = null;
            newConfig.write = null;
            return newConfig;
        }
        /**
         * Parse and prepare the database configuration.
         *
         * @param  array   $config
         * @param  string  $name
         * @return array
         */
        protected ApplicationDatabaseConnectionConfig parseConfig(ApplicationDatabaseConnectionConfig config, string name)
        {
            ApplicationDatabaseConnectionConfig newConfig = config;
            newConfig.prefix = "";
            newConfig.name = name;
            return newConfig;
        }
        /**
         * Create a connector instance based on the configuration.
         *
         * @param  array  $config
         * @return \Illuminate\Database\Connectors\ConnectorInterface
         *
         * @throws \InvalidArgumentException
         */
        public ConnectorInterface createConnector(ApplicationDatabaseConnectionConfig config)
        {
            if (config.driver == null) {
                throw new Exception("A driver must be specified.");
            }
            switch (config.driver) {
                case "sqlsrv":
                    return new SqlServerConnector();
            }
            throw new Exception("Unsupported driver [" + config.driver + "]");
        }
        /**
         * Create a new connection instance.
         *
         * @param  string   $driver
         * @param  \PDO|\Closure     $connection
         * @param  string   $database
         * @param  string   $prefix
         * @param  array    $config
         * @return \Illuminate\Database\Connection
         *
         * @throws \InvalidArgumentException
         */
        protected Connection createConnection(string driver, Func<object> connection, string database, string prefix = "", ApplicationDatabaseConnectionConfig config = default(ApplicationDatabaseConnectionConfig))
        {
            switch (driver) {
                case "sqlsrv":
                    return new SqlServerConnection(connection, database, prefix, config);
            }
            throw new NotImplementedException("Unsupported driver [" + driver + "]");
        }
    }
}

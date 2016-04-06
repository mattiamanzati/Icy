using System;
using System.Collections.Generic;
using System.Text;
using Icy.Util;
using System.Data.SqlClient;
using Icy.Foundation;

namespace Icy.Database.Connectors
{
    // b273677  4 Feb 2016
    public class SqlServerConnector: Connector, ConnectorInterface
    {

    /**
     * Establish a database connection.
     *
     * @param  array  $config
     * @return \PDO
     */
    public object connect(ApplicationDatabaseConnectionConfig config)
    {
        SqlConnection conn = new SqlConnection(this.getDsn(config));
        try{
            conn.Open();
        }catch(Exception e){
            Console.WriteLine(e);
            throw e;
        }
        return conn;
    }
    /**
     * Create a DSN string from a configuration.
     *
     * @param  array   $config
     * @return string
     */
    protected string getDsn(ApplicationDatabaseConnectionConfig config)
    {
        Dictionary<string, string> arguments = new Dictionary<string, string>();
        arguments["Server"] = this.buildHostString(config, ",");

        if (config.database != null) {
            arguments["Database"] = config.database;
        }
        if (config.appname != null) {
            arguments["APP"] = config.appname;
        }
        if (config.username != null)
        {
            arguments["User Id"] = config.username;
        }
        if (config.password != null)
        {
            arguments["Password"] = config.password;
        }
        if (config.extra.ContainsKey("readonly") && config.extra["readonly"] is bool && (bool)config.extra["readonly"] == true)
        {
            arguments["ApplicationIntent"] = "ReadOnly";
        }
        return this.buildConnectString(arguments);
    }

    /**
     * Build a connection string from the given arguments.
     *
     * @param  string  $driver
     * @param  array  $arguments
     * @return string
     */
    protected string buildConnectString(Dictionary<string, string> arguments)
    {
        string[] options = DictionaryUtil.map(arguments, (key, value) => key + "=" + value);
        return string.Join(";", options);
    }
    /**
     * Build a host string from the given configuration.
     *
     * @param  array  $config
     * @param  string  $separator
     * @return string
     */
    protected string buildHostString(ApplicationDatabaseConnectionConfig config, string separator)
    {
        if (config.port != null) {
            return config.host + separator + config.port;
        } else {
            return config.host;
        }
    }

    }
}

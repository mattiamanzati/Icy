using System;
using System.Collections.Generic;
using System.Text;

namespace Icy.Database
{

    public class ApplicationDatabaseConnectionMergeableConfig
    {
        public string appname;
        public string name;
        public string driver;
        public string host;
        public int? port;
        public string prefix;
        public string database;
        public string username;
        public string password;
    }

    public class ApplicationDatabaseConnectionConfig
    {
        public string appname;
        public string name;
        public string driver;
        public string host;
        public int? port;
        public string prefix;
        public string database;
        public string username;
        public string password;
        public Dictionary<string, object> extra = new Dictionary<string,object>();

        public ApplicationDatabaseConnectionMergeableConfig read;
        public ApplicationDatabaseConnectionMergeableConfig write;
    }

    public class ApplicationDatabaseConfig
    {
        public string defaults;
        public Dictionary<string, ApplicationDatabaseConnectionConfig> connections = new Dictionary<string,ApplicationDatabaseConnectionConfig>();
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Icy.Database
{
    // 011b9ba  on 15 Jul 2015
    public interface ConnectionResolverInterface
    {
        /**
         * Get a database connection instance.
         *
         * @param  string  $name
         * @return \Illuminate\Database\ConnectionInterface
         */
        ConnectionInterface connection(string name = null);
        /**
         * Get the default connection name.
         *
         * @return string
         */
        string getDefaultConnection();
        /**
         * Set the default connection name.
         *
         * @param  string  $name
         * @return void
         */
        void setDefaultConnection(string name);
    }
}

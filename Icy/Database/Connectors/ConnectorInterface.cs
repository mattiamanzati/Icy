using System;
using System.Collections.Generic;
using System.Text;
using Icy.Foundation;

namespace Icy.Database.Connectors
{
    // 99c428b  on 1 Jun 2015
    public interface ConnectorInterface
    {
        /**
         * Establish a database connection.
         *
         * @param  array  $config
         * @return \PDO
         */
        object connect(ApplicationDatabaseConnectionConfig config);
    }
}

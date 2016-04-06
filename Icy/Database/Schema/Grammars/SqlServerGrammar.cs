using System;
using System.Collections.Generic;
using System.Text;

namespace Icy.Database.Schema.Grammars
{
    public class SqlServerGrammar: Grammar
    {
        public override string compileColumnType()
        {
            return "SELECT DATA_TYPE AS data_type FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = ? AND COLUMN_NAME = ?";
        }
        /**
     * Compile the query to determine if a table exists.
     *
     * @return string
     */
        public override string compileTableExists()
        {
            return "select * from sysobjects where type = 'U' and name = ?";
        }

        
    /**
     * Compile the query to determine the list of columns.
     *
     * @param  string  $table
     * @return string
     */
    public override string compileColumnExists()
    {
        return "select col.name from sys.columns as col join sys.objects as obj on col.object_id = obj.object_id where obj.type = 'U' and obj.name = ?";
    }
    }
}

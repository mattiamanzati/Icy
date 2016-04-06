using System;
using System.Collections.Generic;
using System.Text;

namespace Icy.Database.Query.Processors
{
    // eff9e63 3 Aug 2015
    public class SqlServerProcessor: Processor
    {
        /**
         * Process an "insert get ID" query.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  string  $sql
         * @param  array   $values
         * @param  string  $sequence
         * @return int
         */
        public override int processInsertGetId(Builder query, string sql, object[] values, string sequence = null)
        {
            Dictionary<string, object>[] id = query.getConnection().select(String.Concat(sql, "; SELECT SCOPE_IDENTITY() AS last_id"), values);

            if(id.Length > 0){
                int r = 0;
                Int32.TryParse(id[0]["last_id"].ToString(), out r);
                return r;
            }

            return -1;
        }
    }
}

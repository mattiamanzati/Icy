using System;
using System.Collections.Generic;
using System.Text;
using Icy.Util;

namespace Icy.Database.Query.Processors
{
    // 99c428b  on 1 Jun
    public class Processor
    {
        /**
         * Process the results of a "select" query.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $results
         * @return array
         */
        public virtual Dictionary<string, object>[] processSelect(Builder query, Dictionary<string, object>[] results)
        {
            return results;
        }

        /**
         * Process an  "insert get ID" query.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  string  $sql
         * @param  array   $values
         * @param  string  $sequence
         * @return int
         */
        public virtual int processInsertGetId(Builder query, string sql, object[] values, string sequence = null)
        {
            query.getConnection().insert(sql, values);
            return 0;
        }
        /**
         * Process the results of a column listing query.
         *
         * @param  array  $results
         * @return array
         */
        public virtual string[] processColumnListing(Dictionary<string, object>[] results)
        {
            if (results.Length == 0) return new string[0];
            return ArrayUtil.map(results, row => row["name"].ToString());
        }
    }
}

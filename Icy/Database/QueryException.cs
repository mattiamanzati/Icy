using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Icy.Database
{
    public class QueryException : Exception
    {

        /**
         * The SQL for the query.
         *
         * @var string
         */
        protected string _sql;
        /**
         * The bindings for the query.
         *
         * @var array
         */
        protected object[] _bindings;

        protected QueryException _previous = null;
        /**
         * Create a new query exception instance.
         *
         * @param  string  $sql
         * @param  array  $bindings
         * @param  \Exception $previous
         * @return void
         */
        public QueryException(string sql, object[] bindings, QueryException previous)
            : base("", previous)
        {
            this._sql = sql;
            this._bindings = bindings;
            this._previous = previous;
        }

        /**
         * Format the SQL error message.
         *
         * @param  string  $sql
         * @param  array  $bindings
         * @param  \Exception $previous
         * @return string
         */
        protected string formatMessage(string sql, object[] bindings, Exception previous)
        {
            int i = 0;
            return previous.Message + " (SQL: " + (new Regex(@"\?")).Replace(sql, (m) => bindings[i++].ToString()) + ")";
        }
        /**
         * Get the SQL for the query.
         *
         * @return string
         */
        public string getSql()
        {
            return this._sql;
        }
        /**
         * Get the bindings for the query.
         *
         * @return array
         */
        public object[] getBindings()
        {
            return this._bindings;
        }

        public QueryException getPrevious()
        {
            return this._previous;
        }
    }
}

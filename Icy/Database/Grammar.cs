using System;
using System.Collections.Generic;
using System.Text;

namespace Icy.Database
{
    // b030789 Dec 28, 2015
    public abstract class Grammar
    {
        /**
         * The grammar table prefix.
         *
         * @var string
         */
        protected string _tablePrefix = "";


        /**
         * Wrap an array of values.
         *
         * @param  array  $values
         * @return array
         */
        public virtual string[] wrapArray(object[] values)
        {
            string[] result = new string[values.Length];

            for (var i = 0; i < values.Length; i++)
            {
                result[i] = this.wrap(values[i]);
            }

            return result;
        }

        /**
         * Wrap a table in keyword identifiers.
         *
         * @param  string|\Illuminate\Database\Query\Expression  $table
         * @return string
         */

        public virtual string wrapTable(object table)
        {
            if (this.isExpression(table))
            {
                return this.getValue(table);
            }

            return this.wrap(this._tablePrefix + table.ToString(), true);
        }


        /**
         * Wrap a value in keyword identifiers.
         *
         * @param  string|\Illuminate\Database\Query\Expression  $value
         * @param  bool    $prefixAlias
         * @return string
         */
        public virtual string wrap(object value, bool prefixedAlias = false)
        {
            if (this.isExpression(value))
            {
                return this.getValue(value);
            }

            return this.wrap(value.ToString(), prefixedAlias);
        }

        public virtual string wrap(string value, bool prefixedAlias = false)
        {
            string[] segments;

            // If the value being wrapped has a column alias we will need to separate out
            // the pieces so we can wrap each of the segments of the expression on it
            // own, and then joins them both back together with the "as" connector.

            if (value.ToLower().IndexOf(" as ") > -1)
            {
                segments = value.Split(' ');

                if (prefixedAlias)
                {
                    segments[2] = this._tablePrefix + segments[2];
                }

                return this.wrap(segments[0]) + " as " + this.wrapValue(segments[2]);
            }


            segments = value.Split('.');

            string[] wrapped = new string[segments.Length];

            // If the value is not an aliased table expression, we'll just wrap it like
            // normal, so if there is more than one segment, we will wrap the first
            // segments as if it was a table and the rest as just regular values.
            for (var key = 0; key < segments.Length; key++)
            {
                if (key == 0 && segments.Length > 1)
                {
                    wrapped[key] = this.wrapTable(segments[key]);
                }
                else
                {
                    wrapped[key] = this.wrapValue(segments[key]);
                }
            }

            return string.Join(".", wrapped);
        }

        /**
         * Wrap a single string in keyword identifiers.
         *
         * @param  string  $value
         * @return string
         */
        protected virtual string wrapValue(string value)
        {
            if(value == "*") return "*";

            return '"' + value.Replace("\"", "\"\"") + '"';
        }


        /**
         * Convert an array of column names into a delimited string.
         *
         * @param  array   $columns
         * @return string
         */
        public virtual string columnize(object[] columns)
        {
            return string.Join(", ", this.wrapArray(columns));
        }


        /**
         * Create query parameter place-holders for an array.
         *
         * @param  array   $values
         * @return string
         */
        public virtual string parameterize(object[] values)
        {
            string[] result = new string[values.Length];

            for (var i = 0; i < values.Length; i++)
            {
                result[i] = this.parameter(values[i]);
            }

            return string.Join(", ", result);
        }


        /**
         * Get the appropriate query parameter place-holder for a value.
         *
         * @param  mixed   $value
         * @return string
         */
        public virtual string parameter(object value)
        {
            return this.isExpression(value) ? this.getValue(value) : "?";
        }


        /**
         * Get the value of a raw expression.
         *
         * @param  \Illuminate\Database\Query\Expression  $expression
         * @return string
         */
        public virtual string getValue(object expression)
        {
            if (expression is Query.Expression)
            {
                return ((Query.Expression)expression).getValue().ToString();
            }
            return expression.ToString();
        }


        /**
         * Determine if the given value is a raw expression.
         *
         * @param  mixed  $value
         * @return bool
         */
        public virtual bool isExpression(object value)
        {
            return value is Query.Expression;
        }


        /**
         * Get the format for database stored dates.
         *
         * @return string
         */
        public virtual string getDateFormat()
        {
            return "Y-m-d H:i:s";
        }


        /**
         * Get the grammar's table prefix.
         *
         * @return string
         */
        public virtual string getTablePrefix()
        {
            return this._tablePrefix;
        }

        /**
         * Set the grammar's table prefix.
         *
         * @param  string  $prefix
         * @return $this
         */
        public virtual void setTablePrefix(string prefix)
        {
            this._tablePrefix = prefix;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Icy.Util;

namespace Icy.Database.Query
{

    public class JoinClauseOptions{
        public object first;
        public string operator1;
        public object second;
        public string boolean;
        public bool where;
        public bool nested;
        public JoinClause join;
    }

    // 4d8e4bb Dec 28, 2015
    public class JoinClause
    {
        /**
         * The type of join being performed.
         *
         * @var string
         */
        public string _type;

        /**
         * The table the join clause is joining to.
         *
         * @var string
         */
        public string _table;

        /**
         * The "on" clauses for the join.
         *
         * @var array
         */
        public JoinClauseOptions[] _clauses = new JoinClauseOptions[0];

        /**
         * The "on" bindings for the join.
         *
         * @var array
         */
        public object[] _bindings = new object[0];

        /**
         * Create a new join clause instance.
         *
         * @param  string  type
         * @param  string  table
         * @return void
         */
        public JoinClause(string type, string table)
        {
            this._type = type;
            this._table = table;
        }


        /**
         * Add an "on" clause to the join.
         *
         * On clauses can be chained, e.g.
         *
         *  join.on('contacts.user_id', '=', 'users.id')
         *       .on('contacts.info_id', '=', 'info.id')
         *
         * will produce the following SQL:
         *
         * on `contacts`.`user_id` = `users`.`id`  and `contacts`.`info_id` = `info`.`id`
         *
         * @param  string  first
         * @param  string|null  operator
         * @param  string|null  second
         * @param  string  boolean
         * @param  bool  where
         * @return this
         */
        public JoinClause on(Action<JoinClause> first, string operator1 = null, object second = null, string boolean = "and", bool where = false)
        {
            return this.nest(first, boolean);
        }

        public JoinClause on(object first, string operator1 = null, object second = null, string boolean = "and", bool where = false)
        {
            if (where)
            {
                this._bindings = ArrayUtil.push(this._bindings, second);
            }

            if(where && (operator1 == "in" || operator1 == "not in") && (second is IList<object> || second is object[])){
                second = ((IList<object>)second).Count;
            }

            JoinClauseOptions options = new JoinClauseOptions();
            options.first = first;
            options.operator1 = operator1;
            options.second = second;
            options.boolean = boolean;
            options.where = where;
            options.nested = false;

            this._clauses = ArrayUtil.push(this._clauses, options);

            return this;
        }

        /**
         * Add an "or on" clause to the join.
         *
         * @param  string  first
         * @param  string|null  operator
         * @param  string|null  second
         * @return \Illuminate\Database\Query\JoinClause
         */
        public JoinClause orOn(object first, string operator1 = null, object second = null)
        {
            return this.on(first, operator1, second, "or");
        }

        /**
         * Add an "on where" clause to the join.
         *
         * @param  string  first
         * @param  string|null  operator
         * @param  string|null  second
         * @param  string  boolean
         * @return \Illuminate\Database\Query\JoinClause
         */
        public JoinClause where(object first, string operator1 = null, object second = null, string boolean = "and")
        {
            return this.on(first, operator1, second, boolean, true);
        }

        /**
         * Add an "or on where" clause to the join.
         *
         * @param  string  first
         * @param  string|null  operator
         * @param  string|null  second
         * @return \Illuminate\Database\Query\JoinClause
         */
        public JoinClause orWhere(object first, string operator1 = null, object second = null)
        {
            return this.on(first, operator1, second, "or", true);
        }

        /**
         * Add an "on where is null" clause to the join.
         *
         * @param  string  column
         * @param  string  boolean
         * @return \Illuminate\Database\Query\JoinClause
         */
        public JoinClause whereNull(object column, string boolean = "and")
        {
            return this.on(column, "is", new Expression("null"), boolean, false);
        }

        /**
         * Add an "or on where is null" clause to the join.
         *
         * @param  string  column
         * @return \Illuminate\Database\Query\JoinClause
         */
        public JoinClause orWhereNull(object column)
        {
            return this.whereNull(column, "or");
        }

        /**
         * Add an "on where is not null" clause to the join.
         *
         * @param  string  column
         * @param  string  boolean
         * @return \Illuminate\Database\Query\JoinClause
         */
        public JoinClause whereNotNull(object column, string boolean = "and")
        {
            return this.on(column, "is", new Expression("not null"), boolean, false);
        }

        /**
         * Add an "or on where is not null" clause to the join.
         *
         * @param  string  column
         * @return \Illuminate\Database\Query\JoinClause
         */
        public JoinClause orWhereNotNull(object column)
        {
            return this.whereNotNull(column, "or");
        }

        /**
         * Add an "on where in (...)" clause to the join.
         *
         * @param  string  column
         * @param  array  values
         * @return \Illuminate\Database\Query\JoinClause
         */
        public JoinClause whereIn(object column, object[] values)
        {
            return this.on(column, "in", values, "and", true);
        }

        /**
         * Add an "on where not in (...)" clause to the join.
         *
         * @param  string  column
         * @param  array  values
         * @return \Illuminate\Database\Query\JoinClause
         */
        public JoinClause whereNotIn(object column, object[] values)
        {
            return this.on(column, "not in", values, "and", true);
        }

        /**
         * Add an "or on where in (...)" clause to the join.
         *
         * @param  string  column
         * @param  array  values
         * @return \Illuminate\Database\Query\JoinClause
         */
        public JoinClause orWhereIn(object column, object[] values)
        {
            return this.on(column, "in", values, "or", true);
        }

        /**
         * Add an "or on where not in (...)" clause to the join.
         *
         * @param  string  column
         * @param  array  values
         * @return \Illuminate\Database\Query\JoinClause
         */
        public JoinClause orWhereNotIn(object column, object[] values)
        {
            return this.on(column, "not in", values, "or", true);
        }

        
        /**
         * Add a nested where statement to the query.
         *
         * @param  \Closure  $callback
         * @param  string   $boolean
         * @return \Illuminate\Database\Query\JoinClause
         */
        public JoinClause nest(Action<JoinClause> callback, string boolean = "and")
        {
            JoinClause join = new JoinClause(this._type, this._table);
            
            callback(join);

            if (join._clauses.Length > 0) {

                JoinClauseOptions options = new JoinClauseOptions();
                options.nested = true;
                options.join = join;
                options.boolean = boolean;

                this._clauses = ArrayUtil.push(this._clauses, options);
                this._bindings = ArrayUtil.concat(this._bindings, join._bindings);
            }

            return this;
        }
    }
}

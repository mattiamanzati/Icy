using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Icy.Support;
using Icy.Util;
using BaseGrammar = Icy.Database.Query.Grammars.Grammar;
using BaseProcessor = Icy.Database.Query.Processors.Processor;

namespace Icy.Database.Query
{
    public class WhereOptions
    {
        public string type;
        public object column;
        public string operator1;
        public object value;
        public object[] values;
        public string boolean;
        public string sql;
        public bool not;
        public Builder query;
    }

    public class HavingOptions
    {
        public string type;
        public object column;
        public string operator1;
        public object value;
        public string boolean;
        public string sql;
    }

    public class AggregateOptions
    {
        public string function;
        public object[] columns;
    }

    public class OrderOptions
    {
        public string type;
        public string sql;
        public object column;
        public string direction;
    }

    public class UnionOptions
    {
        public Builder query;
        public bool all;
    }

    // 99bc4c3  Apr 1, 2016
    public class Builder
    {

        /**
         * The database connection instance.
         *
         * @var \Illuminate\Database\Connection
         */
        protected ConnectionInterface _connection = null;

        /**
         * The database query grammar instance.
         *
         * @var \Illuminate\Database\Query\Grammars\Grammar
         */
        protected BaseGrammar _grammar = null;

        /**
         * The database query post processor instance.
         *
         * @var \Illuminate\Database\Query\Processors\Processor
         */
        protected BaseProcessor _processor = null;

        /**
         * The current query value bindings.
         *
         * @var array
         */
        protected Dictionary<string, object[]> _bindings = new Dictionary<string, object[]>()
        {
            {"select", new object[0]},
            {"join", new object[0]},
            {"where", new object[0]},
            {"having", new object[0]},
            {"order", new object[0]},
            {"union", new object[0]}
        };

        /**
         * An aggregate function and column to be run.
         *
         * @var array
         */
        public AggregateOptions _aggregate = null;

        /**
         * The columns that should be returned.
         *
         * @var array
         */
        public object[] _columns = new object[0];

        /**
         * Indicates if the query returns distinct results.
         *
         * @var bool
         */
        public bool _distinct = false;

        /**
         * The table which the query is targeting.
         *
         * @var string
         */
        public object _from = null;

        /**
         * The table joins for the query.
         *
         * @var array
         */
        public JoinClause[] _joins = new JoinClause[0];

        /**
         * The where constraints for the query.
         *
         * @var array
         */
        public WhereOptions[] _wheres = new WhereOptions[0];


        /**
         * The groupings for the query.
         *
         * @var array
         */
        public object[] _groups = new object[0];


        /**
         * The having constraints for the query.
         *
         * @var array
         */
        public HavingOptions[] _havings = new HavingOptions[0];

        /**
         * The orderings for the query.
         *
         * @var array
         */
        public OrderOptions[] _orders = new OrderOptions[0];


        /**
         * The maximum number of records to return.
         *
         * @var int
         */
        public int? _limit = null;


        /**
         * The number of records to skip.
         *
         * @var int
         */
        public int? _offset = null;

        /**
         * The query union statements.
         *
         * @var array
         */
        public UnionOptions[] _unions = new UnionOptions[0];


        /**
         * The maximum number of union records to return.
         *
         * @var int
         */
        public int? _unionLimit = null;


        /**
         * The number of union records to skip.
         *
         * @var int
         */
        public int? _unionOffset = null;

        /**
         * The orderings for the union query.
         *
         * @var array
         */
        public OrderOptions[] _unionOrders = new OrderOptions[0];

        
        /**
         * Indicates whether row locking is being used.
         *
         * @var string|bool
         */
        public string _lock;

        /**
         * The field backups currently in use.
         *
         * @var array
         */
        protected Dictionary<string, object> _backups = new Dictionary<string,object>();

        /**
         * The binding backups currently in use.
         *
         * @var array
         */
        protected Dictionary<string, object[]> _bindingBackups = new Dictionary<string, object[]>();

        
        /**
         * All of the available clause operators.
         *
         * @var array
         */
        protected string[] _operators = new string[]{
            "=", "<", ">", "<=", ">=", "<>", "!=",
            "like", "like binary", "not like", "between", "ilike",
            "&", "|", "^", "<<", ">>",
            "rlike", "regexp", "not regexp",
            "~", "~*", "!~", "!~*", "similar to",
            "not similar to"
        };


        /**
         * Whether use write pdo for select.
         *
         * @var bool
         */
        protected bool _useWritePdo = false;

        /**
         * Create a new query builder instance.
         *
         * @param  \Illuminate\Database\ConnectionInterface  $connection
         * @param  \Illuminate\Database\Query\Grammars\Grammar  $grammar
         * @param  \Illuminate\Database\Query\Processors\Processor  $processor
         * @return void
         */
        public Builder(ConnectionInterface connection, BaseGrammar grammar = null, BaseProcessor processor = null)
        {
            this._grammar = grammar ?? connection.getQueryGrammar();
            this._processor = processor ?? connection.getPostProcessor();
            this._connection = connection;
        }

        /**
         * Support method to clone the Builder object instance
         **/
        public virtual Builder clone(){
            Builder q = new Builder(this._connection, this._grammar, this._processor);
            q._bindings = new Dictionary<string, object[]>(this._bindings);
            q._aggregate = this._aggregate;
            q._columns = ArrayUtil.copy(this._columns);
            q._distinct = this._distinct;
            q._from = this._from;
            q._joins = ArrayUtil.copy(this._joins);
            q._wheres = ArrayUtil.copy(this._wheres);
            q._groups = ArrayUtil.copy(this._groups);
            q._havings = ArrayUtil.copy(this._havings);
            q._orders = ArrayUtil.copy(this._orders);
            q._limit = this._limit;
            q._offset = this._offset;
            q._unions = ArrayUtil.copy(this._unions);
            q._unionLimit = this._unionLimit;
            q._unionOffset = this._unionOffset;
            q._unionOrders = ArrayUtil.copy(this._unionOrders);
            q._lock = this._lock;
            q._backups = new Dictionary<string, object>(this._backups);
            q._bindingBackups = new Dictionary<string, object[]>(this._bindingBackups);

            return q;
        }


        /**
         * Set the columns to be selected.
         *
         * @param  array|mixed  $columns
         * @return $this
         */

        public virtual Builder select(params object[] columns)
        {
            if (columns.Length == 1 && columns[0] is object[]) return this.select(columns[0]);

            columns = columns.Length > 0 ? columns : new object[] { "*" };

            this._columns = columns;

            return this;
        }

        public virtual Builder select(Expression column)
        {
            return this.select(new object[] { column });
        }


        /**
         * Add a new "raw" select expression to the query.
         *
         * @param  string  $expression
         * @param  array   $bindings
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder selectRaw(string expression, object[] bindings = null)
        {
            this.addSelect(new Expression(expression));

            if (bindings != null)
            {
                this.addBinding(bindings, "select");
            }

            return this;
        }


        /**
         * Add a subselect expression to the query.
         *
         * @param  \Closure|\Illuminate\Database\Query\Builder|string $query
         * @param  string  $as
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder selectSub(string query, string as1, object[] bindings)
        {
            return this.selectRaw("(" + query + ") as " + this._grammar.wrap(as1), bindings);
        }

        public virtual Builder selectSub(Builder query, string as1)
        {
            return this.selectSub(query.toSql(), as1, query.getBindings());
        }

        public virtual Builder selectSub(Action<Builder> callback, string as1)
        {
            Builder query = this.newQuery();
            callback(query);
            return this.selectSub(query, as1);
        }


        /**
         * Add a new select column to the query.
         *
         * @param  array|mixed  $column
         * @return $this
         */
        public virtual Builder addSelect(params object[] columns)
        {
            if (columns.Length == 1 && columns[0] is object[]) return this.addSelect(columns[0]);

            this._columns = ArrayUtil.concat(this._columns, columns);

            return this;
        }

        public virtual Builder addSelect(Expression column)
        {
            return this.addSelect(new object[] { column });
        }


        /**
         * Force the query to only return distinct results.
         *
         * @return $this
         */
        public virtual Builder distinct()
        {
            this._distinct = true;

            return this;
        }


        /**
         * Set the table which the query is targeting.
         *
         * @param  string  $table
         * @return $this
         */
        public virtual Builder from(object table)
        {
            this._from = table;

            return this;
        }


        /**
         * Add a join clause to the query.
         *
         * @param  string  $table
         * @param  string  $one
         * @param  string  $operator
         * @param  string  $two
         * @param  string  $type
         * @param  bool    $where
         * @return $this
         */
        public virtual Builder join(string table, Action<JoinClause> callback, string operator1 = null, object two = null, string type1 = "inner", bool where = false)
        {
            JoinClause join = new JoinClause(type1, table);

            this._joins = ArrayUtil.push(this._joins, join);

            callback(join);

            this.addBinding(join._bindings, "join");

            return this;
        }

        public virtual Builder join(string table, object one, string operator1, object two, string type1 = "inner", bool where = false)
        {
            JoinClause join = new JoinClause(type1, table);

            this._joins = ArrayUtil.push(this._joins, join.on(one, operator1, two, "and", where));

            this.addBinding(join._bindings, "join");

            return this;
        }

        /**
         * Add a "join where" clause to the query.
         *
         * @param  string  $table
         * @param  string  $one
         * @param  string  $operator
         * @param  string  $two
         * @param  string  $type
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder joinWhere(string table, object one, string operator1, object two, string type = "inner")
        {
            return this.join(table, one, operator1, two, type, true);
        }

        public virtual Builder joinWhere(string table, Action<JoinClause> callback, string operator1 = null, object two = null, string type = "inner")
        {
            return this.join(table, callback, operator1, two, type, true);
        }

        /**
         * Add a left join to the query.
         *
         * @param  string  $table
         * @param  string  $first
         * @param  string  $operator
         * @param  string  $second
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder leftJoin(string table, object one, string operator1, object two)
        {
            return this.join(table, one, operator1, two, "left");
        }

        public virtual Builder leftJoin(string table, Action<JoinClause> callback, string operator1 = null, object two = null)
        {
            return this.join(table, callback, operator1, two, "left");
        }


        /**
         * Add a "left join where" clause to the query.
         *
         * @param  string  $table
         * @param  string  $one
         * @param  string  $operator
         * @param  string  $two
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder leftJoinWhere(string table, object one, string operator1, object two)
        {
            return this.join(table, one, operator1, two, "left", true);
        }

        public virtual Builder leftJoinWhere(string table, Action<JoinClause> callback, string operator1 = null, object two = null)
        {
            return this.join(table, callback, operator1, two, "left", true);
        }

        /**
         * Add a left join to the query.
         *
         * @param  string  $table
         * @param  string  $first
         * @param  string  $operator
         * @param  string  $second
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder rightJoin(string table, object one, string operator1, object two)
        {
            return this.join(table, one, operator1, two, "right");
        }

        public virtual Builder rightJoin(string table, Action<JoinClause> callback, string operator1 = null, object two = null)
        {
            return this.join(table, callback, operator1, two, "right");
        }


        /**
         * Add a "left join where" clause to the query.
         *
         * @param  string  $table
         * @param  string  $one
         * @param  string  $operator
         * @param  string  $two
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder rightJoinWhere(string table, object one, string operator1, object two)
        {
            return this.join(table, one, operator1, two, "right", true);
        }

        public virtual Builder rightJoinWhere(string table, Action<JoinClause> callback, string operator1 = null, object two = null)
        {
            return this.join(table, callback, operator1, two, "right", true);
        }

             /**
     * Add a "cross join" clause to the query.
     *
     * @param  string  $table
     * @return \Illuminate\Database\Query\Builder|static
     */
    public virtual Builder crossJoin(string table)
    {
        this._joins = ArrayUtil.push(this._joins, new JoinClause("cross", table));

        return this;
    }

    /**
     * Apply the callback's query changes if the given "value" is true.
     *
     * @param  bool  $value
     * @param  \Closure  $callback
     * @return \Illuminate\Database\Query\Builder
     */
    public virtual Builder when(bool value, Func<Builder, Builder> callback)
    {
        var builder = this;

        if (value) {
                    builder = callback(builder);
                }

        return builder;
    }

    /**
     * Add a basic where clause to the query.
     *
     * @param  string|array|\Closure  $column
     * @param  string  $operator
     * @param  mixed   $value
     * @param  string  $boolean
     * @return $this
     *
     * @throws \InvalidArgumentException
     */

    public virtual Builder where(Dictionary<object, object> column, string operator1 = null, object value = null, string boolean = "and")
        {
            // If the column is an array, we will assume it is an array of key-value pairs
            // and can add them each as a where clause. We will maintain the boolean we
            // received when the method was called and pass it into the nested where.
            return this.where(new Dictionary<object, object>[]{ column }, operator1, value, boolean);
        }

        /**
         * Add an array of where clauses to the query.
         *
         * @param  array  $column
         * @param  string  $boolean
         * @return $this
         */
        public virtual Builder where(Dictionary<object, object>[] columns, string operator1 = null, object value = null, string boolean = "and")
        {
            // If the column is an array, we will assume it is an array of key-value pairs
            // and can add them each as a where clause. We will maintain the boolean we
            // received when the method was called and pass it into the nested where.
            return this.whereNested((query) =>
            {
                foreach (var row in columns)
                {
                    foreach (var e in row)
                    {
                        query.where(e.Key, "=", e.Value);
                    }
                }
            }, boolean);
        }

        public virtual Builder where(object column, object value){
            // Here we will make some assumptions about the operator. If only 2 values are
            // passed to the method, we will assume that the operator is an equals sign
            // and keep going. Otherwise, we'll require the operator to be passed in.
            if (this.invalidOperatorAndValue("=", value)) {
                throw new System.ArgumentException("Illegal operator and value combination.");
            }

            return this.where(column, "=", value);
        }

        public virtual Builder where(Action<Builder> query, string operator1 = null, object value = null, string boolean = "and"){
            // If the columns is actually a Closure instance, we will assume the developer
            // wants to begin a nested where statement which is wrapped in parenthesis.
            // We'll add that Closure to the query then return back out immediately.
            return this.whereNested(query, boolean);
        }


        public virtual Builder where(object column, string operator1 = null, object value = null, string boolean = "and")
        {
            if(value is Action<Builder>)
            {
                // If the value is a Closure, it means the developer is performing an entire
                // sub-select within the query and we will need to compile the sub-select
                // within the where clause to get the appropriate query record results.
                return this.whereSub(column, operator1, value as Action<Builder>, boolean);
            }

            // If the value is "null", we will just assume the developer wants to add a
            // where null clause to the query. So, we will allow a short-cut here to
            // that method for convenience so the developer doesn't have to check.
            if (value == null) {
                return this.whereNull(column, boolean, operator1 != "=");
            }
            // Now that we are working with just a simple query we can put the elements
            // in our array and add the query binding to our array of bindings that
            // will be bound to each SQL statements when it is finally executed.
            WhereOptions options = new WhereOptions();
            options.type = "Basic";
            options.column = column;
            options.operator1 = operator1;
            options.value = value;
            options.boolean = boolean;

            this._wheres = ArrayUtil.push(this._wheres, options);

            if (!(value is Expression)) {
                this.addBinding(value, "where");
            }

            return this;
        }

        /**
         * Add an "or where" clause to the query.
         *
         * @param  string  $column
         * @param  string  $operator
         * @param  mixed   $value
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder orWhere(object column, string operator1 = null, object value = null)
        {
            return this.where(column, operator1, value, "or");
        }

        /**
         * Determine if the given operator and value combination is legal.
         *
         * @param  string  $operator
         * @param  mixed  $value
         * @return bool
         */
        protected bool invalidOperatorAndValue(string operator1, object value)
        {
            bool isOperator = ArrayUtil.indexOf(this._operators, operator1) > -1;
            return isOperator && operator1 != "=" && value == null;
        }

        /**
         * Add a raw where clause to the query.
         *
         * @param  string  $sql
         * @param  array   $bindings
         * @param  string  $boolean
         * @return $this
         */
        public virtual Builder whereRaw(string sql, object[] bindings = null, string boolean = "and")
        {
            bindings = bindings ?? new object[0];

            WhereOptions options = new WhereOptions();
            options.type = "Raw";
            options.sql = sql;
            options.boolean = boolean;

            this._wheres = ArrayUtil.push(this._wheres, options);

            this.addBinding(bindings, "where");

            return this;
        }

        /**
         * Add a raw or where clause to the query.
         *
         * @param  string  $sql
         * @param  array   $bindings
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder orWhereRaw(string sql, object[] bindings = null)
        {
            return this.whereRaw(sql, bindings, "or");
        }

        /**
         * Add a where between statement to the query.
         *
         * @param  string  $column
         * @param  array   $values
         * @param  string  $boolean
         * @param  bool  $not
         * @return $this
         */
        public virtual Builder whereBetween(object column, object[] values, string boolean = "and", bool not = false)
        {
            WhereOptions options = new WhereOptions();
            options.type = "Between";
            options.column = column;
            options.boolean = boolean;
            options.not = not;

            this._wheres = ArrayUtil.push(this._wheres, options);

            this.addBinding(values, "where");

            return this;
        }

        /**
         * Add an or where between statement to the query.
         *
         * @param  string  $column
         * @param  array   $values
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder orWhereBetween(object column, object[] values)
        {
            return this.whereBetween(column, values, "or");
        }

        /**
         * Add a where not between statement to the query.
         *
         * @param  string  $column
         * @param  array   $values
         * @param  string  $boolean
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder whereNotBetween(object column, object[] values, string boolean = "and")
        {
            return this.whereBetween(column, values, boolean, true);
        }

        /**
         * Add an or where not between statement to the query.
         *
         * @param  string  $column
         * @param  array   $values
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder orWhereNotBetween(object column, object[] values)
        {
            return this.whereNotBetween(column, values, "or");
        }

        /**
         * Add a nested where statement to the query.
         *
         * @param  \Closure $callback
         * @param  string   $boolean
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder whereNested(Action<Builder> callback, string boolean = "and")
        {
            // To handle nested queries we'll actually create a brand new query instance
            // and pass it off to the Closure that we have. The Closure can simply do
            // do whatever it wants to a query then we will store it for compiling.
            Builder query = this.forNestedWhere();
            callback(query);
            return this.addNestedWhereQuery(query, boolean);
        }

        
        /**
        * Create a new query instance for nested where condition.
        *
        * @return \Illuminate\Database\Query\Builder
        */
        public virtual Builder forNestedWhere()
        {
            Builder query = this.newQuery();
 
            return query.from(this._from);
        }

        /**
         * Add another query builder as a nested where to the query builder.
         *
         * @param  \Illuminate\Database\Query\Builder|static $query
         * @param  string  $boolean
         * @return $this
         */
        public virtual Builder addNestedWhereQuery(Builder query, string boolean = "and")
        {
            if (this._wheres.Length > 0) {
                WhereOptions options = new WhereOptions();
                options.type = "Nested";
                options.query = query;
                options.boolean = boolean;

                this._wheres = ArrayUtil.push(this._wheres, options);
                this.addBinding(query.getBindings(), "where");
            }
            return this;
        }
        /**
         * Add a full sub-select to the query.
         *
         * @param  string   $column
         * @param  string   $operator
         * @param  \Closure $callback
         * @param  string   $boolean
         * @return $this
         */
        protected Builder whereSub(object column, string operator1, Action<Builder> callback, string boolean)
        {
            Builder query = this.newQuery();
            // Once we have the query instance we can simply execute it so it can add all
            // of the sub-select's conditions to itself, and then we can cache it off
            // in the array of where clauses for the "main" parent query instance.
            callback(query);

            WhereOptions options = new WhereOptions();
            options.type = "Sub";
            options.column = column;
            options.operator1 = operator1;
            options.query = query;
            options.boolean = boolean;

            this._wheres = ArrayUtil.push(this._wheres, options);
            
            this.addBinding(query.getBindings(), "where");

            return this;
        }
        /**
         * Add an exists clause to the query.
         *
         * @param  \Closure $callback
         * @param  string   $boolean
         * @param  bool     $not
         * @return $this
         */
        public virtual Builder whereExists(Action<Builder> callback, string boolean = "and", bool not = false)
        {
            string type = not ? "NotExists" : "Exists";
            Builder query = this.newQuery();
            // Similar to the sub-select clause, we will create a new query instance so
            // the developer may cleanly specify the entire exists query and we will
            // compile the whole thing in the grammar and insert it into the SQL.
            
            return this.addWhereExistsQuery(query, boolean, not);
        }

             /**
      * Add an exists clause to the query.
      *
      * @param  \Illuminate\Database\Query\Builder $query
      * @param  string  $boolean
      * @param  bool  $not
      * @return $this
      */
     public virtual Builder addWhereExistsQuery(Builder query, string boolean = "and", bool not = false)
     {
         string type = not ? "NotExists" : "Exists";

         this._wheres = ArrayUtil.push(this._wheres, new WhereOptions(){ type = type, query = query, boolean = boolean});
 
         this.addBinding(query.getBindings(), "where");
 
         return this;
     }

        /**
         * Add an or exists clause to the query.
         *
         * @param  \Closure $callback
         * @param  bool     $not
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder orWhereExists(Action<Builder> callback, bool not = false)
        {
            return this.whereExists(callback, "or", not);
        }

        /**
         * Add a where not exists clause to the query.
         *
         * @param  \Closure $callback
         * @param  string   $boolean
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder whereNotExists(Action<Builder> callback, string boolean = "and")
        {
            return this.whereExists(callback, boolean, true);
        }

        /**
         * Add a where not exists clause to the query.
         *
         * @param  \Closure  $callback
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder orWhereNotExists(Action<Builder> callback)
        {
            return this.orWhereExists(callback, true);
        }

        /**
         * Add a "where in" clause to the query.
         *
         * @param  string  $column
         * @param  mixed   $values
         * @param  string  $boolean
         * @param  bool    $not
         * @return $this
         */
        
        public virtual Builder whereIn(object column, Action<Builder> values, string boolean = "and", bool not = false)
        {
            // If the value of the where in clause is actually a Closure, we will assume that
            // the developer is using a full sub-select for this "in" statement, and will
            // execute those Closures, then we can re-construct the entire sub-selects.
            return this.whereInSub(column, values, boolean, not);
        }

        public virtual Builder whereIn(object column, IArrayable values, string boolean = "and", bool not = false){
            return this.whereIn(column, values.toArray<object>(), boolean, not);
        }

        public virtual Builder whereIn(object column, Builder values, string boolean = "and", bool not = false)
        {
            return this.whereInExistingQuery(column, values, boolean, not);
        }

        public virtual Builder whereIn(object column, object[] values, string boolean = "and", bool not = false)
        {
            string type = not ? "NotIn" : "In";

            WhereOptions options = new WhereOptions();
            options.type = type;
            options.column = column;
            options.values = values;
            options.boolean = boolean;

            this._wheres = ArrayUtil.push(this._wheres, options);

            this.addBinding(values, "where");
            return this;
        }

             /**
      * Add a external sub-select to the query.
      *
      * @param  string   $column
      * @param  \Illuminate\Database\Query\Builder|static $query
      * @param  string   $boolean
      * @param  bool     $not
      * @return $this
      */
     protected virtual Builder whereInExistingQuery(object column, Builder query, string boolean, bool not)
     {
         string type = not ? "NotInSub" : "InSub";
 
         this._wheres = ArrayUtil.push(this._wheres, new WhereOptions(){ type = type, column = column, query = query, boolean = boolean});
 
         this.addBinding(query.getBindings(), "where");
 
         return this;
     }

        /**
         * Add an "or where in" clause to the query.
         *
         * @param  string  $column
         * @param  mixed   $values
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder orWhereIn(object column, object[] values)
        {
            return this.whereIn(column, values, "or");
        }

        /**
         * Add a "where not in" clause to the query.
         *
         * @param  string  $column
         * @param  mixed   $values
         * @param  string  $boolean
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder whereNotIn(object column, object[] values, string boolean = "and")
        {
            return this.whereIn(column, values, boolean, true);
        }

        public virtual Builder whereNotIn(object column, Action<Builder> query, string boolean = "and")
        {
            return this.whereIn(column, query, boolean, true);
        }

        /**
         * Add an "or where not in" clause to the query.
         *
         * @param  string  $column
         * @param  mixed   $values
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder orWhereNotIn(object column, object[] values)
        {
            return this.whereNotIn(column, values, "or");
        }

        /**
         * Add a where in with a sub-select to the query.
         *
         * @param  string   $column
         * @param  \Closure $callback
         * @param  string   $boolean
         * @param  bool     $not
         * @return $this
         */
        protected Builder whereInSub(object column, Action<Builder> callback, string boolean, bool not)
        {
            string type = not ? "NotInSub" : "InSub";
            // To create the exists sub-select, we will actually create a query and call the
            // provided callback with the query so the developer may set any of the query
            // conditions they want for the in clause, then we'll put it in this array.
            Builder query = this.newQuery();
            callback(query);

            WhereOptions options = new WhereOptions();
            options.type = type;
            options.column = column;
            options.query = query;
            options.boolean = boolean;

            this._wheres = ArrayUtil.push(this._wheres, options);
            this.addBinding(query.getBindings(), "where");

            return this;
        }

        /**
         * Add a "where null" clause to the query.
         *
         * @param  string  $column
         * @param  string  $boolean
         * @param  bool    $not
         * @return $this
         */
        public virtual Builder whereNull(object column, string boolean = "and", bool not = false)
        {
            string type = not ? "NotNull" : "Null";

            WhereOptions options = new WhereOptions();
            options.type = type;
            options.column = column;
            options.boolean = boolean;

            this._wheres = ArrayUtil.push(this._wheres, options);
            return this;
        }
        /**
         * Add an "or where null" clause to the query.
         *
         * @param  string  $column
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder orWhereNull(object column)
        {
            return this.whereNull(column, "or");
        }

        /**
         * Add a "where not null" clause to the query.
         *
         * @param  string  $column
         * @param  string  $boolean
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder whereNotNull(object column, string boolean = "and")
        {
            return this.whereNull(column, boolean, true);
        }

        /**
         * Add an "or where not null" clause to the query.
         *
         * @param  string  $column
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder orWhereNotNull(object column)
        {
            return this.whereNotNull(column, "or");
        }

        /**
         * Add a "where date" statement to the query.
         *
         * @param  string  $column
         * @param  string   $operator
         * @param  int   $value
         * @param  string   $boolean
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder whereDate(object column, string operator1, object value, string boolean = "and")
        {
            return this.addDateBasedWhere("Date", column, operator1, value, boolean);
        }

             /**
      * Add an "or where date" statement to the query.
      *
      * @param  string  $column
      * @param  string   $operator
      * @param  int   $value
      * @return \Illuminate\Database\Query\Builder|static
      */
     public virtual Builder orWhereDate(object column, string operator1, object value)
     {
         return this.whereDate(column, operator1, value, "or");
     }

        /**
         * Add a "where day" statement to the query.
         *
         * @param  string  $column
         * @param  string   $operator
         * @param  int   $value
         * @param  string   $boolean
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder whereDay(object column, string operator1, int value, string boolean = "and")
        {
            return this.addDateBasedWhere("Day", column, operator1, value, boolean);
        }
        /**
         * Add a "where month" statement to the query.
         *
         * @param  string  $column
         * @param  string   $operator
         * @param  int   $value
         * @param  string   $boolean
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder whereMonth(object column, string operator1, int value, string boolean = "and")
        {
            return this.addDateBasedWhere("Month", column, operator1, value, boolean);
        }
        /**
         * Add a "where year" statement to the query.
         *
         * @param  string  $column
         * @param  string   $operator
         * @param  int   $value
         * @param  string   $boolean
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder whereYear(object column, string operator1, object value, string boolean = "and")
        {
            return this.addDateBasedWhere("Year", column, operator1, value, boolean);
        }
        /**
         * Add a date based (year, month, day) statement to the query.
         *
         * @param  string  $type
         * @param  string  $column
         * @param  string  $operator
         * @param  int  $value
         * @param  string  $boolean
         * @return $this
         */
        protected Builder addDateBasedWhere(string type, object column, string operator1, object value, string boolean = "and")
        {

            WhereOptions options = new WhereOptions();
            options.type = type;
            options.column = column;
            options.boolean = boolean;
            options.operator1 = operator1;
            options.value = value;
            
            this._wheres = ArrayUtil.push(this._wheres, options);

            this.addBinding(value, "where");
            return this;
        }

        /**
         * Handles dynamic "where" clauses to the query.
         *
         * @param  string  $method
         * @param  string  $parameters
         * @return $this
         */
        public virtual Builder dynamicWhere(string method, object[] parameters)
        {
            string finder = method.Substring(5);
            string[] segments = Regex.Split(finder, "(And|Or)(?=[A-Z])");
            // The connector variable will determine which connector will be used for the
            // query condition. We will change it as we come across new boolean values
            // in the dynamic method strings, which could contain a number of these.
            string connector = "and";
            int index = 0;
            foreach (var segment in segments) {
                // If the segment is not a boolean connector, we can assume it is a column's name
                // and we will add it to the query as a new constraint as a where clause, then
                // we can keep iterating through the dynamic method string's segments again.
                if (segment != "And" && segment != "Or") {
                    this.addDynamic(segment, connector, parameters, index);
                    index++;
                }
                // Otherwise, we will store the connector so we know how the next where clause we
                // find in the query should be connected to the previous ones, meaning we will
                // have the proper boolean connector to connect the next where clause found.
                else {
                    connector = segment;
                }
            }
            return this;
        }

        /**
         * Add a single dynamic where clause statement to the query.
         *
         * @param  string  $segment
         * @param  string  $connector
         * @param  array   $parameters
         * @param  int     $index
         * @return void
         */
        protected void addDynamic(string segment, string connector, object[] parameters, int index)
        {
            // Once we have parsed out the columns and formatted the boolean operators we
            // are ready to add it to this query as a where clause just like any other
            // clause on the query. Then we'll increment the parameter index values.
            string boolean = connector.ToLower();
            this.where(StrUtil.snake(segment), "=", parameters[index], boolean);
        }

        /**
         * Add a "group by" clause to the query.
         *
         * @param  array|string  $column,...
         * @return $this
         */

        public virtual Builder groupBy(params object[] args){
            if (args.Length == 1 && args[0] is object[]) return this.groupBy(args[0]);

            this._groups = ArrayUtil.concat(this._groups, args);
            return this;
        }
        /**
         * Add a "having" clause to the query.
         *
         * @param  string  $column
         * @param  string  $operator
         * @param  string  $value
         * @param  string  $boolean
         * @return $this
         */
        public virtual Builder having(object column, string operator1 = null, object value = null, string boolean = "and")
        {
            string type = "basic";

            HavingOptions options = new HavingOptions();
            options.type = type;
            options.column = column;
            options.operator1 = operator1;
            options.value = value;
            options.boolean = boolean;

            this._havings = ArrayUtil.push(this._havings, options);

            if (!(value is Expression)) {
                this.addBinding(value, "having");
            }
            return this;
        }
        /**
         * Add a "or having" clause to the query.
         *
         * @param  string  $column
         * @param  string  $operator
         * @param  string  $value
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder orHaving(object column, string operator1 = null, object value = null)
        {
            return this.having(column, operator1, value, "or");
        }

        /**
         * Add a raw having clause to the query.
         *
         * @param  string  $sql
         * @param  array   $bindings
         * @param  string  $boolean
         * @return $this
         */
        public virtual Builder havingRaw(string sql, object[] bindings = null, string boolean = "and")
        {
            bindings = bindings ?? new object[0];

            HavingOptions options = new HavingOptions();
            options.type = "raw";
            options.sql = sql;
            options.boolean = boolean;

            this._havings = ArrayUtil.push(this._havings, options);

            this.addBinding(bindings, "having");
            return this;
        }

        /**
         * Add a raw or having clause to the query.
         *
         * @param  string  $sql
         * @param  array   $bindings
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder orHavingRaw(string sql, object[] bindings = null)
        {
            return this.havingRaw(sql, bindings, "or");
        }

        /**
         * Add an "order by" clause to the query.
         *
         * @param  string  $column
         * @param  string  $direction
         * @return $this
         */
        public virtual Builder orderBy(object column, string direction = "asc")
        {
            OrderOptions options = new OrderOptions();
            options.column = column;
            options.direction = direction.ToLower() == "asc" ? "asc" : "desc";

            if(this._unions.Length > 0){
                this._unionOrders = ArrayUtil.push(this._unionOrders, options);
            }else{
                this._orders = ArrayUtil.push(this._orders, options);
            }
            return this;
        }

        /**
         * Add an "order by" clause for a timestamp to the query.
         *
         * @param  string  $column
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder latest(string column = "created_at")
        {
            return this.orderBy(column, "desc");
        }

        /**
         * Add an "order by" clause for a timestamp to the query.
         *
         * @param  string  $column
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder oldest(string column = "created_at")
        {
            return this.orderBy(column, "asc");
        }

        /**
         * Add a raw "order by" clause to the query.
         *
         * @param  string  $sql
         * @param  array  $bindings
         * @return $this
         */
        public virtual Builder orderByRaw(string sql, object[] bindings = null)
        {
            bindings = bindings ?? new object[0];

            OrderOptions options = new OrderOptions();
            options.type = "raw";
            options.sql = sql;

            if(this._unions.Length > 0){
                this._unionOrders = ArrayUtil.push(this._unionOrders, options);
            }else{
                this._orders = ArrayUtil.push(this._orders, options);
            }

            this.addBinding(bindings, "order");
            return this;
        }
        
        /**
         * Set the "offset" value of the query.
         *
         * @param  int  $value
         * @return $this
         */
        public virtual Builder offset(int value)
        {
            if(this._unions.Length > 0){
                this._unionOffset = Math.Max(0, value);
            }else{
                this._offset = Math.Max(0, value);
            }

            return this;
        }

        /**
         * Alias to set the "offset" value of the query.
         *
         * @param  int  $value
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder skip(int value)
        {
            return this.offset(value);
        }

        /**
         * Set the "limit" value of the query.
         *
         * @param  int  $value
         * @return $this
         */
        public virtual Builder limit(int value)
        {
            if(value >= 0){
                if(this._unions.Length > 0){
                    this._unionLimit = value;
                }else{
                    this._limit = value;
                }
            }

            return this;
        }
        /**
         * Alias to set the "limit" value of the query.
         *
         * @param  int  $value
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder take(int value)
        {
            return this.limit(value);
        }

        /**
         * Set the limit and offset for a given page.
         *
         * @param  int  $page
         * @param  int  $perPage
         * @return \Illuminate\Database\Query\Builder|static
         */
        public virtual Builder forPage(int page, int perPage = 15)
        {
            return this.skip((page - 1) * perPage).take(perPage);
        }

             /**
     * Constrain the query to the next "page" of results after a given ID.
     *
     * @param  int  $perPage
     * @param  int  $lastId
     * @param  string  $column
     * @return \Illuminate\Database\Query\Builder|static
     */
    public virtual Builder forPageAfterId(int perPage = 15, int lastId = 0, string column = "id")
    {
        return this.select(column)
                    .where(column, ">", lastId)
                    .orderBy(column, "asc")
                    .take(perPage);
    }

    /**
     * Add a union statement to the query.
     *
     * @param  \Illuminate\Database\Query\Builder|\Closure  $query
     * @param  bool  $all
     * @return \Illuminate\Database\Query\Builder|static
     */
    public virtual Builder union(Action<Builder> callback, bool all = false){
            Builder query = this.newQuery();
            callback(query);
            return this.union(query, all);
        }

        public virtual Builder union(Builder query, bool all = false)
        {

            UnionOptions options = new UnionOptions();
            options.query = query;
            options.all = all;

            this._unions = ArrayUtil.push(this._unions, options);

            this.addBinding(query.getBindings(), "union");

            return this;
        }


        /**
         * Add a union all statement to the query.
         *
         * @param  \Illuminate\Database\Query\Builder|\Closure  $query
         * @return \Illuminate\Database\Query\Builder|static
         */

        public virtual Builder unionAll(Builder query)
        {
            return this.union(query, true);
        }
        /**
         * Lock the selected rows in the table.
         *
         * @param  bool  $value
         * @return $this
         */

        public virtual Builder locks(bool value = true)
        {
            this._lock = value ? "lock" : null;
            if (this._lock != null) {
                this.useWritePdo();
            }
            return this;
        }

        /**
         * Lock the selected rows in the table for updating.
         *
         * @return \Illuminate\Database\Query\Builder
         */
        public virtual Builder lockForUpdate()
        {
            return this.locks(true);
        }

        /**
         * Share lock the selected rows in the table.
         *
         * @return \Illuminate\Database\Query\Builder
         */
        public virtual Builder sharedLock()
        {
            return this.locks(false);
        }

        /**
         * Get the SQL representation of the query.
         *
         * @return string
         */
        public string toSql()
        {
            return this._grammar.compileSelect(this);
        }

        /**
         * Execute a query for a single record by ID.
         *
         * @param  int    $id
         * @param  array  $columns
         * @return mixed|static
         */
        public Dictionary<string, object> find(int id, object[] columns = null)
        {
            columns = columns ?? new object[]{ "*" };
            return this.where("id", "=", id).first(columns);
        }
        /**
         * Get a single column's value from the first result of a query.
         *
         * @param  string  $column
         * @return mixed
         */
        public object value(string column)
        {
            Dictionary<string, object> result = this.first(new object[]{ column });
            if(result == null) return null;

            string[] keys = DictionaryUtil.keys(result);

            if (keys.Length == 0) return null;

            return result[keys[0]];
        }
        /**
         * Execute the query and get the first result.
         *
         * @param  array   $columns
         * @return mixed|static
         */
        public Dictionary<string, object> first(object[] columns = null)
        {
            columns = columns ?? new object[]{ "*" };
            Dictionary<string, object>[] results = this.take(1).get(columns);
            return results.Length > 0 ? results[0] : null;
        }
        
        /**
         * Execute the query as a "select" statement.
         *
         * @param  array  $columns
         * @return array|static[]
         */
        public Dictionary<string, object>[] get(object[] columns = null)
        {
            columns = columns ?? new object[]{ "*" };
            object[] original = this._columns;
            if (original == null) {
                this._columns = ArrayUtil.copy(columns);
            }
            Dictionary<string, object>[] results = this._processor.processSelect(this, this.runSelect());
            this._columns = original;
            return results;
        }

        
        
        /**
         * Run the query as a "select" statement against the connection.
         *
         * @return array
         */
        protected Dictionary<string, object>[] runSelect()
        {
            return this._connection.select(this.toSql(), this.getBindings(), !this._useWritePdo);
        }
        
        /**
         * Get the count of the total records for the paginator.
         *
         * @param  array  $columns
         * @return int
         */
        public int getCountForPagination(object[] columns = null)
        {
            columns = columns ?? new object[]{ "*" };
            this.backupFieldsForCount();

            AggregateOptions options = new AggregateOptions();
            options.function = "count";
            options.columns = this.clearSelectAliases(columns);

            this._aggregate = options;
            Dictionary<string, object>[] results = this.get();
            this._aggregate = null;
            this.restoreFieldsForCount();
            if (this._groups != null) {
                return results.Length;
            }
            return results.Length > 0 ? (int) results[0]["aggregate"] : 0;
        }

        /**
         * Backup some fields for the pagination count.
         *
         * @return void
         */
        protected void backupFieldsForCount()
        {
            this._backups["orders"] = this._orders;
            this._backups["limit"] = this._limit;
            this._backups["offset"] = this._offset;
            this._backups["columns"] = this._columns;

            this._bindingBackups["order"] = this._bindings["order"];
            this._bindingBackups["select"] = this._bindings["select"];

            this._orders = null;
            this._limit = 0;
            this._offset = 0;
            this._columns = null;
            this._bindings["order"] = new object[0];
            this._bindings["select"] = new object[0];
        }

        /**
         * Remove the column aliases since they will break count queries.
         *
         * @param  array  $columns
         * @return array
         */
        protected object[] clearSelectAliases(object[] columns)
        {
            return ArrayUtil.map(columns, (column) => {
                if(column is string && ((string)column).ToLower().IndexOf(" as ") > -1){
                    string col = (string)column;
                    return col.Substring(0, col.ToLower().IndexOf(" as "));
                }
                return column;
            });
        }

        /**
         * Restore some fields after the pagination count.
         *
         * @return void
         */
        protected void restoreFieldsForCount()
        {
            this._orders = (OrderOptions[]) this._backups["orders"];
            this._limit = (int?)this._backups["limit"];
            this._offset = (int?) this._backups["offset"];
            this._columns = (object[]) this._backups["columns"];
            this._bindings["order"] = (object[]) this._bindingBackups["order"];
            this._bindings["select"] = (object[]) this._bindingBackups["select"];

            this._backups = new Dictionary<string, object>();
            this._bindingBackups = new Dictionary<string, object[]>();
        }

        /**
         * Chunk the results of the query.
         *
         * @param  int  $count
         * @param  callable  $callback
         * @return bool
         */
        public bool chunk(int count, Func<Dictionary<string, object>[], bool> callback)
        {
            int page = 1;
            Dictionary<string, object>[] results = this.forPage(page, count).get();
            while (results.Length > 0) {
                // On each chunk result set, we will pass them to the callback and then let the
                // developer take care of everything within the callback, which allows us to
                // keep the memory low for spinning through large result sets for working.
                if (callback(results) == false) {
                    return false;
                }
                page++;
                results = this.forPage(page, count).get();
            }
            return true;
        }

             /**
     * Chunk the results of a query by comparing numeric IDs.
     *
     * @param  int  $count
     * @param  callable  $callback
     * @param  string  $column
     * @return bool
     */
    public virtual bool chunkById(int count, Func<Dictionary<string, object>[], bool> callback, string column = "id")
    {
        int lastId;

        var results = this.forPageAfterId(count, 0, column).get();

        while (results.Length != 0) {
            if (callback(results) == false) {
                return false;
            }

            lastId = (int)results[results.Length - 1][column];

            results = this.forPageAfterId(count, lastId, column).get();
        }

        return true;
    }

    /**
     * Execute a callback over each item.
     *
     * We're also saving memory by chunking the results into memory.
     *
     * @param  callable  $callback
     * @param  int  $count
     * @return bool
     */
    public virtual bool each(Func<Dictionary<string, object>, int, bool> callback, int count = 1000)
     {

         if ((this._orders == null || this._orders.Length == 0) && (this._unionOrders == null || this._unionOrders.Length == 0))
         {
             throw new Exception("You should provide a orderBy clause to use each");
         }

         return this.chunk(count, (results) => {
             for(var i = 0; i < results.Length; i++){
                 Dictionary<string, object> item = results[i];
                 if(callback(item, i) == false){
                     return false;
                 }
             }
             return true;
         });
     }

        /**
         * Strip off the table name or alias from a column identifier.
         *
         * @param  string  $column
         * @return string|null
         */
        protected string stripTableForPluck(string column)
        {
            if (column == null) return column;

            string[] parts = column.Split(new char[] { '.', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts[parts.Length - 1];
        }

        /**
         * Concatenate values of a given column as a string.
         *
         * @param  string  $column
         * @param  string  $glue
         * @return string
         */
        public string implode(string column, string glue = "")
        {
            return string.Join(glue, ArrayUtil.map(this.pluck(column), item => item.ToString()));
        }

        /**
         * Get an array with the values of a given column.
         *
         * @param  string  $column
         * @param  string|null  $key
         * @return array
         */
        public object[] pluck(string column)
        {
            string[] columns = this.getListSelect(column);

            return ArrayUtil.map(this.get(columns), row => row[column]);
        }

        public Dictionary<object, object> pluck(string column, string key)
        {
            string[] columns = this.getListSelect(column);
            Dictionary<object, object> result = new Dictionary<object, object>();
            
            ArrayUtil.map(this.get(columns), row => {
                result[row[key]] = row[column];
                return row;
            });

            return result;
        }

        /**
         * Get the columns that should be used in a list array.
         *
         * @param  string  $column
         * @param  string  $key
         * @return array
         */
        protected string[] getListSelect(string column, string key = null)
        {
            string[] select = key == null ? new string[] { column } : new string[] { column, key };

            // If the selected column contains a "dot", we will remove it so that the list
            // operation can run normally. Specifying the table is not needed, since we
            // really want the names of the columns as it is in this resulting array.

            return ArrayUtil.map(select, (col) => col.IndexOf('.') > -1 ? col.Split(new char[]{ '.' }, 2)[1] : col);
        }

        /**
         * Determine if any rows exist for the current query.
         *
         * @return bool|null
         */
        public bool exists()
        {
            string sql = this._grammar.compileExists(this);
            Dictionary<string, object>[] results = this._connection.select(sql, this.getBindings(), !this._useWritePdo);
            if (results.Length > 0) {
                return (bool)results[0]["exists"];
            }
            return false;
        }
        
        /**
         * Retrieve the "count" result of the query.
         *
         * @param  string  $columns
         * @return int
         */
        public int count(object column){
            return this.count(new object[]{ column });
        }
        public int count(object[] columns = null)
        {
            return this.aggregate("count", columns);
        }

        /**
         * Retrieve the minimum value of a given column.
         *
         * @param  string  $column
         * @return float|int
         */
        public T min<T>(object column)
        {
            return this.aggregate<T>("min", new object[] { column });
        }
        /**
         * Retrieve the maximum value of a given column.
         *
         * @param  string  $column
         * @return float|int
         */
        public T max<T>(object column)
        {
            return this.aggregate<T>("max", new object[] { column });
        }

        /**
         * Retrieve the sum of the values of a given column.
         *
         * @param  string  $column
         * @return float|int
         */
        public T sum<T>(object column)
        {
            return this.aggregate<T>("sum", new object[] { column });
        }
        /**
         * Retrieve the average of the values of a given column.
         *
         * @param  string  $column
         * @return float|int
         */
        public T avg<T>(object column)
        {
            return this.aggregate<T>("avg", new object[] { column });
        }
        /**
         * Alias for the "avg" method.
         *
         * @param  string  $column
         * @return float|int
         */
        public T average<T>(object column)
        {
            return this.avg<T>(new object[]{ column });
        }

        /**
         * Execute an aggregate function on the database.
         *
         * @param  string  $function
         * @param  array   $columns
         * @return float|int
         */

        public int aggregate(string function, object[] columns = null)
        {
            return this.aggregate<int>(function, columns);
        }

        public T aggregate<T>(string function, object[] columns = null)
        {
            columns = columns ?? new object[] { "*" };

            AggregateOptions options = new AggregateOptions();
            options.function = function;
            options.columns = columns;

            this._aggregate = options;

            object[] previousColumns = ArrayUtil.copy(this._columns);


            // We will also back up the select bindings since the select clause will be
            // removed when performing the aggregate function. Once the query is run
            // we will add the bindings back onto this query so they can get used.
            object[] previousSelectBindings = ArrayUtil.copy(this._bindings["select"]);

            this._bindings["select"] = new object[0];

            Dictionary<string, object>[] results = this.get(columns);


            // Once we have executed the query, we will reset the aggregate property so
            // that more select queries can be executed against the database without
            // the aggregate value getting in the way when the grammar builds it.
            this._aggregate = null;
            this._columns = previousColumns;
            this._bindings["select"] = previousSelectBindings;

            if (results.Length > 0)
            {
                return (T)results[0]["aggregate"];
            }

            return default(T);
        }

        /**
         * Insert a new record into the database.
         *
         * @param  array  $values
         * @return bool
         */
        public bool insert(Dictionary<string, object> values)
        {
            // Since every insert gets treated like a batch insert, we will make sure the
            // bindings are structured in a way that is convenient for building these
            // inserts statements by verifying the elements are actually an array.

            return this.insert(new Dictionary<string, object>[] { values });
        }

        public bool insert(Dictionary<string, object>[] values)
        {
            if (values.Length == 0) return true;
            values = ArrayUtil.map(values, value => DictionaryUtil.normalize(value));

            object[] bindings = new object[0];

            foreach (var record in values)
            {
                bindings = ArrayUtil.concat(bindings, DictionaryUtil.values(record));
            }

            string sql = this._grammar.compileInsert(this, values);

            bindings = this.cleanBindings(bindings);

            return this._connection.insert(sql, bindings);
        }

        /**
         * Insert a new record and get the value of the primary key.
         *
         * @param  array   $values
         * @param  string  $sequence
         * @return int
         */
        public int insertGetId(Dictionary<string, object> values, string sequence = null)
        {
            values = DictionaryUtil.normalize(values);
            string sql = this._grammar.compileInsertGetId(this, values, sequence);

            object[] values1 = this.cleanBindings(DictionaryUtil.values(values));

            return this._processor.processInsertGetId(this, sql, values1, sequence);
        }

        /**
         * Update a record in the database.
         *
         * @param  array  $values
         * @return int
         */
        public int update(Dictionary<string, object> values)
        {
            values = DictionaryUtil.normalize(values);
            var bindings = ArrayUtil.concat(DictionaryUtil.values(values), this.getBindings());

            string sql = this._grammar.compileUpdate(this, values);

            return this._connection.update(sql, this.cleanBindings(bindings));
        }

             /**
     * Insert or update a record matching the attributes, and fill it with values.
     *
     * @param  array  $attributes
     * @param  array  $values
     * @return bool
     */
    public bool updateOrInsert(Dictionary<string, object> attributes, Dictionary<string, object> values = null)
    {
        values = values ?? new Dictionary<string, object>();
        values = DictionaryUtil.normalize(values);
        attributes = DictionaryUtil.normalize(attributes);

        if (! this.where(attributes).exists()) {
            return this.insert(DictionaryUtil.merge(attributes, values));
        }

        return this.where(attributes).take(1).update(values) > 0;
    }

    /**
     * Increment a column's value by a given amount.
     *
     * @param  string  $column
     * @param  int     $amount
     * @param  array   $extra
     * @return int
     */
    public int increment(string column, int amount = 1, Dictionary<string, object> extra = null)
        {
            extra = extra ?? new Dictionary<string, object>();

            object wrapped = this._grammar.wrap(column);

            extra[column] = this.raw(wrapped + " + " + amount);

            return this.update(extra);
        }

        /**
         * Decrement a column's value by a given amount.
         *
         * @param  string  $column
         * @param  int     $amount
         * @param  array   $extra
         * @return int
         */
        public int decrement(string column, int amount = 1, Dictionary<string, object> extra = null)
        {
            extra = extra ?? new Dictionary<string, object>();

            object wrapped = this._grammar.wrap(column);

            extra[column] = this.raw(wrapped + " - " + amount);

            return this.update(extra);
        }

        /**
         * Delete a record from the database.
         *
         * @param  mixed  $id
         * @return int
         */
        public int delete(object id = null)
        {
            if (id != null)
            {
                this.where("id", "=", id);
            }

            string sql = this._grammar.compileDelete(this);

            return this._connection.delete(sql, this.getBindings());
        }

        /**
         * Run a truncate statement on the table.
         *
         * @return void
         */
        public void truncate()
        {
            foreach(var e in this._grammar.compileTruncate(this)){
                this._connection.statement(e.Key, e.Value);
            }
        }

        /**
         * Get a new instance of the query builder.
         *
         * @return \Illuminate\Database\Query\Builder
         */
        public virtual Builder newQuery()
        {
            return new Builder(this._connection, this._grammar, this._processor);
        }
        /**
         * Merge an array of where clauses and bindings.
         *
         * @param  array  $wheres
         * @param  array  $bindings
         * @return void
         */
        public void mergeWheres(WhereOptions[] wheres, object[] bindings)
        {
            this._wheres = ArrayUtil.concat(this._wheres, wheres);
            this._bindings["where"] = ArrayUtil.concat(this._bindings["where"], bindings);
        }

        /**
         * Remove all of the expressions from a list of bindings.
         *
         * @param  array  $bindings
         * @return array
         */
        protected object[] cleanBindings(object[] bindings)
        {
            return ArrayUtil.filter(bindings, value => !(value is Expression));
        }

        /**
         * Create a raw database expression.
         *
         * @param  mixed  $value
         * @return \Illuminate\Database\Query\Expression
         */
        public Expression raw(object value)
        {
            return this._connection.raw(value);
        }

        /**
         * Get the current query value bindings in a flattened array.
         *
         * @return array
         */
        public object[] getBindings()
        {
            object[] result = new object[0];

            foreach (var e in this._bindings)
            {
                result = ArrayUtil.concat(result, e.Value);
            }

            return result;
        }

        /**
         * Get the raw array of bindings.
         *
         * @return array
         */
        public Dictionary<string, object[]> getRawBindings()
        {
            return this._bindings;
        }

        /**
         * Set the bindings on the query builder.
         *
         * @param  array   $bindings
         * @param  string  $type
         * @return $this
         *
         * @throws \InvalidArgumentException
         */
        public virtual Builder setBindings(object[] bindings, string type1 = "where")
        {

            if (!this._bindings.ContainsKey(type1)) throw new System.ArgumentException("Invalid type of binding", "type1");

            this._bindings[type1] = bindings;

            return this;
        }

        /**
         * Add a binding to the query.
         *
         * @param  mixed   $value
         * @param  string  $type
         * @return $this
         *
         * @throws \InvalidArgumentException
         */
        public virtual Builder addBinding(object[] value, string type1 = "where")
        {

            if (!this._bindings.ContainsKey(type1)) throw new System.ArgumentException("Invalid type of binding", "type1");

            this._bindings[type1] = ArrayUtil.concat(this._bindings[type1], value);

            return this;
        }

        public virtual Builder addBinding(object value, string type1 = "where")
        {
            return this.addBinding(new object[] { value }, type1);
        }

        /**
         * Merge an array of bindings into our bindings.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @return $this
         */
        public virtual Builder mergeBindings(Builder query)
        {
            foreach(var e in query._bindings){
                this._bindings[e.Key] = ArrayUtil.concat(this._bindings[e.Key], query._bindings[e.Key]);
            }

            return this;
        }

        /**
         * Get the database connection instance.
         *
         * @return \Illuminate\Database\ConnectionInterface
         */
        public ConnectionInterface getConnection()
        {
            return this._connection;
        }

        /**
         * Get the database query processor instance.
         *
         * @return \Illuminate\Database\Query\Processors\Processor
         */
        public BaseProcessor getProcessor(){
            return this._processor;
        }

        /**
         * Get the query grammar instance.
         *
         * @return \Illuminate\Database\Query\Grammars\Grammar
         */
        public BaseGrammar getGrammar()
        {
            return this._grammar;
        }

        /**
         * Use the write pdo for query.
         *
         * @return $this
         */
        public virtual Builder useWritePdo(){
            this._useWritePdo = true;
         
            return this;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Icy.Database.Query;
using Icy.Util;
using BaseGrammar = Icy.Database.Query.Grammars.Grammar;
using BaseProcessor = Icy.Database.Query.Processors.Processor;
using SchemaGrammar = Icy.Database.Schema.Grammars.Grammar;
using SchemaBuilder = Icy.Database.Schema.Builder;
using Icy.Foundation;

namespace Icy.Database
{
    public class QueryLogItem
    {
        public string query;
        public object[] bindings;
        public float? time;
    }

    // 99da716 Jan 13, 2016
    public class Connection: ConnectionInterface
    {
         /**
         * The active PDO connection.
         *
         * @var PDO
         */
        protected object _pdo;

        /**
         * The PDO creator
         */
        protected Func<object> _pdoCreator;
        /**
         * The active PDO connection used for reads.
         *
         * @var PDO
         */
        protected object _readPdo;
        /**
         * The reconnector instance for the connection.
         *
         * @var callable
         */
        protected Action<Connection> _reconnector;
        /**
         * The query grammar implementation.
         *
         * @var \Illuminate\Database\Query\Grammars\Grammar
         */
        protected BaseGrammar _queryGrammar;
        /**
         * The schema grammar implementation.
         *
         * @var \Illuminate\Database\Schema\Grammars\Grammar
         */
        protected SchemaGrammar _schemaGrammar;
        /**
         * The query post processor implementation.
         *
         * @var \Illuminate\Database\Query\Processors\Processor
         */
        protected BaseProcessor _postProcessor;
        /**
         * The event dispatcher instance.
         *
         * @var \Illuminate\Contracts\Events\Dispatcher
         */
        protected object _events;
        /**
         * The default fetch mode of the connection.
         *
         * @var int
         */
        protected int _fetchMode = 1;

        /**
         * The number of active transactions.
         *
         * @var int
         */
        protected int _transactions = 0;
        /**
         * All of the queries run against the connection.
         *
         * @var array
         */
        protected QueryLogItem[] _queryLog = new QueryLogItem[0];
        /**
         * Indicates whether queries are being logged.
         *
         * @var bool
         */
        protected bool _loggingQueries = false;
        /**
         * Indicates if the connection is in a "dry run".
         *
         * @var bool
         */
        protected bool _pretending = false;
        /**
         * The name of the connected database.
         *
         * @var string
         */
        protected string _database;
        /**
         * The instance of Doctrine connection.
         *
         * @var \Doctrine\DBAL\Connection
         */
        protected object _doctrineConnection;
        /**
         * The table prefix for the connection.
         *
         * @var string
         */
        protected string _tablePrefix = "";
        /**
         * The database connection configuration options.
         *
         * @var array
         */
        protected ApplicationDatabaseConnectionConfig _config = new ApplicationDatabaseConnectionConfig();
        /**
         * Create a new database connection instance.
         *
         * @param  \PDO     $pdo
         * @param  string   $database
         * @param  string   $tablePrefix
         * @param  array    $config
         * @return void
         */

        public Connection(object pdo, string database = "", string tablePrefix = "", ApplicationDatabaseConnectionConfig config = default(ApplicationDatabaseConnectionConfig))
        {
            this._pdo = pdo;
            // First we will setup the default properties. We keep track of the DB
            // name we are connected to since it is needed when some reflective
            // type commands are run such as checking whether a table exists.
            this._database = database;
            this._tablePrefix = tablePrefix;
            this._config = config;
            // We need to initialize a query grammar and the query post processors
            // which are both very important parts of the database abstractions
            // so we initialize these to their default values while starting.
            this.useDefaultQueryGrammar();
            this.useDefaultPostProcessor();
        }

        /**
         * Set the query grammar to the default implementation.
         *
         * @return void
         */
        public virtual void useDefaultQueryGrammar()
        {
            this._queryGrammar = this.getDefaultQueryGrammar();
        }
        /**
         * Get the default query grammar instance.
         *
         * @return \Illuminate\Database\Query\Grammars\Grammar
         */
        protected virtual BaseGrammar getDefaultQueryGrammar()
        {
            return new BaseGrammar();
        }
        /**
         * Set the schema grammar to the default implementation.
         *
         * @return void
         */
        public virtual void useDefaultSchemaGrammar()
        {
            this._schemaGrammar = this.getDefaultSchemaGrammar();
        }
        /**
         * Get the default schema grammar instance.
         *
         * @return \Illuminate\Database\Schema\Grammars\Grammar
         */
        protected virtual SchemaGrammar getDefaultSchemaGrammar()
        {
            return new SchemaGrammar();
        }
        /**
         * Set the query post processor to the default implementation.
         *
         * @return void
         */
        public virtual void useDefaultPostProcessor()
        {
            this._postProcessor = this.getDefaultPostProcessor();
        }
        /**
         * Get the default post processor instance.
         *
         * @return \Illuminate\Database\Query\Processors\Processor
         */
        protected virtual BaseProcessor getDefaultPostProcessor()
        {
            return new BaseProcessor();
        }
        /**
         * Get a schema builder instance for the connection.
         *
         * @return \Illuminate\Database\Schema\Builder
         */
        public virtual SchemaBuilder getSchemaBuilder()
        {
            if (this._schemaGrammar == null) {
                this.useDefaultSchemaGrammar();
            }
            return new SchemaBuilder(this);
        }
        /**
         * Begin a fluent query against a database table.
         *
         * @param  string  $table
         * @return \Illuminate\Database\Query\Builder
         */
        public virtual Builder table(string table)
        {
            return this.query().from(table);
        }
        /**
         * Get a new query builder instance.
         *
         * @return \Illuminate\Database\Query\Builder
         */
        public virtual Builder query()
        {
            return new Builder(
                this, this.getQueryGrammar(), this.getPostProcessor()
            );
        }
        /**
         * Get a new raw query expression.
         *
         * @param  mixed  $value
         * @return \Illuminate\Database\Query\Expression
         */
        public virtual Expression raw(object value)
        {
            return new Expression(value);
        }
        /**
         * Run a select statement and return a single result.
         *
         * @param  string  $query
         * @param  array   $bindings
         * @return mixed
         */
        public virtual Dictionary<string, object> selectOne(string query, object[] bindings = null)
        {
            bindings = bindings ?? new object[0];

            Dictionary<string, object>[] records = this.select(query, bindings);
            return records.Length > 0 ? records[0] : null;
        }
        /**
         * Run a select statement against the database.
         *
         * @param  string  $query
         * @param  array   $bindings
         * @return array
         */
        public virtual Dictionary<string, object>[] selectFromWriteConnection(string query, object[] bindings = null)
        {
            bindings = bindings ?? new object[0];
            return this.select(query, bindings, false);
        }
        /**
         * Run a select statement against the database.
         *
         * @param  string  $query
         * @param  array  $bindings
         * @param  bool  $useReadPdo
         * @return array
         */
        public virtual Dictionary<string, object>[] select(string query, object[] bindings = null, bool useReadPdo = true)
        {
            bindings = bindings ?? new object[0];
            return this.selectingStatement(query, bindings, useReadPdo);
        }
        /**
         * Get the PDO connection to use for a select query.
         *
         * @param  bool  $useReadPdo
         * @return \PDO
         */
        protected virtual object getPdoForSelect(bool useReadPdo = true)
        {
            return useReadPdo ? this.getReadPdo() : this.getPdo();
        }
        /**
         * Run an insert statement against the database.
         *
         * @param  string  $query
         * @param  array   $bindings
         * @return bool
         */
        public virtual bool insert(string query, object[] bindings = null)
        {
            bindings = bindings ?? new object[0];
            return this.statement(query, bindings);
        }
        /**
         * Run an update statement against the database.
         *
         * @param  string  $query
         * @param  array   $bindings
         * @return int
         */
        public virtual int update(string query, object[] bindings = null)
        {
            bindings = bindings ?? new object[0];
            return this.affectingStatement(query, bindings);
        }
        /**
         * Run a delete statement against the database.
         *
         * @param  string  $query
         * @param  array   $bindings
         * @return int
         */
        public virtual int delete(string query, object[] bindings = null)
        {
            bindings = bindings ?? new object[0];
            return this.affectingStatement(query, bindings);
        }
        /**
         * Execute an SQL statement and return the boolean result.
         *
         * @param  string  $query
         * @param  array   $bindings
         * @return bool
         */
        public virtual bool statement(string query, object[] bindings = null)
        {
            bindings = bindings ?? new object[0];
			throw new NotImplementedException ("statement(query, bindings) method is not implemented by the CustomConnection class");
        }
        /**
         * Execute an SQL statement and return the results.
         *
         * @param  string  $query
         * @param  array   $bindings
         * @return array
         */
        public virtual Dictionary<string, object>[] selectingStatement(string query, object[] bindings = null, bool useReadPdo = true)
        {
            bindings = bindings ?? new object[0];
            throw new NotImplementedException("selectingStatement(query, bindings) method is not implemented by the CustomConnection class");
        }
        /**
         * Run an SQL statement and get the number of rows affected.
         *
         * @param  string  $query
         * @param  array   $bindings
         * @return int
         */
        public virtual int affectingStatement(string query, object[] bindings = null)
        {
            bindings = bindings ?? new object[0];
            throw new NotImplementedException ("affectingStatement(query, bindings) method is not implemented by the CustomConnection class");
        }
        /**
         * Run a raw, unprepared query against the PDO connection.
         *
         * @param  string  $query
         * @return bool
         */
        public virtual bool unprepared(string query)
        {
            throw new NotImplementedException ("unprepared(query) method is not implemented by the CustomConnection class");
        }
        /**
         * Prepare the query bindings for execution.
         *
         * @param  array  $bindings
         * @return array
         */
        public virtual string[] prepareBindings(object[] bindings)
        {
            Grammar grammar = this.getQueryGrammar();
            string[] newBindings = new string[bindings.Length];

            for (var i = 0; i < bindings.Length; i++) {
                // We need to transform all instances of DateTimeInterface into the actual
                // date string. Each query grammar maintains its own date string format
                // so we'll just ask the grammar for the format to get from the date.
                if (bindings[i] is DateTime) {
                    newBindings[i] = String.Format(String.Concat("{0:", grammar.getDateFormat(), "}"), bindings[i]);
                } else if (bindings[i] is bool) {
                    newBindings[i] = ((bool)bindings[i] == false ? 0 : 1).ToString();
                }
            }
            return newBindings;
        }

        /**
         * Execute a Closure within a transaction.
         *
         * @param  \Closure  $callback
         * @return mixed
         *
         * @throws \Throwable
         */
        public virtual T transaction<T>(Func<Connection, T> callback)
        {
            T result = default(T);

            this.beginTransaction();
            // We'll simply execute the given callback within a try / catch block
            // and if we catch any exception we can rollback the transaction
            // so that none of the changes are persisted to the database.
            try {
                result = callback(this);
                this.commit();
            }
            // If we catch an exception, we will roll back so nothing gets messed
            // up in the database. Then we'll re-throw the exception so it can
            // be handled how the developer sees fit for their applications.
            catch (Exception e) {
                this.rollBack();
                throw e;
            }

            return result;
        }

        /**
         * Start a new database transaction.
         *
         * @return void
         */
        public virtual void beginTransaction()
        {
            throw new Exception("beginTransaction() method is not implemented by the CustomConnection class");
        }
        /**
         * Commit the active database transaction.
         *
         * @return void
         */
        public virtual void commit()
        {
            throw new Exception("commit() method is not implemented by the CustomConnection class");
        }
        /**
         * Rollback the active database transaction.
         *
         * @return void
         */
        public virtual void rollBack()
        {
            throw new Exception("rollBack() method is not implemented by the CustomConnection class");
        }
        /**
         * Get the number of active transactions.
         *
         * @return int
         */
        public virtual int transactionLevel()
        {
            throw new Exception("transactionLevel() method is not implemented by the CustomConnection class");
        }


        protected virtual bool causedByLostConnection(QueryException e)
        {
            return false;
        }


        public virtual QueryLogItem[] pretend(Func<Connection, object> callback)
        {
            bool loggingQueries = this._loggingQueries;
            this.enableQueryLog();
            this._pretending = true;
            this._queryLog = new QueryLogItem[0];
            // Basically to make the database connection "pretend", we will just return
            // the default values for all the query methods, then we will return an
            // array of queries that were "executed" within the Closure callback.
            callback(this);
            this._pretending = false;
            this._loggingQueries = loggingQueries;
            return this._queryLog;
        }

        /**
         * Run a SQL statement and log its execution context.
         *
         * @param  string    $query
         * @param  array     $bindings
         * @param  \Closure  $callback
         * @return mixed
         *
         * @throws \Illuminate\Database\QueryException
         */
        protected virtual T run<T>(string query, object[] bindings, Func<Connection, string, object[], T> callback)
        {
            T result = default(T);
            this.reconnectIfMissingConnection();
            DateTime start = DateTime.Now;
            //$start = microtime(true);
            // TODO: Track if missing connection
            // Here we will run this query. If an exception occurs we'll determine if it was
            // caused by a connection that has been lost. If that is the cause, we'll try
            // to re-establish connection and re-run the query with a fresh connection.
            try {
                result = this.runQueryCallback(query, bindings, callback);
            } catch (QueryException e) {
                result = this.tryAgainIfCausedByLostConnection(
                    e, query, bindings, callback
                );
            }
            // Once we have run the query we will calculate the time that it took to run and
            // then log the query, bindings, and execution time so we will report them on
            // the event that the developer needs them. We'll log time in milliseconds.
            //$time = $this->getElapsedTime($start);
            TimeSpan span = DateTime.Now - start;
            float? time = span.Milliseconds;
            this.logQuery(query, bindings, time);
            return result;
        }
        /**
         * Run a SQL statement.
         *
         * @param  string    $query
         * @param  array     $bindings
         * @param  \Closure  $callback
         * @return mixed
         *
         * @throws \Illuminate\Database\QueryException
         */
        protected virtual T runQueryCallback<T>(string query, object[] bindings, Func<Connection, string, object[], T> callback)
        {
            T result = default(T);
            // To execute the statement, we'll simply call the callback, which will actually
            // run the SQL against the PDO connection. Then we can calculate the time it
            // took to execute and log the query SQL, bindings and time in our memory.
            try {
                result = callback(this, query, bindings);
            }
            // If an exception occurs when attempting to run a query, we'll format the error
            // message to include the bindings with SQL, which will make this exception a
            // lot more helpful to the developer instead of just the database's errors.
            catch (QueryException e) {
                throw new QueryException(
                    query, this.prepareBindings(bindings), e
                );
            }
            return result;
        }
        /**
         * Handle a query exception that occurred during query execution.
         *
         * @param  \Illuminate\Database\QueryException  $e
         * @param  string    $query
         * @param  array     $bindings
         * @param  \Closure  $callback
         * @return mixed
         *
         * @throws \Illuminate\Database\QueryException
         */
        protected virtual T tryAgainIfCausedByLostConnection<T>(QueryException e, string query, object[] bindings, Func<Connection, string, object[], T> callback)
        {
            if (this.causedByLostConnection(e.getPrevious())) {
                this.reconnect();
                return this.runQueryCallback(query, bindings, callback);
            }
            throw e;
        }
        /**
         * Disconnect from the underlying PDO connection.
         *
         * @return void
         */
        public virtual void disconnect()
        {
            this.setPdo(null).setReadPdo(null);
        }
        /**
         * Reconnect to the database.
         *
         * @return void
         *
         * @throws \LogicException
         */
        public virtual void reconnect()
        {
            if (this._reconnector != null)
            {
                this._reconnector(this);
            }
            else
            {
                throw new Exception("Lost connection and no reconnector available.");
            }
        }
        /**
         * Reconnect to the database if a PDO connection is missing.
         *
         * @return void
         */
        protected virtual void reconnectIfMissingConnection()
        {
            if (this.getPdo() == null || this.getReadPdo() == null) {
                this.reconnect();
            }
        }
        /**
         * Log a query in the connection's query log.
         *
         * @param  string  $query
         * @param  array   $bindings
         * @param  float|null  $time
         * @return void
         */
        public virtual void logQuery(string query, object[] bindings, float? time = null)
        {
        /* TODO
            if (isset($this->events)) {
                $this->events->fire(new Events\QueryExecuted(
                    $query, $bindings, $time, $this
                ));
            } */
            if (this._loggingQueries) {
                this._queryLog = ArrayUtil.push(this._queryLog, new QueryLogItem(){ query = query, bindings = bindings, time = time });
            }
        }
        /**
         * Register a database query listener with the connection.
         *
         * @param  \Closure  $callback
         * @return void
         *//* TODO
        public function listen(Closure $callback)
        {
            if (isset($this->events)) {
                $this->events->listen(Events\QueryExecuted::class, $callback);
            }
        }*/
        /**
         * Fire an event for this connection.
         *
         * @param  string  $event
         * @return void
         *//* TODO
        protected function fireConnectionEvent($event)
        {
            if (! isset($this->events)) {
                return;
            }
            switch ($event) {
                case 'beganTransaction':
                    return $this->events->fire(new Events\TransactionBeginning($this));
                case 'committed':
                    return $this->events->fire(new Events\TransactionCommitted($this));
                case 'rollingBack':
                    return $this->events->fire(new Events\TransactionRolledBack($this));
            }
        }*/
        /**
         * Get the elapsed time since a given starting point.
         *
         * @param  int    $start
         * @return float
         */
        protected virtual float getElapsedTime(int start)
        {
            //return round((microtime(true) - $start) * 1000, 2);
            // TODO:
            return 0;
        }
        /**
         * Get the current PDO connection.
         *
         * @return \PDO
         */
        public virtual object getPdo()
        {
            if (this._pdo is Func<object>)
            {
                this._pdo = ((Func<object>)this._pdo)();
            }

            return this._pdo;
        }
        /**
         * Get the current PDO connection used for reading.
         *
         * @return \PDO
         */
        public virtual object getReadPdo()
        {
            if (this._transactions >= 1) {
                return this.getPdo();
            }
            return this._readPdo == null ? this.getPdo() : this._readPdo;
        }
        /**
         * Set the PDO connection.
         *
         * @param  \PDO|null  $pdo
         * @return $this
         */
        public virtual Connection setPdo(object pdo)
        {
            if (this._transactions >= 1) {
                throw new Exception("Can't swap PDO instance while within transaction.");
            }
            this._pdo = pdo;
            return this;
        }
        /**
         * Set the PDO connection used for reading.
         *
         * @param  \PDO|null  $pdo
         * @return $this
         */
        public virtual Connection setReadPdo(object pdo)
        {
            this._readPdo = pdo;
            return this;
        }
        /**
         * Set the reconnect instance on the connection.
         *
         * @param  callable  $reconnector
         * @return $this
         */
        public virtual Connection setReconnector(Action<Connection> reconnector)
        {
            this._reconnector = reconnector;
            return this;
        }
        /**
         * Get the database connection name.
         *
         * @return string|null
         */
        public virtual string getName()
        {
            return this._config.name == null ? "" : this._config.name;
        }
        /**
         * Get an option from the configuration options.
         *
         * @param  string  $option
         * @return mixed
         */
        public virtual object getConfig()
        {
            return this._config;
        }
        /**
         * Get the PDO driver name.
         *
         * @return string
         */
        public virtual string getDriverName()
        {
            /*return $this->pdo->getAttribute(PDO::ATTR_DRIVER_NAME); */
            return "";
        }
        /**
         * Get the query grammar used by the connection.
         *
         * @return \Illuminate\Database\Query\Grammars\Grammar
         */
        public virtual BaseGrammar getQueryGrammar()
        {
            return this._queryGrammar;
        }
        /**
         * Set the query grammar used by the connection.
         *
         * @param  \Illuminate\Database\Query\Grammars\Grammar  $grammar
         * @return void
         */
        public virtual void setQueryGrammar(BaseGrammar grammar)
        {
            this._queryGrammar = grammar;
        }
        /**
         * Get the schema grammar used by the connection.
         *
         * @return \Illuminate\Database\Schema\Grammars\Grammar
         */
        public virtual SchemaGrammar getSchemaGrammar()
        {
            return this._schemaGrammar;
        }
        /**
         * Set the schema grammar used by the connection.
         *
         * @param  \Illuminate\Database\Schema\Grammars\Grammar  $grammar
         * @return void
         */
        public virtual void setSchemaGrammar(SchemaGrammar grammar)
        {
            this._schemaGrammar = grammar;
        }
        /**
         * Get the query post processor used by the connection.
         *
         * @return \Illuminate\Database\Query\Processors\Processor
         */
        public virtual BaseProcessor getPostProcessor()
        {
            return this._postProcessor;
        }
        /**
         * Set the query post processor used by the connection.
         *
         * @param  \Illuminate\Database\Query\Processors\Processor  $processor
         * @return void
         */
        public virtual void setPostProcessor(BaseProcessor processor)
        {
            this._postProcessor = processor;
        }
        /**
         * Get the event dispatcher used by the connection.
         *
         * @return \Illuminate\Contracts\Events\Dispatcher
         *//* TODO
        public Dispatcher getEventDispatcher()
        {
            return this._events;
        }
        /**
         * Set the event dispatcher instance on the connection.
         *
         * @param  \Illuminate\Contracts\Events\Dispatcher  $events
         * @return void
         *//* TODO
        public void setEventDispatcher(Dispatcher events)
        {
            this._events = events;
        }
        /**
         * Determine if the connection in a "dry run".
         *
         * @return bool
         */
        public virtual bool pretending()
        {
            return this._pretending == true;
        }
        /**
         * Get the default fetch mode for the connection.
         *
         * @return int
         */
        public virtual int getFetchMode()
        {
            return this._fetchMode;
        }
        /**
         * Set the default fetch mode for the connection.
         *
         * @param  int  $fetchMode
         * @return int
         */
        public virtual void setFetchMode(int fetchMode)
        {
            this._fetchMode = fetchMode;
        }
        /**
         * Get the connection query log.
         *
         * @return array
         */
        public virtual QueryLogItem[] getQueryLog()
        {
            return this._queryLog;
        }
        /**
         * Clear the query log.
         *
         * @return void
         */
        public virtual void flushQueryLog()
        {
            this._queryLog = new QueryLogItem[0];
        }
        /**
         * Enable the query log on the connection.
         *
         * @return void
         */
        public virtual void enableQueryLog()
        {
            this._loggingQueries = true;
        }
        /**
         * Disable the query log on the connection.
         *
         * @return void
         */
        public virtual void disableQueryLog()
        {
            this._loggingQueries = false;
        }
        /**
         * Determine whether we're logging queries.
         *
         * @return bool
         */
        public virtual bool logging()
        {
            return this._loggingQueries;
        }
        /**
         * Get the name of the connected database.
         *
         * @return string
         */
        public virtual string getDatabaseName()
        {
            return this._database;
        }
        /**
         * Set the name of the connected database.
         *
         * @param  string  $database
         * @return this
         */
        public virtual Connection setDatabaseName(string database)
        {
            this._database = database;
            return this;
        }
        /**
         * Get the table prefix for the connection.
         *
         * @return string
         */
        public virtual string getTablePrefix()
        {
            return this._tablePrefix;
        }
        /**
         * Set the table prefix in use by the connection.
         *
         * @param  string  $prefix
         * @return void
         */
        public virtual void setTablePrefix(string prefix)
        {
            this._tablePrefix = prefix;
            this.getQueryGrammar().setTablePrefix(prefix);
        }
        /**
         * Set the table prefix and return the grammar.
         *
         * @param  \Illuminate\Database\Grammar  $grammar
         * @return \Illuminate\Database\Grammar
         */
        public virtual BaseGrammar withTablePrefix(BaseGrammar grammar)
        {
            grammar.setTablePrefix(this._tablePrefix);
            return grammar;
        }
    }
}

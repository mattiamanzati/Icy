using System;
using System.Collections.Generic;
using System.Text;
using Icy.Database.Query;

namespace Icy.Database
{
    // 03221c9  on 17 Jul
    public interface ConnectionInterface
    {
        /**
         * Begin a fluent query against a database table.
         *
         * @param  string  $table
         * @return \Illuminate\Database\Query\Builder
         */
        Builder table(string table);
        /**
         * Get a new raw query expression.
         *
         * @param  mixed  $value
         * @return \Illuminate\Database\Query\Expression
         */
        Expression raw(object value);
        /**
         * Run a select statement and return a single result.
         *
         * @param  string  $query
         * @param  array   $bindings
         * @return mixed
         */
        Dictionary<string, object> selectOne(string query, object[] bindings);
        /**
         * Run a select statement against the database.
         *
         * @param  string  $query
         * @param  array   $bindings
         * @return array
         */
        Dictionary<string, object>[] select(string query, object[] bindings, bool useWritePdo = false);
        /**
         * Run an insert statement against the database.
         *
         * @param  string  $query
         * @param  array   $bindings
         * @return bool
         */
        bool insert(string query, object[] bindings);
        /**
         * Run an update statement against the database.
         *
         * @param  string  $query
         * @param  array   $bindings
         * @return int
         */
        int update(string query, object[] bindings);
        /**
         * Run a delete statement against the database.
         *
         * @param  string  $query
         * @param  array   $bindings
         * @return int
         */
        int delete(string query, object[] bindings);
        /**
         * Execute an SQL statement and return the boolean result.
         *
         * @param  string  $query
         * @param  array   $bindings
         * @return bool
         */
        bool statement(string query, object[] bindings);
        /**
         * Run an SQL statement and get the number of rows affected.
         *
         * @param  string  $query
         * @param  array   $bindings
         * @return int
         */
        int affectingStatement(string query, object[] bindings);
        /**
         * Run a raw, unprepared query against the PDO connection.
         *
         * @param  string  $query
         * @return bool
         */
        bool unprepared(string query);
        /**
         * Prepare the query bindings for execution.
         *
         * @param  array  $bindings
         * @return array
         */
        string[] prepareBindings(object[] bindings);
        /**
         * Execute a Closure within a transaction.
         *
         * @param  \Closure  $callback
         * @return mixed
         *
         * @throws \Throwable
         */
        T transaction<T>(Func<Connection, T> callback);
        /**
         * Start a new database transaction.
         *
         * @return void
         */
        void beginTransaction();
        /**
         * Commit the active database transaction.
         *
         * @return void
         */
        void commit();
        /**
         * Rollback the active database transaction.
         *
         * @return void
         */
        void rollBack();
        /**
         * Get the number of active transactions.
         *
         * @return int
         */
        int transactionLevel();
        /**
         * Execute the given callback in "dry run" mode.
         *
         * @param  \Closure  $callback
         * @return array
         */
        QueryLogItem[] pretend(Func<Connection, object> callback);
    }
}

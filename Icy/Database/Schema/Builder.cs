using System;
using System.Collections.Generic;
using System.Text;
using Icy.Util;
using SchemaGrammar = Icy.Database.Schema.Grammars.Grammar;

namespace Icy.Database.Schema
{
    public class Builder
    {
            /**
     * The database connection instance.
     *
     * @var \Illuminate\Database\Connection
     */
    protected Connection _connection;
    /**
     * The schema grammar instance.
     *
     * @var \Illuminate\Database\Schema\Grammars\Grammar
     */
    protected SchemaGrammar _grammar;
    /**
     * The Blueprint resolver callback.
     *
     * @var \Closure
     */
    protected Func<string, Action<Blueprint>, Blueprint> _resolver;
    /**
     * Create a new database Schema manager.
     *
     * @param  \Illuminate\Database\Connection  $connection
     * @return void
     */
    public Builder(Connection connection)
    {
        this._connection = connection;
        this._grammar = connection.getSchemaGrammar();
    }
    /**
     * Determine if the given table exists.
     *
     * @param  string  $table
     * @return bool
     */
    public virtual bool hasTable(string table)
    {
        string sql = this._grammar.compileTableExists();
        table = this._connection.getTablePrefix() + table;
        return this._connection.select(sql, new object[]{ table }).Length > 0;
    }
    /**
     * Determine if the given table has a given column.
     *
     * @param  string  $table
     * @param  string  $column
     * @return bool
     */
    public virtual bool hasColumn(string table, string column)
    {
        column = column.ToLower();
        return ArrayUtil.indexOf(ArrayUtil.map(this.getColumnListing(table), col => col.ToLower()), column) > -1;
    }
    /**
     * Determine if the given table has given columns.
     *
     * @param  string  $table
     * @param  array   $columns
     * @return bool
     */
    public virtual bool hasColumns(string table, string[] columns )
    {
        string[] tableColumns = ArrayUtil.map(this.getColumnListing(table), col => col.ToLower());
        foreach(string column in columns){
            if(ArrayUtil.indexOf(columns, column) == -1){
                return false;
            }
        }
        return true;
    }
    /**
     * Get the data type for the given column name.
     *
     * @param  string  $table
     * @param  string  $column
     * @return string
     */
    public virtual string getColumnType(string table, string column)
    {
        string sql = this._grammar.compileColumnType();
        table = this._connection.getTablePrefix() + table;
        var rows = this._connection.select(sql, new object[] { table, column });
        if (rows.Length == 0) return null;
        return rows[0]["data_type"].ToString();
    }
    /**
     * Get the column listing for a given table.
     *
     * @param  string  $table
     * @return array
     */
    public virtual string[] getColumnListing(string table)
    {
        table = this._connection.getTablePrefix() + table;
        Dictionary<string, object>[] results = this._connection.select(this._grammar.compileColumnExists(), new object[]{ table });
        return this._connection.getPostProcessor().processColumnListing(results);
    }
    /**
     * Modify a table on the schema.
     *
     * @param  string    $table
     * @param  \Closure  $callback
     * @return \Illuminate\Database\Schema\Blueprint
     */
    public virtual void table(string table, Action<Blueprint> callback)
    {
        this.build(this.createBlueprint(table, callback));
    }
    /**
     * Create a new table on the schema.
     *
     * @param  string    $table
     * @param  \Closure  $callback
     * @return \Illuminate\Database\Schema\Blueprint
     */
    public virtual void create(string table, Action<Blueprint> callback)
    {
        Blueprint blueprint = this.createBlueprint(table);
        blueprint.create();
        callback(blueprint);
        this.build(blueprint);
    }
    /**
     * Drop a table from the schema.
     *
     * @param  string  $table
     * @return \Illuminate\Database\Schema\Blueprint
     */
    public virtual void drop(string table)
    {
        Blueprint blueprint = this.createBlueprint(table);
        blueprint.drop();
        this.build(blueprint);
    }
    /**
     * Drop a table from the schema if it exists.
     *
     * @param  string  $table
     * @return \Illuminate\Database\Schema\Blueprint
     */
    public virtual void dropIfExists(string table)
    {
        Blueprint blueprint = this.createBlueprint(table);
        blueprint.dropIfExists();
        this.build(blueprint);
    }
    /**
     * Rename a table on the schema.
     *
     * @param  string  $from
     * @param  string  $to
     * @return \Illuminate\Database\Schema\Blueprint
     */
    public virtual void rename(string from, string to)
    {
        Blueprint blueprint = this.createBlueprint(from);
        blueprint.rename(to);
        this.build(blueprint);
    }
    /**
     * Execute the blueprint to build / modify the table.
     *
     * @param  \Illuminate\Database\Schema\Blueprint  $blueprint
     * @return void
     */
    protected virtual void build(Blueprint blueprint)
    {
        blueprint.build(this._connection, this._grammar);
    }
    /**
     * Create a new command set with a Closure.
     *
     * @param  string  $table
     * @param  \Closure|null  $callback
     * @return \Illuminate\Database\Schema\Blueprint
     */
    protected virtual Blueprint createBlueprint(string table, Action<Blueprint> callback = null)
    {
        if (this._resolver != null) {
            return this._resolver(table, callback);
        }
        return new Blueprint(table, callback);
    }
    /**
     * Get the database connection instance.
     *
     * @return \Illuminate\Database\Connection
     */
    public Connection getConnection()
    {
        return this._connection;
    }
    /**
     * Set the database connection instance.
     *
     * @param  \Illuminate\Database\Connection  $connection
     * @return $this
     */
    public virtual Builder setConnection(Connection connection)
    {
        this._connection = connection;
        return this;
    }
    /**
     * Set the Schema Blueprint resolver callback.
     *
     * @param  \Closure  $resolver
     * @return void
     */
    public virtual void blueprintResolver(Func<string, Action<Blueprint>, Blueprint> resolver)
    {
        this._resolver = resolver;
    }

    }
}

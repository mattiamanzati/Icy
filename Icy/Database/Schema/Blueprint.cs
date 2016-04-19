using Icy.Util;
using System;
using System.Collections.Generic;
using System.Text;
using SchemaGrammar = Icy.Database.Schema.Grammars.Grammar;

namespace Icy.Database.Schema
{

    public class BlueprintCommand
    {
        public string name;
        public string[] columns;
        public string from;
        public string to;
        public string index;
        public string column;
        public string algorithm;
    }

    public class BlueprintColumn
    {
        public bool _unsigned;
        public string _name;
        public string _type;
        public string _primary;
        public string _unique;
        public string _index;
        public bool _change;
        public bool _autoIncrement;
        public int? _total;
        public int? _places;
        public object[] _allowed;
        public int _length;
        public bool _nullable;

        public BlueprintColumn nullable()
        {
            this._nullable = true;
            return this;
        }
    }
    public class Blueprint
    {
        /**
     * The table the blueprint describes.
     *
     * @var string
     */
        protected string _table;
        /**
         * The columns that should be added to the table.
         *
         * @var array
         */
        protected BlueprintColumn[] _columns = new BlueprintColumn[0];
        /**
         * The commands that should be run for the table.
         *
         * @var array
         */
        protected BlueprintCommand[] _commands = new BlueprintCommand[0];
        /**
         * The storage engine that should be used for the table.
         *
         * @var string
         */
        public string _engine;
        /**
         * The default character set that should be used for the table.
         */
        public string _charset;
        /**
         * The collation that should be used for the table.
         */
        public string _collation;
        /**
         * Whether to make the table temporary.
         *
         * @var bool
         */
        public bool _temporary = false;

        public Blueprint(string table, Action<Blueprint> callback = null)
        {
            this._table = table;

            if (callback != null)
            {
                callback(this);
            }
        }


        /**
         * Execute the blueprint against the database.
         *
         * @param  \Illuminate\Database\Connection  $connection
         * @param  \Illuminate\Database\Schema\Grammars\Grammar $grammar
         * @return void
         */
        public virtual void build(Connection connection, SchemaGrammar grammar)
        {
            foreach (var statement in this.toSql(connection, grammar))
            {
                connection.statement(statement);
            }
        }

        /**
         * Get the raw SQL statements for the blueprint.
         *
         * @param  \Illuminate\Database\Connection  $connection
         * @param  \Illuminate\Database\Schema\Grammars\Grammar  $grammar
         * @return array
         */
        public virtual string[] toSql(Connection connection, SchemaGrammar grammar)
        {
            this.addImpliedCommands();
            var statements = new string[0];
            // Each type of command has a corresponding compiler BlueprintColumn on the schema
            // grammar which is used to build the necessary SQL statements to build
            // the blueprint element, so we'll just call that compilers function.
            foreach (var command in this._commands)
            {
                var method = "compile" + StrUtil.ucfirst(command.name);
                if (ReflectionUtil.existsMethod(this.GetType(), method))
                {
                    string[] sql = (string[])ReflectionUtil.callMethod(grammar, method, this, command, connection);
                    statements = ArrayUtil.push(statements, sql);
                }
            }
            return statements;
        }

        /**
         * Add the commands that are implied by the blueprint.
         *
         * @return void
         */
        protected virtual void addImpliedCommands()
        {
            if (this.getAddedColumns().Length > 0 && !this.creating())
            {
                this._commands = ArrayUtil.prepend(this._commands, this.createCommand("add"));
            }
            if (this.getChangedColumns().Length > 0 && !this.creating())
            {
                this._commands = ArrayUtil.prepend(this._commands, this.createCommand("change"));
            }
            this.addFluentIndexes();
        }

        /**
         * Add the index commands fluently specified on columns.
         *
         * @return void
         */
        protected virtual void addFluentIndexes()
        {
            foreach (var column in this._columns)
            {
                // Handle primary
                if (column._primary == "true")
                {
                    this.primary(new string[] { column._name });
                    continue;
                }
                else if (column._primary != null)
                {
                    this.primary(new string[] { column._name }, column._primary);
                    continue;
                }
                // Handle unique
                if (column._unique == "true")
                {
                    this.unique(new string[] { column._name });
                    continue;
                }
                else if (column._unique != null)
                {
                    this.unique(new string[] { column._name }, column._unique);
                    continue;
                }
                // Handle index
                if (column._index == "true")
                {
                    this.index(new string[] { column._name });
                    continue;
                }
                else if (column._index != null)
                {
                    this.index(new string[] { column._name }, column._index);
                    continue;
                }
            }
        }

        /**
         * Determine if the blueprint has a create command.
         *
         * @return bool
         */
        protected virtual bool creating()
        {
            foreach (var command in this._commands)
            {
                if (command.name == "create")
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * Indicate that the table needs to be created.
         *
         * @return \Illuminate\Support\Fluent
         */
        public virtual BlueprintCommand create()
        {
            return this.addCommand("create");
        }
        /**
         * Indicate that the table needs to be temporary.
         *
         * @return void
         */
        public virtual void temporary()
        {
            this._temporary = true;
        }
        /**
         * Indicate that the table should be dropped.
         *
         * @return \Illuminate\Support\Fluent
         */
        public virtual BlueprintCommand drop()
        {
            return this.addCommand("drop");
        }
        /**
         * Indicate that the table should be dropped if it exists.
         *
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintCommand dropIfExists()
        {
            return this.addCommand("dropIfExists");
        }
        /**
         * Indicate that the given columns should be dropped.
         *
         * @param  array|mixed  $columns
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintCommand dropColumn(params string[] columns)
        {
            var cmd = this.addCommand("dropColumn");
            cmd.columns = columns;
            return cmd;
        }
        /**
         * Indicate that the given columns should be renamed.
         *
         * @param  string  $from
         * @param  string  $to
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintCommand renameColumn(string from, string to)
        {
            var cmd = this.addCommand("renameColumn");
            cmd.from = from;
            cmd.to = to;
            return cmd;
        }
        /**
         * Indicate that the given primary key should be dropped.
         *
         * @param  string|array  $index
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintCommand dropPrimary(string index = null)
        {
            return this.dropIndexCommand("dropPrimary", "primary", index);
        }
        /**
         * Indicate that the given unique key should be dropped.
         *
         * @param  string|array  $index
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintCommand dropUnique(string index)
        {
            return this.dropIndexCommand("dropUnique", "unique", index);
        }
        /**
         * Indicate that the given index should be dropped.
         *
         * @param  string|array  $index
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintCommand dropIndex(string index)
        {
            return this.dropIndexCommand("dropIndex", "index", index);
        }
        /**
         * Indicate that the given foreign key should be dropped.
         *
         * @param  string|array  $index
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintCommand dropForeign(string index)
        {
            return this.dropIndexCommand("dropForeign", "foreign", index);
        }
        /**
         * Indicate that the timestamp columns should be dropped.
         *
         * @return void
         */
        public void dropTimestamps()
        {
            this.dropColumn("created_at", "updated_at");
        }
        /**
         * Indicate that the timestamp columns should be dropped.
         *
         * @return void
         */
        public void dropTimestampsTz()
        {
            this.dropTimestamps();
        }
        /**
         * Indicate that the soft delete column should be dropped.
         *
         * @return void
         */
        public void dropSoftDeletes()
        {
            this.dropColumn("deleted_at");
        }
        /**
         * Indicate that the remember token column should be dropped.
         *
         * @return void
         */
        public void dropRememberToken()
        {
            this.dropColumn("remember_token");
        }
        /**
         * Rename the table to a given name.
         *
         * @param  string  $to
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintCommand rename(string to)
        {
            var cmd = this.addCommand("rename");
            cmd.to = to;
            return cmd;
        }

        /**
    * Specify the primary key(s) for the table.
    *
    * @param  string|array  $columns
    * @param  string  $name
    * @param  string|null  $algorithm
    * @return \Illuminate\Support\Fluent
    */
        public BlueprintCommand primary(string[] columns, string name = null, string algorithm = null)
        {
            return this.indexCommand("primary", columns, name, algorithm);
        }
        /**
         * Specify a unique index for the table.
         *
         * @param  string|array  $columns
         * @param  string  $name
         * @param  string|null  $algorithm
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintCommand unique(string[] columns, string name = null, string algorithm = null)
        {
            return this.indexCommand("unique", columns, name, algorithm);
        }
        /**
         * Specify an index for the table.
         *
         * @param  string|array  $columns
         * @param  string  $name
         * @param  string|null  $algorithm
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintCommand index(string[] columns, string name = null, string algorithm = null)
        {
            return this.indexCommand("index", columns, name, algorithm);
        }
        /**
         * Specify a foreign key for the table.
         *
         * @param  string|array  $columns
         * @param  string  $name
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintCommand foreign(string[] columns, string name = null)
        {
            return this.indexCommand("foreign", columns, name);
        }
        /**
         * Create a new auto-incrementing integer (4-byte) column on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn increments(string column)
        {
            return this.unsignedInteger(column, true);
        }
        /**
         * Create a new auto-incrementing small integer (2-byte) column on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn smallIncrements(string column)
        {
            return this.unsignedSmallInteger(column, true);
        }
        /**
         * Create a new auto-incrementing medium integer (3-byte) column on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn mediumIncrements(string column)
        {
            return this.unsignedMediumInteger(column, true);
        }
        /**
         * Create a new auto-incrementing big integer (8-byte) column on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn bigIncrements(string column)
        {
            return this.unsignedBigInteger(column, true);
        }
        /**
         * Create a new char column on the table.
         *
         * @param  string  $column
         * @param  int  $length
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn chars(string column, int length = 255)
        {
            var cmd = this.addColumn("char", column);
            cmd._length = length;
            return cmd;
        }
        /**
         * Create a new string column on the table.
         *
         * @param  string  $column
         * @param  int  $length
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn strings(string column, int length = 255)
        {
            var cmd = this.addColumn("string", column);
            cmd._length = length;
            return cmd;
        }
        /**
         * Create a new text column on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn text(string column)
        {
            return this.addColumn("text", column);
        }
        /**
         * Create a new medium text column on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn mediumText(string column)
        {
            return this.addColumn("mediumText", column);
        }
        /**
         * Create a new long text column on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn longText(string column)
        {
            return this.addColumn("longText", column);
        }
        /**
         * Create a new integer (4-byte) column on the table.
         *
         * @param  string  $column
         * @param  bool  $autoIncrement
         * @param  bool  $unsigned
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn integer(string column, bool autoIncrement = false, bool unsigned = false)
        {
            var cmd = this.addColumn("integer", column);
            cmd._autoIncrement = autoIncrement;
            cmd._unsigned = unsigned;
            return cmd;
        }
        /**
         * Create a new tiny integer (1-byte) column on the table.
         *
         * @param  string  $column
         * @param  bool  $autoIncrement
         * @param  bool  $unsigned
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn tinyInteger(string column, bool autoIncrement = false, bool unsigned = false)
        {
            var cmd = this.addColumn("tinyInteger", column);
            cmd._autoIncrement = autoIncrement;
            cmd._unsigned = unsigned;
            return cmd;
        }
        /**
         * Create a new small integer (2-byte) column on the table.
         *
         * @param  string  $column
         * @param  bool  $autoIncrement
         * @param  bool  $unsigned
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn smallInteger(string column, bool autoIncrement = false, bool unsigned = false)
        {
            var cmd = this.addColumn("smallInteger", column);
            cmd._autoIncrement = autoIncrement;
            cmd._unsigned = unsigned;
            return cmd;
        }
        /**
         * Create a new medium integer (3-byte) column on the table.
         *
         * @param  string  $column
         * @param  bool  $autoIncrement
         * @param  bool  $unsigned
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn mediumInteger(string column, bool autoIncrement = false, bool unsigned = false)
        {
            var cmd = this.addColumn("mediumInteger", column);
            cmd._autoIncrement = autoIncrement;
            cmd._unsigned = unsigned;
            return cmd;
        }
        /**
         * Create a new big integer (8-byte) column on the table.
         *
         * @param  string  $column
         * @param  bool  $autoIncrement
         * @param  bool  $unsigned
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn bigInteger(string column, bool autoIncrement = false, bool unsigned = false)
        {
            var cmd = this.addColumn("bigInteger", column);
            cmd._autoIncrement = autoIncrement;
            cmd._unsigned = unsigned;
            return cmd;
        }
        /**
         * Create a new unsigned tiny integer (1-byte) column on the table.
         *
         * @param  string  $column
         * @param  bool  $autoIncrement
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn unsignedTinyInteger(string column, bool autoIncrement = false)
        {
            return this.tinyInteger(column, autoIncrement, true);
        }

        /**
         * Create a new unsigned small integer (2-byte) column on the table.
         *
         * @param  string  $column
         * @param  bool  $autoIncrement
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn unsignedSmallInteger(string column, bool autoIncrement = false)
        {
            return this.smallInteger(column, autoIncrement, true);
        }
        /**
         * Create a new unsigned medium integer (3-byte) column on the table.
         *
         * @param  string  $column
         * @param  bool  $autoIncrement
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn unsignedMediumInteger(string column, bool autoIncrement = false)
        {
            return this.mediumInteger(column, autoIncrement, true);
        }
        /**
         * Create a new unsigned integer (4-byte) column on the table.
         *
         * @param  string  $column
         * @param  bool  $autoIncrement
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn unsignedInteger(string column, bool autoIncrement = false)
        {
            return this.integer(column, autoIncrement, true);
        }
        /**
         * Create a new unsigned big integer (8-byte) column on the table.
         *
         * @param  string  $column
         * @param  bool  $autoIncrement
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn unsignedBigInteger(string column, bool autoIncrement = false)
        {
            return this.bigInteger(column, autoIncrement, true);
        }
        /**
         * Create a new float column on the table.
         *
         * @param  string  $column
         * @param  int     $total
         * @param  int     $places
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn floats(string column, int total = 8, int places = 2)
        {
            var cmd = this.addColumn("float", column);
            cmd._total = total;
            cmd._places = places;
            return cmd;
        }
        /**
         * Create a new double column on the table.
         *
         * @param  string   $column
         * @param  int|null    $total
         * @param  int|null $places
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn doubles(string column, int? total = null, int? places = null)
        {
            var cmd = this.addColumn("double", column);
            cmd._total = total;
            cmd._places = places;
            return cmd;
        }
        /**
         * Create a new decimal column on the table.
         *
         * @param  string  $column
         * @param  int     $total
         * @param  int     $places
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn decimals(string column, int total = 8, int places = 2)
        {
            var cmd = this.addColumn("decimal", column);
            cmd._total = total;
            cmd._places = places;
            return cmd;
        }
        /**
         * Create a new boolean column on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn boolean(string column)
        {
            return this.addColumn("boolean", column);
        }
        /**
         * Create a new enum column on the table.
         *
         * @param  string  $column
         * @param  array   $allowed
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn enums(string column, object[] allowed)
        {
            var cmd = this.addColumn("enum", column);
            cmd._allowed = allowed;
            return cmd;
        }
        /**
         * Create a new json column on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn json(string column)
        {
            return this.addColumn("json", column);
        }
        /**
         * Create a new jsonb column on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn jsonb(string column)
        {
            return this.addColumn("jsonb", column);
        }
        /**
         * Create a new date column on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn date(string column)
        {
            return this.addColumn("date", column);
        }
        /**
         * Create a new date-time column on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn dateTime(string column)
        {
            return this.addColumn("dateTime", column);
        }
        /**
         * Create a new date-time column (with time zone) on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn dateTimeTz(string column)
        {
            return this.addColumn("dateTimeTz", column);
        }
        /**
         * Create a new time column on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn time(string column)
        {
            return this.addColumn("time", column);
        }
        /**
         * Create a new time column (with time zone) on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn timeTz(string column)
        {
            return this.addColumn("timeTz", column);
        }
        /**
         * Create a new timestamp column on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn timestamp(string column)
        {
            return this.addColumn("timestamp", column);
        }
        /**
         * Create a new timestamp (with time zone) column on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn timestampTz(string column)
        {
            return this.addColumn("timestampTz", column);
        }
        /**
         * Add nullable creation and update timestamps to the table.
         *
         * Alias for self::timestamps().
         *
         * @return void
         */
        public void nullableTimestamps()
        {
            this.timestamps();
        }
        /**
         * Add nullable creation and update timestamps to the table.
         *
         * @return void
         */
        public void timestamps()
        {
            this.timestamp("created_at").nullable();
            this.timestamp("updated_at").nullable();
        }
        /**
         * Add creation and update timestampTz columns to the table.
         *
         * @return void
         */
        public void timestampsTz()
        {
            this.timestampTz("created_at").nullable();
            this.timestampTz("updated_at").nullable();
        }
        /**
         * Add a "deleted at" timestamp for the table.
         *
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn softDeletes()
        {
            return this.timestamp("deleted_at").nullable();
        }
        /**
         * Create a new binary column on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn binary(string column)
        {
            return this.addColumn("binary", column);
        }
        /**
         * Create a new uuid column on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn uuid(string column)
        {
            return this.addColumn("uuid", column);
        }
        /**
         * Create a new IP address column on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn ipAddress(string column)
        {
            return this.addColumn("ipAddress", column);
        }
        /**
         * Create a new MAC address column on the table.
         *
         * @param  string  $column
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn macAddress(string column)
        {
            return this.addColumn("macAddress", column);
        }
        /**
         * Add the proper columns for a polymorphic table.
         *
         * @param  string  $name
         * @param  string|null  $indexName
         * @return void
         */
        public void morphs(string name, string indexName = null)
        {
            this.unsignedInteger(name + "_id");
            this.strings(name + "_type");
            this.index(new string[] { name + "_id", name + "_type" }, indexName);
        }
        /**
         * Adds the `remember_token` column to the table.
         *
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn rememberToken()
        {
            return this.strings("remember_token", 100).nullable();
        }
        /**
         * Create a new drop index command on the blueprint.
         *
         * @param  string  $command
         * @param  string  $type
         * @param  string|array  $index
         * @return \Illuminate\Support\Fluent
         */
        protected BlueprintCommand dropIndexCommand(string command, string type, string index)
        {
            var columns = new string[0];
            // If the given "index" is actually an array of columns, the developer means
            // to drop an index merely by specifying the columns involved without the
            // conventional name, so we will build the index name from the columns.
            /*
            if (is_array($index))
            {
                columns = index;
                index = this.createIndexName(type, columns);
            }*/
            return this.indexCommand(command, columns, index);
        }
        /**
         * Add a new index command to the blueprint.
         *
         * @param  string        $type
         * @param  string|array  $columns
         * @param  string        $index
         * @param  string|null   $algorithm
         * @return \Illuminate\Support\Fluent
         */
        protected BlueprintCommand indexCommand(string type, string[] columns, string index, string algorithm = null)
        {
            // If no name was specified for this index, we will create one using a basic
            // convention of the table name, followed by the columns, followed by an
            // index type, such as primary or index, which makes the index unique.
            if (index == null)
            {
                index = this.createIndexName(type, columns);
            }
            var cmd = this.addCommand(type);
            cmd.index = index;
            cmd.columns = columns;
            cmd.algorithm = algorithm;
            return cmd;
        }
        /**
         * Create a default index name for the table.
         *
         * @param  string  $type
         * @param  array   $columns
         * @return string
         */
        protected string createIndexName(string type, string[] columns)
        {
            return (this._table + "_" + string.Join("_", columns) + "_" + type).ToLower().Replace('-','_').Replace('.', '_');
        }
        /**
         * Add a new column to the blueprint.
         *
         * @param  string  $type
         * @param  string  $name
         * @param  array   $parameters
         * @return \Illuminate\Support\Fluent
         */
        public BlueprintColumn addColumn(string type, string name)
        {
            var column = new BlueprintColumn() { _type = type, _name = name };
            this._columns = ArrayUtil.push(this._columns, column);
            return column;
        }
        /**
         * Remove a column from the schema blueprint.
         *
         * @param  string  $name
         * @return $this
         */
        public Blueprint removeColumn(string name)
        {
            this._columns = ArrayUtil.filter(this._columns, col => col._name != name);
            return this;
        }


        /**
         * Add a new command to the blueprint.
         *
         * @param  string  $name
         * @param  array  $parameters
         * @return \Illuminate\Support\Fluent
         */
        protected virtual BlueprintCommand addCommand(string name)
        {
            var command = this.createCommand(name);
            this._commands = ArrayUtil.push(this._commands, command);
            return command;
        }
        /**
         * Create a new Fluent command.
         *
         * @param  string  $name
         * @param  array   $parameters
         * @return \Illuminate\Support\Fluent
         */
        protected virtual BlueprintCommand createCommand(string name)
        {
            var cmd = new BlueprintCommand() { name = name };
            return cmd;
        }

        /**
         * Get the table the blueprint describes.
         *
         * @return string
         */
        public string getTable()
        {
            return this._table;
        }
        /**
         * Get the columns on the blueprint.
         *
         * @return array
         */
        public virtual BlueprintColumn[] getColumns()
        {
            return this._columns;
        }
        /**
         * Get the commands on the blueprint.
         *
         * @return array
         */
        public virtual BlueprintCommand[] getCommands()
        {
            return this._commands;
        }
        /**
         * Get the columns on the blueprint that should be added.
         *
         * @return array
         */
        public virtual BlueprintColumn[] getAddedColumns()
        {
            return ArrayUtil.filter(this._columns, (column) => !column._change);
        }
        /**
         * Get the columns on the blueprint that should be changed.
         *
         * @return array
         */
        public virtual BlueprintColumn[] getChangedColumns()
        {
            return ArrayUtil.filter(this._columns, (column) => column._change);
        }

    }
}

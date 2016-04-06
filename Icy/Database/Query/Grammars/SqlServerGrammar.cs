using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Icy.Util;

namespace Icy.Database.Query.Grammars
{
    // 9372e92 Dec 29, 2015
 public class SqlServerGrammar: Grammar
{
    /**
     * All of the available clause operators.
     *
     * @var array
     */
    protected string[] operators = new string[]{
        "=", "<", ">", "<=", ">=", "!<", "!>", "<>", "!=",
        "like", "not like", "between", "ilike",
        "&", "&=", "|", "|=", "^", "^=",
    };

    /**
     * Compile a select query into SQL.
     *
     * @param  \Illuminate\Database\Query\Builder  $query
     * @return string
     */
    public override string compileSelect(Builder query)
    {
        object[] original = query._columns;

        if (query._columns == null || query._columns.Length == 0) {
            query._columns = new object[]{"*"};
        }

        Dictionary<string, string> components = this.compileComponents(query);

        // If an offset is present on the query, we will need to wrap the query in
        // a big "ANSI" offset syntax block. This is very nasty compared to the
        // other database systems but is necessary for implementing features.
        if (query._offset > 0) {
            return this.compileAnsiOffset(query, components);
        }

        string sql = this.concatenate(components);

        query._columns = original;

        return sql;
    }

    /**
     * Compile the "select *" portion of the query.
     *
     * @param  \Illuminate\Database\Query\Builder  $query
     * @param  array  $columns
     * @return string|null
     */
    protected override string compileColumns(Builder query, object[] columns)
    {
        if (query._aggregate != null) {
            return "";
        }

        string select = query._distinct ? "select distinct " : "select ";

        // If there is a limit on the query, but not an offset, we will add the top
        // clause to the query, which serves as a "limit" type clause within the
        // SQL Server system similar to the limit keywords available in MySQL.
        if (query._limit > 0 && query._offset <= 0) {
            select += "top " + query._limit + " ";
        }

        return select + this.columnize(columns);
    }

    /**
     * Compile the "from" portion of the query.
     *
     * @param  \Illuminate\Database\Query\Builder  $query
     * @param  string  $table
     * @return string
     */
    protected override string compileFrom(Builder query, object table)
    {
        string from = base.compileFrom(query, table);

        if (query._lock != null) {
            return from + " " + query._lock;
        }

            /* TODO
        if (! is_null($query->lock)) {
            return $from.' with(rowlock,'.($query->lock ? 'updlock,' : '').'holdlock)';
        }*/

        return from;
    }

    /**
     * Create a full ANSI offset clause for the query.
     *
     * @param  \Illuminate\Database\Query\Builder  $query
     * @param  array  $components
     * @return string
     */
    protected virtual string compileAnsiOffset(Builder query, Dictionary<string, string> components)
    {
        // An ORDER BY clause is required to make this offset query work, so if one does
        // not exist we'll just create a dummy clause to trick the database and so it
        // does not complain about the queries for not having an "order by" clause.
        if (!components.ContainsKey("orders")) {
            components["orders"] = "order by (select 0)";
        }

        // We need to add the row number to the query so we can compare it to the offset
        // and limit values given for the statements. So we will add an expression to
        // the "select" that will give back the row numbers on each of the records.
        string orderings = components["orders"];

        components["columns"] += this.compileOver(orderings);

        components.Remove("orders");

        // Next we need to calculate the constraints that should be placed on the query
        // to get the right offset and limit from our query but if there is no limit
        // set we will just handle the offset only since that is all that matters.
        string constraint = this.compileRowConstraint(query);

        string sql = this.concatenate(components);

        // We are now ready to build the final SQL query so we'll create a common table
        // expression from the query and get the records with row numbers within our
        // given limit and offset value that we just put on as a query constraint.
        return this.compileTableExpression(sql, constraint);
    }

    /**
     * Compile the over statement for a table expression.
     *
     * @param  string  $orderings
     * @return string
     */
    protected virtual string compileOver(string orderings)
    {
        return ", row_number() over (" + orderings + ") as row_num";
    }

    /**
     * Compile the limit / offset row constraint for a query.
     *
     * @param  \Illuminate\Database\Query\Builder  $query
     * @return string
     */
    protected virtual string compileRowConstraint(Builder query)
    {
        int? start = query._offset + 1;

        if (query._limit > 0) {
            int? finish = query._offset + query._limit;

            return "between " + start + " and " + finish;
        }

        return ">= " + start;
    }

    /**
     * Compile a common table expression for a query.
     *
     * @param  string  $sql
     * @param  string  $constraint
     * @return string
     */
    protected virtual string compileTableExpression(string sql, string constraint)
    {
        return "select * from (" + sql + ") as temp_table where row_num " + constraint;
    }

    /**
     * Compile the "limit" portions of the query.
     *
     * @param  \Illuminate\Database\Query\Builder  $query
     * @param  int  $limit
     * @return string
     */
    protected override string compileLimit(Builder query, int? limit)
    {
        return "";
    }

    /**
     * Compile the "offset" portions of the query.
     *
     * @param  \Illuminate\Database\Query\Builder  $query
     * @param  int  $offset
     * @return string
     */
    protected override string compileOffset(Builder query, int? offset)
    {
        return "";
    }

    /**
     * Compile a truncate table statement into SQL.
     *
     * @param  \Illuminate\Database\Query\Builder  $query
     * @return array
     */
    public override Dictionary<string, object[]> compileTruncate(Builder query)
    {
        return new Dictionary<string, object[]>(){
            {"truncate table " + this.wrapTable(query._from), new object[0]}
        };
    }

    /**
     * Compile an exists statement into SQL.
     *
     * @param \Illuminate\Database\Query\Builder $query
     * @return string
     */
    public override string compileExists(Builder query)
    {
        Builder existsQuery = query.clone();

        existsQuery._columns = new object[0];

        return this.compileSelect(existsQuery.selectRaw("1 [exists]").limit(1));
    }

    /**
     * Compile a "where date" clause.
     *
     * @param  \Illuminate\Database\Query\Builder  $query
     * @param  array  $where
     * @return string
     */
    protected override string whereDate(Builder query, WhereOptions where)
    {
        string value = this.parameter(where.value);

        return "cast(" + this.wrap(where.column) + " as date) " + where.operator1 + " " + value;
    }

    /**
     * Determine if the grammar supports savepoints.
     *
     * @return bool
     */
    public override bool supportsSavepoints()
    {
        return false;
    }

    /**
     * Get the format for database stored dates.
     *
     * @return string
     */
    public override string getDateFormat()
    {
        return "yyyyMMdd  HH:mm:ss.000";
    }

    /**
     * Wrap a single string in keyword identifiers.
     *
     * @param  string  $value
     * @return string
     */
    protected override string wrapValue(string value)
    {
        if (value == "*") {
            return value;
        }

        return "[" + value.Replace("]", "]]") + "]";
    }

    /**
     * Compile an update statement into SQL.
     *
     * @param  \Illuminate\Database\Query\Builder  $query
     * @param  array  $values
     * @return string
     */
    public override string compileUpdate(Builder query, Dictionary<string, object> values)
    {
        string table = this.wrapTable(query._from);
        string alias = table;

        if (table.ToLower().Contains("] as [")) {
            string[] segments = table.Split(new string[]{ "] as [" }, StringSplitOptions.None);

            alias = "[" + segments[1];
        }

        // Each one of the columns in the update statements needs to be wrapped in the
        // keyword identifiers, also a place-holder needs to be created for each of
        // the values in the list of bindings so we can make the sets statements.
        string[] columns = new string[0];
        foreach (var e in values)
        {
            columns = ArrayUtil.push(columns, this.wrap(e.Key) + " = " + this.parameter(e.Value));
        }
        
        string columns1 = string.Join(", ", columns);

        // If the query has any "join" clauses, we will setup the joins on the builder
        // and compile them so we can attach them to this update, as update queries
        // can get join statements to attach to other tables when they're needed.
        string joins = "";
        if (query._joins != null && query._joins.Length > 0) {
            joins = " " + this.compileJoins(query, query._joins);
        }

        // Of course, update queries may also be constrained by where clauses so we'll
        // need to compile the where clauses and attach it to the query so only the
        // intended records are updated by the SQL statements we generate to run.
        string where = this.compileWheres(query);

        if (!string.IsNullOrEmpty(joins)) {
            return ("update " + alias + " set " + columns1 + " from " + table + joins + " " + where).Trim();
        }

        return ("update " + table + joins + " set " + columns1 + " " + where).Trim();
    }

    /**
     * Wrap a table in keyword identifiers.
     *
     * @param  \Illuminate\Database\Query\Expression|string  $table
     * @return string
     */
    public override string wrapTable(object table)
    {
        return this.wrapTableValuedFunction(base.wrapTable(table));
    }

    /**
     * Wrap a table in keyword identifiers.
     *
     * @param  string  $table
     * @return string
     */
    protected virtual string wrapTableValuedFunction(string table)
    {
        MatchCollection matches = Regex.Matches(table, @"^(.+?)(\(.*?\))]$");
        if (matches.Count > 1) {
            table = matches[1] + "]" + matches[2];
        }

        return table;
    }
 }
}

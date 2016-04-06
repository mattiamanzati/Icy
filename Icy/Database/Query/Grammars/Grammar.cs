using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Icy.Util;
using BaseGrammar = Icy.Database.Grammar;

namespace Icy.Database.Query.Grammars
{
    // 770cd01  18 Nov circa
    public class Grammar: BaseGrammar
    {
        /**
        * The components that make up a select clause.
        *
        * @var array
        */
        protected string[] _selectComponents = new string[] {
            "aggregate",
            "columns",
            "from",
            "joins",
            "wheres",
            "groups",
            "havings",
            "orders",
            "limit",
            "offset",
            "unions",
            "lock",
        };

        /**
         * Compile a select query into SQL.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @return string
         */
        public virtual string compileSelect(Builder query)
        {
            object[] original = ArrayUtil.copy(query._columns);
            if (query._columns.Length == 0) {
                query._columns = new object[]{ "*" };
            }
            string sql = this.concatenate(this.compileComponents(query)).Trim();
            query._columns = original;
            return sql;
        }



        /**
         * Compile the components necessary for a select clause.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @return array
         */
        protected virtual Dictionary<string, string> compileComponents(Builder query)
        {
            Dictionary<string, string> sql = new Dictionary<string, string>();
            
            if(query._aggregate != null) sql["aggregate"] = this.compileAggregate(query, query._aggregate);
            if(query._columns != null && query._columns.Length > 0) sql["columns"] = this.compileColumns(query, query._columns);
            if(query._from != null) sql["from"] = this.compileFrom(query, query._from);
            if(query._joins != null && query._joins.Length > 0) sql["joins"] = this.compileJoins(query, query._joins);
            if(query._wheres != null && query._wheres.Length > 0) sql["wheres"] = this.compileWheres(query, query._wheres);
            if(query._groups != null && query._groups.Length > 0) sql["groups"] = this.compileGroups(query, query._groups);
            if(query._havings != null && query._havings.Length > 0) sql["havings"] = this.compileHavings(query, query._havings);
            if(query._orders != null && query._orders.Length > 0) sql["orders"] = this.compileOrders(query, query._orders);
            if(query._limit != null) sql["limit"] = this.compileLimit(query, query._limit);
            if(query._offset != null) sql["offset"] = this.compileOffset(query, query._offset);
            if(query._unions != null && query._unions.Length > 0) sql["unions"] = this.compileUnions(query, query._unions);
            if(query._lock != null) sql["lock"] = this.compileLock(query, query._lock);

            return sql;
        }
        
        
        /**
         * Compile an aggregated select clause.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $aggregate
         * @return string
         */
        protected virtual string compileAggregate(Builder query, AggregateOptions aggregate)
        {
            string column = this.columnize(aggregate.columns);
            // If the query has a "distinct" constraint and we're not asking for all columns
            // we need to prepend "distinct" onto the column name so that the query takes
            // it into account when it performs the aggregating operations on the data.
            if (query._distinct && column != "*") {
                column = "distinct " + column;
            }
            return "select " + aggregate.function + "(" + column + ") as aggregate";
        }

        
        /**
         * Compile the "select *" portion of the query.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $columns
         * @return string|null
         */
        protected virtual string compileColumns(Builder query, object[] columns)
        {
            // If the query is actually performing an aggregating select, we will let that
            // compiler handle the building of the select clauses, as it will need some
            // more syntax that is best handled by that function to keep things neat.
            if (query._aggregate != null) {
                return null;
            }
            string select = query._distinct ? "select distinct " : "select ";
            return select + this.columnize(columns);
        }

        
        /**
         * Compile the "from" portion of the query.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  string  $table
         * @return string
         */
        protected virtual string compileFrom(Builder query, object table)
        {
            return "from " + this.wrapTable(table);
        }

        
        /**
         * Compile the "join" portions of the query.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $joins
         * @return string
         */
        protected virtual string compileJoins(Builder query, JoinClause[] joins)
        {
            string[] sql = new string[0];
            foreach (JoinClause join in joins) {
                string table = this.wrapTable(join._table);
                // First we need to build all of the "on" clauses for the join. There may be many
                // of these clauses so we will need to iterate through each one and build them
                // separately, then we'll join them up into a single string when we're done.
                string[] clauses = new string[0];
                foreach (JoinClauseOptions clause in join._clauses) {
                    clauses = ArrayUtil.push(clauses, this.compileJoinConstraint(clause));
                }
                // Once we have constructed the clauses, we'll need to take the boolean connector
                // off of the first clause as it obviously will not be required on that clause
                // because it leads the rest of the clauses, thus not requiring any boolean.
                clauses[0] = this.removeLeadingBoolean(clauses[0]);
                string clauses1 = string.Join(" ", clauses);
                // Once we have everything ready to go, we will just concatenate all the parts to
                // build the final join statement SQL for the query and we can then return the
                // final clause back to the callers as a single, stringified join statement.
                sql = ArrayUtil.push(sql, join._type + " join " + table + " on " + clauses1);
            }
            return string.Join(" ", sql);
        }

        
        /**
         * Create a join clause constraint segment.
         *
         * @param  array  $clause
         * @return string
         */
        protected virtual string compileJoinConstraint(JoinClauseOptions clause)
        {
            if (clause.nested) {
                return this.compileNestedJoinConstraint(clause);
            }

            string first = this.wrap(clause.first);
            string second = "";
            if (clause.where) {
                if (clause.operator1 == "in" || clause.operator1 == "not in") {
                    second = "(" + string.Join(", ", ArrayUtil.repeat("?", (int)clause.second)) + ")";
                } else {
                    second = "?";
                }
            } else {
                second = this.wrap(clause.second);
            }
            return clause.boolean + " " + first + " " + clause.operator1 + " " + second;
        }

        /**
         * Create a nested join clause constraint segment.
         *
         * @param  array  $clause
         * @return string
         */
        protected virtual string compileNestedJoinConstraint(JoinClauseOptions clause)
        {
            string[] clauses = new string[0];
            
            foreach (JoinClauseOptions nestedClause in clause.join._clauses) {
                clauses = ArrayUtil.push(clauses, this.compileJoinConstraint(nestedClause));
            }
            clauses[0] = this.removeLeadingBoolean(clauses[0]);
            string clauses1 = string.Join(" ", clauses);
            return clause.boolean + " " + clauses1;
        }

        /**
         * Compile the "where" portions of the query.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @return string
         */
        protected virtual string compileWheres(Builder query, WhereOptions[] wheres = null)
        {
            string[] sql = new string[0];
            if (query._wheres == null || query._wheres.Length == 0) {
                return "";
            }
            // Each type of where clauses has its own compiler function which is responsible
            // for actually creating the where clauses SQL. This helps keep the code nice
            // and maintainable since each clause has a very small method that it uses.
            foreach (WhereOptions where in query._wheres) {
                string tmp = "";

                switch(where.type){
                    case "Basic":
                        tmp = this.whereBasic(query, where);
                        break;
                    case "Between":
                        tmp = this.whereBetween(query, where);
                        break;
                    case "Date":
                        tmp = this.whereDate(query, where);
                        break;
                    case "Day":
                        tmp = this.whereDay(query, where);
                        break;
                    case "Exists":
                        tmp = this.whereExists(query, where);
                        break;
                    case "In":
                        tmp = this.whereIn(query, where);
                        break;
                    case "InSub":
                        tmp = this.whereInSub(query, where);
                        break;
                    case "Month":
                        tmp = this.whereMonth(query, where);
                        break;
                    case "Nested":
                        tmp = this.whereNested(query, where);
                        break;
                    case "WhereNotExists":
                        tmp = this.whereNotExists(query, where);
                        break;
                    case "NotIn":
                        tmp = this.whereNotIn(query, where);
                        break;
                    case "NotInSub":
                        tmp = this.whereNotInSub(query, where);
                        break;
                    case "NotNull":
                        tmp = this.whereNotNull(query, where);
                        break;
                    case "Null":
                        tmp = this.whereNull(query, where);
                        break;
                    case "Raw":
                        tmp = this.whereRaw(query, where);
                        break;
                    case "Sub":
                        tmp = this.whereSub(query, where);
                        break;
                    case "Year":
                        tmp = this.whereYear(query, where);
                        break;

                    default:
                        throw new System.NotImplementedException("Missing where" + where.type + " method on grammar.");
                }
                sql = ArrayUtil.push(sql, where.boolean + " " + tmp);
            }
            // If we actually have some where clauses, we will strip off the first boolean
            // operator, which is added by the query builders for convenience so we can
            // avoid checking for the first clauses in each of the compilers methods.
            if (sql.Length > 0) {
                string sql1 = string.Join(" ", sql);
                return "where " + this.removeLeadingBoolean(sql1);
            }
            return "";
        }

        
        /**
         * Compile a nested where clause.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $where
         * @return string
         */
        protected virtual string whereNested(Builder query, WhereOptions where)
        {
            Builder nested = where.query;
            return "(" + this.compileWheres(nested).Substring(6) + ")";
        }

        /**
         * Compile a where condition with a sub-select.
         *
         * @param  \Illuminate\Database\Query\Builder $query
         * @param  array   $where
         * @return string
         */
        protected virtual string whereSub(Builder query, WhereOptions where)
        {
            string select = this.compileSelect(where.query);
            return this.wrap(where.column) + " " + where.operator1 + " (" + select + ")";
        }

        /**
         * Compile a basic where clause.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $where
         * @return string
         */
        protected virtual string whereBasic(Builder query, WhereOptions where)
        {
            string value = this.parameter(where.value);
            return this.wrap(where.column) + " " + where.operator1 + " " + value;
        }

        /**
         * Compile a "between" where clause.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $where
         * @return string
         */
        protected virtual string whereBetween(Builder query, WhereOptions where)
        {
            string between = where.not ? "not between" : "between";
            string str = this.wrap(where.column) + " " + between + " ? and ?";
            return this.wrap(where.column) + " " + between + " ? and ?";
        }

        /**
         * Compile a where exists clause.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $where
         * @return string
         */
        protected virtual string whereExists(Builder query, WhereOptions where)
        {
            return "exists (" + this.compileSelect(where.query) + ")";
        }

        /**
         * Compile a where exists clause.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $where
         * @return string
         */
        protected virtual string whereNotExists(Builder query, WhereOptions where)
        {
            return "not exists (" + this.compileSelect(where.query) + ")";
        }
        /**
         * Compile a "where in" clause.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $where
         * @return string
         */
        protected virtual string whereIn(Builder query, WhereOptions where)
        {
            if (where.values.Length == 0) {
                return "0 = 1";
            }
            string values = this.parameterize(where.values);
            return this.wrap(where.column) + " in (" + values + ")";
        }
        /**
         * Compile a "where not in" clause.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $where
         * @return string
         */

        protected virtual string whereNotIn(Builder query, WhereOptions where)
        {
            if (where.values.Length == 0) {
                return "1 = 1";
            }
            string values = this.parameterize(where.values);
            return this.wrap(where.column) + " not in (" + values + ")";
        }
        /**
         * Compile a where in sub-select clause.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $where
         * @return string
         */
        protected virtual string whereInSub(Builder query, WhereOptions where)
        {
            string select = this.compileSelect(where.query);
            return this.wrap(where.column) + " in (" + select + ")";
        }

        /**
         * Compile a where not in sub-select clause.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $where
         * @return string
         */
        protected virtual string whereNotInSub(Builder query, WhereOptions where)
        {
            string select = this.compileSelect(where.query);
            return this.wrap(where.column) + " not in (" + select + ")";
        }

        /**
         * Compile a "where null" clause.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $where
         * @return string
         */
        protected virtual string whereNull(Builder query, WhereOptions where)
        {
            return this.wrap(where.column) + " is null";
        }
        /**
         * Compile a "where not null" clause.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $where
         * @return string
         */
        protected virtual string whereNotNull(Builder query, WhereOptions where)
        {
            return this.wrap(where.column) + " is not null";
        }
        /**
         * Compile a "where date" clause.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $where
         * @return string
         */
        protected virtual string whereDate(Builder query, WhereOptions where)
        {
            return this.dateBasedWhere("date", query, where);
        }
        /**
         * Compile a "where day" clause.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $where
         * @return string
         */
        protected virtual string whereDay(Builder query, WhereOptions where)
        {
            return this.dateBasedWhere("day", query, where);
        }
        /**
         * Compile a "where month" clause.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $where
         * @return string
         */
        protected virtual string whereMonth(Builder query, WhereOptions where)
        {
            return this.dateBasedWhere("month", query, where);
        }
        /**
         * Compile a "where year" clause.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $where
         * @return string
         */
        protected virtual string whereYear(Builder query, WhereOptions where)
        {
            return this.dateBasedWhere("year", query, where);
        }
        /**
         * Compile a date based where clause.
         *
         * @param  string  $type
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $where
         * @return string
         */
        protected virtual string dateBasedWhere(string type, Builder query, WhereOptions where)
        {
            string value = this.parameter(where.value);
            return type + "(" + this.wrap(where.column) + ") " + where.operator1 + " " + value;
        }
        /**
         * Compile a raw where clause.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $where
         * @return string
         */
        protected virtual string whereRaw(Builder query, WhereOptions where)
        {
            return where.sql;
        }
        /**
         * Compile the "group by" portions of the query.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $groups
         * @return string
         */
        protected virtual string compileGroups(Builder query, object[] groups)
        {
            return "group by " + this.columnize(groups);
        }
        /**
         * Compile the "having" portions of the query.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $havings
         * @return string
         */
        protected virtual string compileHavings(Builder query, HavingOptions[] havings)
        {
            string sql = string.Join(" ", ArrayUtil.map(havings, this.compileHaving));
            return "having " + this.removeLeadingBoolean(sql);
        }
        /**
         * Compile a single having clause.
         *
         * @param  array   $having
         * @return string
         */
        protected virtual string compileHaving(HavingOptions having)
        {
            // If the having clause is "raw", we can just return the clause straight away
            // without doing any more processing on it. Otherwise, we will compile the
            // clause into SQL based on the components that make it up from builder.
            if (having.type == "raw") {
                return having.boolean + " " + having.sql;
            }

            return this.compileBasicHaving(having);
        }
        /**
         * Compile a basic having clause.
         *
         * @param  array   $having
         * @return string
         */
        protected virtual string compileBasicHaving(HavingOptions having)
        {
            string column = this.wrap(having.column);
            string parameter = this.parameter(having.value);
            return having.boolean + " " + column + " " + having.operator1 + " " + parameter;
        }
        /**
         * Compile the "order by" portions of the query.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $orders
         * @return string
         */
        protected virtual string compileOrders(Builder query, OrderOptions[] orders)
        {
            return "order by " + string.Join(", ", ArrayUtil.map(orders, (order) => {
                if (!string.IsNullOrEmpty(order.sql)) {
                    return order.sql;
                }
                return this.wrap(order.column) + " " + order.direction;
            }));
        }
        /**
         * Compile the "limit" portions of the query.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  int  $limit
         * @return string
         */
        protected virtual string compileLimit(Builder query, int? limit)
        {
            return "limit " + ((int) limit);
        }
        /**
         * Compile the "offset" portions of the query.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  int  $offset
         * @return string
         */
        protected virtual string compileOffset(Builder query, int? offset)
        {
            return "offset " + ((int) offset);
        }
        /**
         * Compile the "union" queries attached to the main query.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @return string
         */
        protected virtual string compileUnions(Builder query, UnionOptions[] unions)
        {
            string sql = "";
            foreach (UnionOptions union in unions) {
                sql += this.compileUnion(union);
            }
            if (query._unionOrders != null && query._unionOrders.Length > 0) {
                sql += " " + this.compileOrders(query, query._unionOrders);
            }
            if (query._unionLimit != null) {
                sql += " " + this.compileLimit(query, query._unionLimit);
            }
            if (query._unionOffset != null) {
                sql += " " + this.compileOffset(query, query._unionOffset);
            }
            return sql.TrimStart();
        }
        /**
         * Compile a single union statement.
         *
         * @param  array  $union
         * @return string
         */
        protected virtual string compileUnion(UnionOptions union)
        {
            string joiner = union.all ? " union all " : " union ";
            return joiner + union.query.toSql();
        }
        /**
         * Compile an exists statement into SQL.
         *
         * @param \Illuminate\Database\Query\Builder $query
         * @return string
         */
        public virtual string compileExists(Builder query)
        {
            string select = this.compileSelect(query);
            return "select exists(" + select + ") as " + this.wrap("exists");
        }

        /**
         * Compile an insert statement into SQL.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $values
         * @return string
         */
        public virtual string compileInsert(Builder query, Dictionary<string, object>[] values)
        {
            // Essentially we will force every insert to be treated as a batch insert which
            // simply makes creating the SQL easier for us since we can utilize the same
            // basic routine regardless of an amount of records given to us to insert.
            string table = this.wrapTable(query._from);

            string columns = this.columnize(DictionaryUtil.keys(values[0]));
            // We need to build a list of parameter place-holders of values that are bound
            // to the query. Each insert should have the exact same amount of parameter
            // bindings so we will loop through the record and parameterize them all.
            string[] parameters = new string[0];

            foreach (Dictionary<string, object> record in values) {
                object[] vals = DictionaryUtil.values(record);

                parameters = ArrayUtil.push(parameters, "("  + this.parameterize(vals) + ")");
            }

            string parameters1 = string.Join(", ", parameters);
            return "insert into " + table + " (" + columns + ") values " + parameters1;
        }

        /**
         * Compile an insert and get ID statement into SQL.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array   $values
         * @param  string  $sequence
         * @return string
         */

        public virtual string compileInsertGetId(Builder query, Dictionary<string, object> values, string sequence)
        {
            return this.compileInsertGetId(query, new Dictionary<string,object>[]{ values }, sequence);
        }

        public virtual string compileInsertGetId(Builder query, Dictionary<string, object>[] values, string sequence)
        {
            return this.compileInsert(query, values);
        }
        /**
         * Compile an update statement into SQL.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  array  $values
         * @return string
         */
        public virtual string compileUpdate(Builder query, Dictionary<string, object> values)
        {
            string table = this.wrapTable(query._from);
            // Each one of the columns in the update statements needs to be wrapped in the
            // keyword identifiers, also a place-holder needs to be created for each of
            // the values in the list of bindings so we can make the sets statements.
            string[] columns = new string[0];
            foreach (var e in values) {
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
            return ("update "+ table + joins + " set " + columns1 + " " + where).Trim();
        }

        /**
         * Compile a delete statement into SQL.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @return string
         */
        public virtual string compileDelete(Builder query)
        {
            string table = this.wrapTable(query._from);
            string where = query._wheres.Length > 0 ? this.compileWheres(query) : "";
            return ("delete from " + table + " " + where).Trim();
        }
        /**
         * Compile a truncate table statement into SQL.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @return array
         */
        public virtual Dictionary<string, object[]> compileTruncate(Builder query)
        {
            return new Dictionary<string, object[]>(){
                {"truncate " + this.wrapTable(query._from), new object[0]}
            };
        }
        /**
         * Compile the lock into SQL.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @param  bool|string  $value
         * @return string
         */
        protected virtual string compileLock(Builder query, string value)
        {
            return value == null ? "" : value;
        }

        /**
         * Determine if the grammar supports savepoints.
         *
         * @return bool
         */
        public virtual bool supportsSavepoints()
        {
            return true;
        }

        /**
         * Compile the SQL statement to define a savepoint.
         *
         * @param  string  $name
         * @return string
         */

        public virtual string compileSavepoint(string name)
        {
            return "SAVEPOINT " + name;
        }

        /**
         * Compile the SQL statement to execute a savepoint rollback.
         *
         * @param  string  $name
         * @return string
         */
        public virtual string compileSavepointRollBack(string name)
        {
            return "ROLLBACK TO SAVEPOINT " + name;
        }

        /**
         * Concatenate an array of segments, removing empties.
         *
         * @param  array   $segments
         * @return string
         */
        protected virtual string concatenate(Dictionary<string, string> segments)
        {
            return this.concatenate(DictionaryUtil.values(segments));
        }

        protected virtual string concatenate(object[] segments)
        {
            return string.Join(" ", ArrayUtil.map(ArrayUtil.filter(segments, (segment) => !string.IsNullOrEmpty(segment.ToString())), (item) => item.ToString()));
        }

        /**
         * Remove the leading boolean from a statement.
         *
         * @param  string  $value
         * @return string
         */
        protected virtual string removeLeadingBoolean(string value)
        {
            return (new Regex("^(and |or )")).Replace(value, "");
        }

    }
}

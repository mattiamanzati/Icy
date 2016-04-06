using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using Icy.Database.Query.Processors;
using Icy.Database.Query.Grammars;
using Icy.Util;
using Icy.Foundation;
using System.Text.RegularExpressions;
using System.Data;
using SchemaGrammar = Icy.Database.Schema.Grammars.Grammar;
using SqlServerSchemaGrammar = Icy.Database.Schema.Grammars.SqlServerGrammar;

namespace Icy.Database
{

    public class SqlClientContext
    {
        public SqlConnection pdo = null;
        public SqlTransaction transaction = null;
        public int transactions = 0;
    }

    public class SqlServerConnection: Connection
    {
        public SqlServerConnection(object pdo, string database = "", string tablePrefix = "", ApplicationDatabaseConnectionConfig config = default(ApplicationDatabaseConnectionConfig)) : base(pdo, database, tablePrefix, config) { }

        /**
         * Get the default query grammar instance.
         *
         * @return \Illuminate\Database\Query\Grammars\SqlServerGrammar
         */
        protected override Icy.Database.Query.Grammars.Grammar getDefaultQueryGrammar()
        {
            return this.withTablePrefix(new SqlServerGrammar());
        }

        /**
         * Get the default schema grammar instance.
         *
         * @return \Illuminate\Database\Schema\Grammars\SqlServerGrammar
         */
        protected override SchemaGrammar getDefaultSchemaGrammar()
        {
            return new SqlServerSchemaGrammar();
        }

        /**
         * Get the default post processor instance.
         *
         * @return \Illuminate\Database\Query\Processors\SqlServerProcessor
         */
        protected override Processor getDefaultPostProcessor()
        {
            return new SqlServerProcessor();
        }


        // overrides the getPdo and setPdo methods so they are "client-dependent" and use SQLServer pooling.
        public override object getPdo()
        {
            SqlClientContext ctx = ContextResolver.resolve<SqlClientContext>();

            if (ctx.pdo == null && this._pdo is Func<object>)
            {
                ctx.pdo = (SqlConnection)((Func<object>)this._pdo)();
            }
            return ctx.pdo;
        }

        public override Connection setPdo(object pdo)
        {
            SqlClientContext ctx = ContextResolver.resolve<SqlClientContext>();

            if (ctx.transactions >= 1)
            {
                throw new Exception("Cannot swap PDO while in a transaction.");
            }

            ctx.pdo = (SqlConnection)pdo;
            return this;
        }

        public override int transactionLevel()
        {
            return ContextResolver.resolve<SqlClientContext>().transactions;
        }

        public override void beginTransaction()
        {
            SqlClientContext ctx = ContextResolver.resolve<SqlClientContext>();

            ++ctx.transactions;

            if (ctx.transactions == 1)
            {
                ctx.pdo.BeginTransaction();
            }
            else if (ctx.transactions > 1 && this._queryGrammar.supportsSavepoints())
            {
                SqlCommand cmd = new SqlCommand(this._queryGrammar.compileSavepoint("trans" + ctx.transactions), ctx.pdo, ctx.transaction);
                cmd.ExecuteNonQuery();
            }

            // TODO: fire event
        }

        public override void commit()
        {
            SqlClientContext ctx = ContextResolver.resolve<SqlClientContext>();

            if (ctx.transactions == 1)
            {
                ctx.transaction.Commit();
                ctx.transaction = null;
            }
        }

        public override void rollBack()
        {
            SqlClientContext ctx = ContextResolver.resolve<SqlClientContext>();

            if (ctx.transactions == 1)
            {
                ctx.transaction.Rollback();
                ctx.transaction = null;
            }
            else if (ctx.transactions > 1 && this._queryGrammar.supportsSavepoints())
            {
                SqlCommand cmd = new SqlCommand(this._queryGrammar.compileSavepointRollBack("trans" + ctx.transactions), ctx.pdo, ctx.transaction);
                cmd.ExecuteNonQuery();
            }

            ctx.transactions = Math.Max(0, ctx.transactions - 1);

            // TODO: fire event
        }

        protected override bool causedByLostConnection(QueryException e)
        {
            SqlClientContext ctx = ContextResolver.resolve<SqlClientContext>();

            return ctx.pdo == null || ctx.pdo.State == System.Data.ConnectionState.Closed;
        }

        protected virtual string replaceQuestionWithSnail(string query)
        {
            int count = 0;
            Regex rgx = new Regex(@"\?");
            return rgx.Replace(query, (m) =>
            {
                return String.Concat("@p", count++);
            });
        }

        protected virtual SqlCommand applySqlCommandBindings(SqlCommand command, object[] bindings = null)
        {
            if (bindings != null)
            {
                for(var i = 0; i < bindings.Length; i++){
                    command.Parameters.AddWithValue("p" + i, bindings[i]);
                }
            }

            return command;
        }

        protected virtual SqlTransaction transactionObject()
        {
            SqlClientContext ctx = ContextResolver.resolve<SqlClientContext>();
            return ctx.transaction;
        }

        public override bool statement(string query, object[] bindings = null)
        {
            return this.run(query, bindings, (conn, query1, bindings1) =>
            {
                SqlConnection pdo = (SqlConnection)this.getPdo();

                using (SqlCommand cmd = pdo.CreateCommand())
                {
                    cmd.CommandText = this.replaceQuestionWithSnail(query1);
                    this.applySqlCommandBindings(cmd, bindings1);

                    if (this.transactionLevel() > 0 && this.transactionObject() != null)
                    {
                        cmd.Transaction = this.transactionObject();
                    }

                    return cmd.ExecuteNonQuery() > 0;

                }
            });
        }

        public override int affectingStatement(string query, object[] bindings = null)
        {
            return this.run(query, bindings, (conn, query1, bindings1) =>
            {
                SqlConnection pdo = (SqlConnection)this.getPdo();

                using (SqlCommand cmd = pdo.CreateCommand())
                {
                    cmd.CommandText = this.replaceQuestionWithSnail(query1);
                    this.applySqlCommandBindings(cmd, bindings1);

                    if (this.transactionLevel() > 0 && this.transactionObject() != null)
                    {
                        cmd.Transaction = this.transactionObject();
                    }

                    return cmd.ExecuteNonQuery();

                }
            });
        }

        public override Dictionary<string, object>[] selectingStatement(string query, object[] bindings = null, bool useReadPdo = true)
        {
            return this.run(query, bindings, (conn, query1, bindings1) =>
            {
                Dictionary<string, object> row;
                Dictionary<string, object>[] result = new Dictionary<string, object>[0];
                object value;
                string name;

                SqlConnection pdo = (SqlConnection)this.getPdoForSelect(useReadPdo);

                using (SqlCommand cmd = pdo.CreateCommand())
                {
                    cmd.CommandText = this.replaceQuestionWithSnail(query1);
                    this.applySqlCommandBindings(cmd, bindings1);

                    if (this.transactionLevel() > 0 && this.transactionObject() != null)
                    {
                        cmd.Transaction = this.transactionObject();
                    }

                    using (IDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            row = new Dictionary<string, object>();

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                value = reader.GetValue(i);
                                name = reader.GetName(i);

                                if (DBNull.Value.Equals(value))
                                {
                                    row[name] = null;
                                }
                                else
                                {
                                    row[name] = value;
                                }
                            }

                            result = ArrayUtil.push(result, row);
                        }
                    }
                }

                return result;
            });
        }
    }
}

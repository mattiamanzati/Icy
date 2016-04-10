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
    public class SqlServerConnection: Connection
    {
        /*
        * The actual transaction object
        */
        protected SqlTransaction _transaction = null;

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

        public override void beginTransaction()
        {

            ++this._transactions;

            if (this._transactions == 1)
            {
                ((SqlConnection)this._pdo).BeginTransaction();
            }
            else if (this._transactions > 1 && this._queryGrammar.supportsSavepoints())
            {
                SqlCommand cmd = new SqlCommand(this._queryGrammar.compileSavepoint("trans" + this._transactions), (SqlConnection)this._pdo, this._transaction);
                cmd.ExecuteNonQuery();
            }

            // TODO: fire event
        }

        public override void commit()
        {

            if (this._transactions == 1)
            {
                this._transaction.Commit();
                this._transaction = null;
            }
        }

        public override void rollBack()
        {
            if (this._transactions == 1)
            {
                this._transaction.Rollback();
                this._transaction = null;
            }
            else if (this._transactions > 1 && this._queryGrammar.supportsSavepoints())
            {
                SqlCommand cmd = new SqlCommand(this._queryGrammar.compileSavepointRollBack("trans" + this._transactions), (SqlConnection)this._pdo, this._transaction);
                cmd.ExecuteNonQuery();
            }

            this._transactions = Math.Max(0, this._transactions - 1);

            // TODO: fire event
        }

        protected override bool causedByLostConnection(QueryException e)
        {
            return this._pdo == null || ((SqlConnection)this._pdo).State == System.Data.ConnectionState.Closed;
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
                    command.Parameters.AddWithValue("p" + i, bindings[i] == null ? DBNull.Value : bindings[i]);
                }
            }

            return command;
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

                    if (this.transactionLevel() > 0 && this._transaction != null)
                    {
                        cmd.Transaction = this._transaction;
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

                    if (this.transactionLevel() > 0 && this._transaction != null)
                    {
                        cmd.Transaction = this._transaction;
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

                    if (this._transactions > 0 && this._transaction != null)
                    {
                        cmd.Transaction = this._transaction;
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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BaseGrammar = Icy.Database.Query.Grammars.Grammar;
using BaseProcessor = Icy.Database.Query.Processors.Processor;
using SqlServerGrammar = Icy.Database.Query.Grammars.SqlServerGrammar;
using SqlServerProcessor = Icy.Database.Query.Processors.SqlServerProcessor;
using Icy.Database.Query;
using Icy.Database;

namespace IcyUnitTest
{
    [TestClass]
    public class DatabaseQueryBuilderTest
    {

        protected Builder getBuilder()
        {
            return new Builder(new Connection(null));
        }
        protected Builder getSqlServerBuilder()
        {
            return new Builder(new SqlServerConnection(null));
        }

        [TestMethod]
        public void testBasicSelect()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users");
            Assert.AreEqual("select * from \"users\"", builder.toSql());
        }


        [TestMethod]
        public void testBasicSelectWithGetColumns()
        {
            Builder builder = this.getBuilder();
            Assert.AreEqual("select * from \"users\"", builder.from("users").toSql());
        }

        [TestMethod]
        public void testBasicTableWrappingProtectsQuotationMarks()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("some\"table");
            Assert.AreEqual("select * from \"some\"\"table\"", builder.toSql());
        }

        [TestMethod]
        public void testAliasWrappingAsWholeConstant()
        {
            Builder builder = this.getBuilder();
            builder.select("x.y as foo.bar").from("baz");
            Assert.AreEqual("select \"x\".\"y\" as \"foo.bar\" from \"baz\"", builder.toSql());
        }

        [TestMethod]
        public void testAddingSelects()
        {
            Builder builder = this.getBuilder();
            builder.select("foo").addSelect("bar").addSelect(new string[] { "baz", "boom" }).from("users");
            Assert.AreEqual("select \"foo\", \"bar\", \"baz\", \"boom\" from \"users\"", builder.toSql());
        }

        [TestMethod]
        public void testBasicSelectWithPrefix()
        {
            Builder builder = this.getBuilder();
            builder.getGrammar().setTablePrefix("prefix_");
            builder.select("*").from("users");
            Assert.AreEqual("select * from \"prefix_users\"", builder.toSql());
        }

        [TestMethod]
        public void testBasicSelectDistinct()
        {
            Builder builder = this.getBuilder();
            builder.distinct().select("foo", "bar").from("users");
            Assert.AreEqual("select distinct \"foo\", \"bar\" from \"users\"", builder.toSql());
        }
        [TestMethod]
        public void testBasicAlias()
        {
            Builder builder = this.getBuilder();
            builder.select("foo as bar").from("users");
            Assert.AreEqual("select \"foo\" as \"bar\" from \"users\"", builder.toSql());
        }
        [TestMethod]
        public void testAliasWithPrefix()
        {
            Builder builder = this.getBuilder();
            builder.getGrammar().setTablePrefix("prefix_");
            builder.select("*").from("users as people");
            string sql = builder.toSql();
            Assert.AreEqual("select * from \"prefix_users\" as \"prefix_people\"", builder.toSql());
        }
        [TestMethod]
        public void testJoinAliasesWithPrefix()
        {
            Builder builder = this.getBuilder();
            builder.getGrammar().setTablePrefix("prefix_");
            builder.select("*").from("services").join("translations AS t", "t.item_id", "=", "services.id");
            Assert.AreEqual("select * from \"prefix_services\" inner join \"prefix_translations\" as \"prefix_t\" on \"prefix_t\".\"item_id\" = \"prefix_services\".\"id\"", builder.toSql());
        }
        [TestMethod]
        public void testBasicTableWrapping()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("public.users");
            Assert.AreEqual("select * from \"public\".\"users\"", builder.toSql());
        }

        [TestMethod]
        public void testBasicWheres()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users").where("id", "=", 1);
            var bindings = builder.getBindings();
            Assert.AreEqual("select * from \"users\" where \"id\" = ?", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1 }, builder.getBindings());
        }

        [TestMethod]
        public void testWhereDaySqlServer()
        {
            Builder builder = this.getSqlServerBuilder();
            builder.select("*").from("users").whereDay("created_at", "=", 1);
            Assert.AreEqual("select * from [users] where day([created_at]) = ?", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1 }, builder.getBindings());
        }
        [TestMethod]
        public void testWhereMonthSqlServer()
        {
            Builder builder = this.getSqlServerBuilder();
            builder.select("*").from("users").whereMonth("created_at", "=", 5);
            Assert.AreEqual("select * from [users] where month([created_at]) = ?", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 5 }, builder.getBindings());
        }
        [TestMethod]
        public void testWhereYearSqlServer()
        {
            Builder builder = this.getSqlServerBuilder();
            builder.select("*").from("users").whereYear("created_at", "=", 2014);
            Assert.AreEqual("select * from [users] where year([created_at]) = ?", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 2014 }, builder.getBindings());
        }
        [TestMethod]
        public void testWhereBetweens()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users").whereBetween("id", new object[] { 1, 2 });
            Assert.AreEqual("select * from \"users\" where \"id\" between ? and ?", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1, 2 }, builder.getBindings());

            var binds = builder.getBindings();

            builder = this.getBuilder();
            builder.select("*").from("users").whereNotBetween("id", new object[] { 1, 2 });
            Assert.AreEqual("select * from \"users\" where \"id\" not between ? and ?", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1, 2 }, builder.getBindings());
        }

        [TestMethod]
        public void testBasicOrWheres()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users").where("id", "=", 1).orWhere("email", "=", "foo");
            Assert.AreEqual("select * from \"users\" where \"id\" = ? or \"email\" = ?", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1, "foo" }, builder.getBindings());
        }
        [TestMethod]
        public void testRawWheres()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users").whereRaw("id = ? or email = ?", new object[] { 1, "foo" });
            Assert.AreEqual("select * from \"users\" where id = ? or email = ?", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1, "foo" }, builder.getBindings());
        }
        [TestMethod]
        public void testRawOrWheres()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users").where("id", "=", 1).orWhereRaw("email = ?", new object[] { "foo" });
            Assert.AreEqual("select * from \"users\" where \"id\" = ? or email = ?", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1, "foo" }, builder.getBindings());
        }
        [TestMethod]
        public void testBasicWhereIns()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users").whereIn("id", new object[] { 1, 2, 3 });
            Assert.AreEqual("select * from \"users\" where \"id\" in (?, ?, ?)", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1, 2, 3 }, builder.getBindings());

            builder = this.getBuilder();
            builder.select("*").from("users").where("id", "=", 1).orWhereIn("id", new object[] { 1, 2, 3 });
            Assert.AreEqual("select * from \"users\" where \"id\" = ? or \"id\" in (?, ?, ?)", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1, 1, 2, 3 }, builder.getBindings());
        }
        [TestMethod]
        public void testBasicWhereNotIns()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users").whereNotIn("id", new object[] { 1, 2, 3 });
            Assert.AreEqual("select * from \"users\" where \"id\" not in (?, ?, ?)", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1, 2, 3 }, builder.getBindings());

            builder = this.getBuilder();
            builder.select("*").from("users").where("id", "=", 1).orWhereNotIn("id", new object[] { 1, 2, 3 });
            Assert.AreEqual("select * from \"users\" where \"id\" = ? or \"id\" not in (?, ?, ?)", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1, 1, 2, 3 }, builder.getBindings());
        }
        [TestMethod]
        public void testEmptyWhereIns()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users").whereIn("id", new object[] { });
            Assert.AreEqual("select * from \"users\" where 0 = 1", builder.toSql());
            CollectionAssert.AreEqual(new object[] { }, builder.getBindings());

            builder = this.getBuilder();
            builder.select("*").from("users").where("id", "=", 1).orWhereIn("id", new object[] { });
            Assert.AreEqual("select * from \"users\" where \"id\" = ? or 0 = 1", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1 }, builder.getBindings());
        }
        [TestMethod]
        public void testEmptyWhereNotIns()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users").whereNotIn("id", new object[] { });
            Assert.AreEqual("select * from \"users\" where 1 = 1", builder.toSql());
            CollectionAssert.AreEqual(new object[] { }, builder.getBindings());

            builder = this.getBuilder();
            builder.select("*").from("users").where("id", "=", 1).orWhereNotIn("id", new object[] { });
            Assert.AreEqual("select * from \"users\" where \"id\" = ? or 1 = 1", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1 }, builder.getBindings());
        }
        [TestMethod]
        public void testUnions()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users").where("id", "=", 1);
            builder.union(this.getBuilder().select("*").from("users").where("id", "=", 2));
            Assert.AreEqual("select * from \"users\" where \"id\" = ? union select * from \"users\" where \"id\" = ?", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1, 2 }, builder.getBindings());
        }
        [TestMethod]
        public void testUnionAlls()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users").where("id", "=", 1);
            builder.unionAll(this.getBuilder().select("*").from("users").where("id", "=", 2));
            Assert.AreEqual("select * from \"users\" where \"id\" = ? union all select * from \"users\" where \"id\" = ?", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1, 2 }, builder.getBindings());
        }

        [TestMethod]
        public void testMultipleUnions()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users").where("id", "=", 1);
            builder.union(this.getBuilder().select("*").from("users").where("id", "=", 2));
            builder.union(this.getBuilder().select("*").from("users").where("id", "=", 3));
            Assert.AreEqual("select * from \"users\" where \"id\" = ? union select * from \"users\" where \"id\" = ? union select * from \"users\" where \"id\" = ?", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1, 2, 3 }, builder.getBindings());
        }
        [TestMethod]
        public void testMultipleUnionAlls()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users").where("id", "=", 1);
            builder.unionAll(this.getBuilder().select("*").from("users").where("id", "=", 2));
            builder.unionAll(this.getBuilder().select("*").from("users").where("id", "=", 3));
            Assert.AreEqual("select * from \"users\" where \"id\" = ? union all select * from \"users\" where \"id\" = ? union all select * from \"users\" where \"id\" = ?", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1, 2, 3 }, builder.getBindings());
        }
        [TestMethod]
        public void testUnionOrderBys()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users").where("id", "=", 1);
            builder.union(this.getBuilder().select("*").from("users").where("id", "=", 2));
            builder.orderBy("id", "desc");
            Assert.AreEqual("select * from \"users\" where \"id\" = ? union select * from \"users\" where \"id\" = ? order by \"id\" desc", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1, 2 }, builder.getBindings());
        }
        [TestMethod]
        public void testUnionLimitsAndOffsets()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users");
            builder.union(this.getBuilder().select("*").from("dogs"));
            builder.skip(5).take(10);
            Assert.AreEqual("select * from \"users\" union select * from \"dogs\" limit 10 offset 5", builder.toSql());
        }
        [TestMethod]
        public void testUnionWithJoin()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users");
            builder.union(this.getBuilder().select("*").from("dogs").join("breeds", (join) =>
            {
                join.on("dogs.breed_id", "=", "breeds.id")
                    .where("breeds.is_native", "=", 1);
            }));
            Assert.AreEqual("select * from \"users\" union select * from \"dogs\" inner join \"breeds\" on \"dogs\".\"breed_id\" = \"breeds\".\"id\" and \"breeds\".\"is_native\" = ?", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1 }, builder.getBindings());
        }
        [TestMethod]
        public void testSubSelectWhereIns()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users").whereIn("id", (q) =>
            {
                q.select("id").from("users").where("age", ">", 25).take(3);
            });
            Assert.AreEqual("select * from \"users\" where \"id\" in (select \"id\" from \"users\" where \"age\" > ? limit 3)", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 25 }, builder.getBindings());

            builder = this.getBuilder();
            builder.select("*").from("users").whereNotIn("id", (q) =>
            {
                q.select("id").from("users").where("age", ">", 25).take(3);
            });
            Assert.AreEqual("select * from \"users\" where \"id\" not in (select \"id\" from \"users\" where \"age\" > ? limit 3)", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 25 }, builder.getBindings());
        }
        [TestMethod]
        public void testBasicWhereNulls()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users").whereNull("id");
            Assert.AreEqual("select * from \"users\" where \"id\" is null", builder.toSql());
            CollectionAssert.AreEqual(new object[] { }, builder.getBindings());

            builder = this.getBuilder();
            builder.select("*").from("users").where("id", "=", 1).orWhereNull("id");
            Assert.AreEqual("select * from \"users\" where \"id\" = ? or \"id\" is null", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1 }, builder.getBindings());
        }
        [TestMethod]
        public void testBasicWhereNotNulls()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users").whereNotNull("id");
            Assert.AreEqual("select * from \"users\" where \"id\" is not null", builder.toSql());
            CollectionAssert.AreEqual(new object[] { }, builder.getBindings());

            builder = this.getBuilder();
            builder.select("*").from("users").where("id", ">", 1).orWhereNotNull("id");
            Assert.AreEqual("select * from \"users\" where \"id\" > ? or \"id\" is not null", builder.toSql());
            CollectionAssert.AreEqual(new object[] { 1 }, builder.getBindings());
        }
        [TestMethod]
        public void testGroupBys()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users").groupBy("id", "email");
            Assert.AreEqual("select * from \"users\" group by \"id\", \"email\"", builder.toSql());

            builder = this.getBuilder();
            builder.select("*").from("users").groupBy(new object[] { "id", "email" });
            Assert.AreEqual("select * from \"users\" group by \"id\", \"email\"", builder.toSql());
        }
        [TestMethod]
        public void testOrderBys()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users").orderBy("email").orderBy("age", "desc");
            Assert.AreEqual("select * from \"users\" order by \"email\" asc, \"age\" desc", builder.toSql());

            builder = this.getBuilder();
            builder.select("*").from("users").orderBy("email").orderByRaw("\"age\" ? desc", new object[] { "foo" });
            Assert.AreEqual("select * from \"users\" order by \"email\" asc, \"age\" ? desc", builder.toSql());
            CollectionAssert.AreEqual(new object[] { "foo" }, builder.getBindings());
        }
        [TestMethod]
        public void testHavings()
        {
            Builder builder = this.getBuilder();
            builder.select("*").from("users").having("email", ">", 1);
            Assert.AreEqual("select * from \"users\" having \"email\" > ?", builder.toSql());

            builder = this.getBuilder();
            builder.select("*").from("users")
                .orHaving("email", "=", "test@example.com")
                .orHaving("email", "=", "test2@example.com");
            Assert.AreEqual("select * from \"users\" having \"email\" = ? or \"email\" = ?", builder.toSql());

            builder = this.getBuilder();
            builder.select("*").from("users").groupBy("email").having("email", ">", 1);
            Assert.AreEqual("select * from \"users\" group by \"email\" having \"email\" > ?", builder.toSql());

            builder = this.getBuilder();
            builder.select("email as foo_email").from("users").having("foo_email", ">", 1);
            Assert.AreEqual("select \"email\" as \"foo_email\" from \"users\" having \"foo_email\" > ?", builder.toSql());

            builder = this.getBuilder();
            builder.select(new object[] { "category", new Expression("count(*) as \"total\"") }).from("item").where("department", "=", "popular").groupBy("category").having("total", ">", new Expression("3"));
            Assert.AreEqual("select \"category\", count(*) as \"total\" from \"item\" where \"department\" = ? group by \"category\" having \"total\" > 3", builder.toSql());

            builder = this.getBuilder();
            builder.select(new object[] { "category", new Expression("count(*) as \"total\"") }).from("item").where("department", "=", "popular").groupBy("category").having("total", ">", 3);
            Assert.AreEqual("select \"category\", count(*) as \"total\" from \"item\" where \"department\" = ? group by \"category\" having \"total\" > ?", builder.toSql());
        }

    }
}

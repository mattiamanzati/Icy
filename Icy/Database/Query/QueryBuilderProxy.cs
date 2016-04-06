using System;
using System.Collections.Generic;
using QueryBuilder = Icy.Database.Query.Builder;

namespace Icy.Database.Query
{
    public interface QueryBuilderProxy<T> where T : class, new()
    {
        QueryBuilder getQuery();
    }

    public static class QueryBuilderProxyMixin
    {
        public static M addBinding<M>(this QueryBuilderProxy<M> self, object value, string type1 = "where") where M: class, QueryBuilderProxy<M>, new() { self.getQuery().addBinding(value, type1); return self as M; }
        public static M addBinding<M>(this QueryBuilderProxy<M> self, object[] value, string type1 = "where") where M: class, QueryBuilderProxy<M>, new() { self.getQuery().addBinding(value, type1); return self as M; }
        public static M addNestedWhereQuery<M>(this QueryBuilderProxy<M> self, QueryBuilder query, string boolean = "and") where M: class, QueryBuilderProxy<M>, new() { self.getQuery().addNestedWhereQuery(query, boolean); return self as M; }
        public static M addSelect<M>(this QueryBuilderProxy<M> self, Expression column) where M: class, QueryBuilderProxy<M>, new() { self.getQuery().addSelect(column); return self as M; }
        public static M addSelect<M>(this QueryBuilderProxy<M> self, params object[] columns) where M: class, QueryBuilderProxy<M>, new() { self.getQuery().addSelect(columns); return self as M; }
        public static M addWhereExistsQuery<M>(this QueryBuilderProxy<M> self, QueryBuilder query, string boolean = "and", bool not = false) where M: class, QueryBuilderProxy<M>, new() { self.getQuery().addWhereExistsQuery(query, boolean, not); return self as M; }
        public static int aggregate<M>(this QueryBuilderProxy<M> self, string function, object[] columns = null) where M: class, QueryBuilderProxy<M>, new() { return self.getQuery().aggregate(function, columns); }
        public static T aggregate<T, M>(this QueryBuilderProxy<M> self, string function, object[] columns = null) where M: class, QueryBuilderProxy<M>, new() { return self.getQuery().aggregate<T>(function, columns); }
        public static T average<T, M>(this QueryBuilderProxy<M> self, object column) where M: class, QueryBuilderProxy<M>, new() { return self.getQuery().average<T>(column); }
        public static T avg<T, M>(this QueryBuilderProxy<M> self, object column) where M: class, QueryBuilderProxy<M>, new() { return self.getQuery().avg<T>(column); }
        public static bool chunk<M>(this QueryBuilderProxy<M> self, int count, Func<Dictionary<string, object>[], bool> callback) where M: class, QueryBuilderProxy<M>, new() { return self.getQuery().chunk(count, callback); }
        /*
        public static M clone<M>(this QueryBuilderProxy<M> self, ) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static int count<M>(this QueryBuilderProxy<M> self, object column) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static int count<M>(this QueryBuilderProxy<M> self, object[] columns = null) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static int decrement<M>(this QueryBuilderProxy<M> self, string column, int amount = 1, Dictionary<string, object> extra = null) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static int delete<M>(this QueryBuilderProxy<M> self, object id = null) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M distinct<M>(this QueryBuilderProxy<M> self, ) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M dynamicWhere<M>(this QueryBuilderProxy<M> self, string method, object[] parameters) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static bool each<M>(this QueryBuilderProxy<M> self, Func<Dictionary<string, object>, int, bool> callback, int count = 1000) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static bool exists<M>(this QueryBuilderProxy<M> self, ) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static Dictionary<string, object> find<M>(this QueryBuilderProxy<M> self, int id, object[] columns = null) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static Dictionary<string, object> first<M>(this QueryBuilderProxy<M> self, object[] columns = null) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M forNestedWhere<M>(this QueryBuilderProxy<M> self, ) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M forPage<M>(this QueryBuilderProxy<M> self, int page, int perPage = 15) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M from<M>(this QueryBuilderProxy<M> self, object table) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static Dictionary<string, object>[] get<M>(this QueryBuilderProxy<M> self, object[] columns = null) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static object[] getBindings<M>(this QueryBuilderProxy<M> self, ) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static Icy.Database.ConnectionInterface getConnection<M>(this QueryBuilderProxy<M> self, ) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static int getCountForPagination<M>(this QueryBuilderProxy<M> self, object[] columns = null) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static Icy.Database.Query.Grammars.Grammar getGrammar<M>(this QueryBuilderProxy<M> self, ) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static Icy.Database.Query.Processors.Processor getProcessor<M>(this QueryBuilderProxy<M> self, ) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static Dictionary<string, object[]> getRawBindings<M>(this QueryBuilderProxy<M> self, ) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M groupBy<M>(this QueryBuilderProxy<M> self, params object[] args) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M having<M>(this QueryBuilderProxy<M> self, object column, string operator1 = null, object value = null, string boolean = "and") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M havingRaw<M>(this QueryBuilderProxy<M> self, string sql, object[] bindings = null, string boolean = "and") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static string implode<M>(this QueryBuilderProxy<M> self, string column, string glue = "") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static int increment<M>(this QueryBuilderProxy<M> self, string column, int amount = 1, Dictionary<string, object> extra = null) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static bool insert<M>(this QueryBuilderProxy<M> self, Dictionary<string, object> values) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static bool insert<M>(this QueryBuilderProxy<M> self, Dictionary<string, object>[] values) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static int insertGetId<M>(this QueryBuilderProxy<M> self, Dictionary<string, object> values, string sequence = null) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M join<M>(this QueryBuilderProxy<M> self, string table, Action<JoinClause> callback, string operator1 = null, object two = null, string type1 = "inner", bool where = false) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M join<M>(this QueryBuilderProxy<M> self, string table, object one, string operator1, object two, string type1 = "inner", bool where = false) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M joinWhere<M>(this QueryBuilderProxy<M> self, string table, Action<JoinClause> callback, string operator1 = null, object two = null, string type = "inner") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M joinWhere<M>(this QueryBuilderProxy<M> self, string table, object one, string operator1, object two, string type = "inner") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M latest<M>(this QueryBuilderProxy<M> self, string column = "created_at") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M leftJoin<M>(this QueryBuilderProxy<M> self, string table, Action<JoinClause> callback, string operator1 = null, object two = null) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M leftJoin<M>(this QueryBuilderProxy<M> self, string table, object one, string operator1, object two) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M leftJoinWhere<M>(this QueryBuilderProxy<M> self, string table, Action<JoinClause> callback, string operator1 = null, object two = null) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M leftJoinWhere<M>(this QueryBuilderProxy<M> self, string table, object one, string operator1, object two) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M limit<M>(this QueryBuilderProxy<M> self, int value) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M lockForUpdate<M>(this QueryBuilderProxy<M> self, ) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M locks<M>(this QueryBuilderProxy<M> self, bool value = true) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static T max<T, M>(this QueryBuilderProxy<M> self, object column) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M mergeBindings<M>(this QueryBuilderProxy<M> self, M query) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static void mergeWheres<M>(this QueryBuilderProxy<M> self, WhereOptions[] wheres, object[] bindings) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static T min<T, M>(this QueryBuilderProxy<M> self, object column) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M newQuery<M>(this QueryBuilderProxy<M> self, ) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M offset<M>(this QueryBuilderProxy<M> self, int value) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M oldest<M>(this QueryBuilderProxy<M> self, string column = "created_at") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M orderBy<M>(this QueryBuilderProxy<M> self, object column, string direction = "asc") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M orderByRaw<M>(this QueryBuilderProxy<M> self, string sql, object[] bindings = null) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M orHaving<M>(this QueryBuilderProxy<M> self, object column, string operator1 = null, object value = null) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M orHavingRaw<M>(this QueryBuilderProxy<M> self, string sql, object[] bindings = null) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M orWhere<M>(this QueryBuilderProxy<M> self, object column, string operator1 = null, object value = null) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M orWhereBetween<M>(this QueryBuilderProxy<M> self, object column, object[] values) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M orWhereDate<M>(this QueryBuilderProxy<M> self, object column, string operator1, object value) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M orWhereExists<M>(this QueryBuilderProxy<M> self, Action<Builder> callback, bool not = false) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M orWhereIn<M>(this QueryBuilderProxy<M> self, object column, object[] values) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M orWhereNotBetween<M>(this QueryBuilderProxy<M> self, object column, object[] values) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M orWhereNotExists<M>(this QueryBuilderProxy<M> self, Action<Builder> callback) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M orWhereNotIn<M>(this QueryBuilderProxy<M> self, object column, object[] values) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M orWhereNotNull<M>(this QueryBuilderProxy<M> self, object column) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M orWhereNull<M>(this QueryBuilderProxy<M> self, object column) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M orWhereRaw<M>(this QueryBuilderProxy<M> self, string sql, object[] bindings = null) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static object[] pluck<M>(this QueryBuilderProxy<M> self, string column) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static Dictionary<object, object> pluck<M>(this QueryBuilderProxy<M> self, string column, string key) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static Expression raw<M>(this QueryBuilderProxy<M> self, object value) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M rightJoin<M>(this QueryBuilderProxy<M> self, string table, Action<JoinClause> callback, string operator1 = null, object two = null) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M rightJoin<M>(this QueryBuilderProxy<M> self, string table, object one, string operator1, object two) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M rightJoinWhere<M>(this QueryBuilderProxy<M> self, string table, Action<JoinClause> callback, string operator1 = null, object two = null) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M rightJoinWhere<M>(this QueryBuilderProxy<M> self, string table, object one, string operator1, object two) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M select<M>(this QueryBuilderProxy<M> self, Expression column) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M select<M>(this QueryBuilderProxy<M> self, params object[] columns) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M selectRaw<M>(this QueryBuilderProxy<M> self, string expression, object[] bindings = null) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M selectSub<M>(this QueryBuilderProxy<M> self, M query, string as1) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M selectSub<M>(this QueryBuilderProxy<M> self, Action<Builder> callback, string as1) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M selectSub<M>(this QueryBuilderProxy<M> self, string query, string as1, object[] bindings) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M setBindings<M>(this QueryBuilderProxy<M> self, object[] bindings, string type1 = "where") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M sharedLock<M>(this QueryBuilderProxy<M> self, ) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M skip<M>(this QueryBuilderProxy<M> self, int value) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static T sum<T, M>(this QueryBuilderProxy<M> self, object column) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M take<M>(this QueryBuilderProxy<M> self, int value) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static string toSql<M>(this QueryBuilderProxy<M> self, ) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static void truncate<M>(this QueryBuilderProxy<M> self, ) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M union<M>(this QueryBuilderProxy<M> self, M query, bool all = false) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M union<M>(this QueryBuilderProxy<M> self, Action<Builder> callback, bool all = false) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M unionAll<M>(this QueryBuilderProxy<M> self, M query) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static int update<M>(this QueryBuilderProxy<M> self, Dictionary<string, object> values) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M useWritePdo<M>(this QueryBuilderProxy<M> self, ) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static object value<M>(this QueryBuilderProxy<M> self, string column) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M where<M>(this QueryBuilderProxy<M> self, Action<Builder> query, string operator1 = null, object value = null, string boolean = "and") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M where<M>(this QueryBuilderProxy<M> self, Dictionary<object, object> column, string operator1 = null, object value = null, string boolean = "and") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M where<M>(this QueryBuilderProxy<M> self, Dictionary<object, object>[] columns, string operator1 = null, object value = null, string boolean = "and") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M where<M>(this QueryBuilderProxy<M> self, object column, object value) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M where<M>(this QueryBuilderProxy<M> self, object column, string operator1 = null, Action<Builder> value = null, string boolean = "and") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M where<M>(this QueryBuilderProxy<M> self, object column, string operator1 = null, object value = null, string boolean = "and") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M whereBetween<M>(this QueryBuilderProxy<M> self, object column, object[] values, string boolean = "and", bool not = false) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M whereDate<M>(this QueryBuilderProxy<M> self, object column, string operator1, object value, string boolean = "and") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M whereDay<M>(this QueryBuilderProxy<M> self, object column, string operator1, int value, string boolean = "and") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M whereExists<M>(this QueryBuilderProxy<M> self, Action<Builder> callback, string boolean = "and", bool not = false) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M whereIn<M>(this QueryBuilderProxy<M> self, object column, M values, string boolean = "and", bool not = false) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M whereIn<M>(this QueryBuilderProxy<M> self, object column, Icy.Support.IArrayable values, string boolean = "and", bool not = false) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M whereIn<M>(this QueryBuilderProxy<M> self, object column, Action<Builder> values, string boolean = "and", bool not = false) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M whereIn<M>(this QueryBuilderProxy<M> self, object column, object[] values, string boolean = "and", bool not = false) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M whereMonth<M>(this QueryBuilderProxy<M> self, object column, string operator1, int value, string boolean = "and") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M whereNested<M>(this QueryBuilderProxy<M> self, Action<Builder> callback, string boolean = "and") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M whereNotBetween<M>(this QueryBuilderProxy<M> self, object column, object[] values, string boolean = "and") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M whereNotExists<M>(this QueryBuilderProxy<M> self, Action<Builder> callback, string boolean = "and") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M whereNotIn<M>(this QueryBuilderProxy<M> self, object column, Action<Builder> query, string boolean = "and") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M whereNotIn<M>(this QueryBuilderProxy<M> self, object column, object[] values, string boolean = "and") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M whereNotNull<M>(this QueryBuilderProxy<M> self, object column, string boolean = "and") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M whereNull<M>(this QueryBuilderProxy<M> self, object column, string boolean = "and", bool not = false) where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M whereRaw<M>(this QueryBuilderProxy<M> self, string sql, object[] bindings = null, string boolean = "and") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
        public static M whereYear<M>(this QueryBuilderProxy<M> self, object column, string operator1, object value, string boolean = "and") where M: class, QueryBuilderProxy<M>, new() { self.getQuery(); return self as M; }
    
         */
     }
}

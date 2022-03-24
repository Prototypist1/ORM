using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ORM
{


    // join 
    // where
    // group by 
    // having
    // order by
    // select 


    //public class Table {
    //    public string TableName() { 
    //    }
    //}



    public static class Sql {
        // do left and right have to be the same type?
        // they probably should be
        public static EqualOperator IsEquals(this ISqlCode left, ISqlCode right) 
        {
            return new EqualOperator(left, right);
        }

        public static CountOperator Count(this SqlCollection sqlCollection)
        {
            return new CountOperator(sqlCollection);
        }

        public static GreaterThanOperator IsGreater(this ISqlNumericValue left, ISqlNumericValue right)
        {
            return new GreaterThanOperator(left, right);
        }
    }

    public class QueryCanExecute<T> : ISqlCode
    {
        private readonly QueryModel queryModel;

        internal QueryCanExecute(QueryModel queryModel)
        {
            this.queryModel = queryModel ?? throw new ArgumentNullException(nameof(queryModel));
        }

        public string ToCode()
        {
            var res =
$@"SELECT {String.Join(", ", queryModel.select.Select(x => x.ToCode()))}
FROM {queryModel.from.GetType().Name}";

            foreach (var join in queryModel.joins ?? Array.Empty<SqlJoin>())
            {
                res += Environment.NewLine + $"INNER JOIN {join.table.Name} ON {join.condition.ToCode()}";
            }

            if (queryModel.where != null) {
                res += Environment.NewLine + $"WHERE {queryModel.where.ToCode()}";
            }

            if (queryModel.groupBy != null)
            {
                res += Environment.NewLine + $"GROUP BY {String.Join(", ", queryModel.groupBy.Select(x => x.ToCode()))}";
            }

            if (queryModel.having != null)
            {
                res += Environment.NewLine + $"HAVING {queryModel.having.ToCode()}";
            }

            if (queryModel.ordersBy != null)
            {
                res += Environment.NewLine + $"ORDER BY {String.Join(", ", queryModel.ordersBy.Select(x => x.inner.ToCode()))}";
            }

            return res;
        }
    }

    public class QueryCanSelect<TJoinedTables> 
    {
        internal readonly TJoinedTables joinedTabls;
        internal readonly QueryModel queryModel;

        internal QueryCanSelect(TJoinedTables joinedTabls, QueryModel queryModel)
        {
            this.joinedTabls = joinedTabls;
            this.queryModel = queryModel ?? throw new ArgumentNullException(nameof(queryModel));
        }


        public QueryCanExecute<T> Select<T>(Func<TJoinedTables, T> select)
        {
            var result = select(joinedTabls);

            var copy = queryModel.ShallowCopy();

            copy.select = typeof(T).GetProperties()
                .Where(prop => prop.CanRead && typeof(ISqlValue).IsAssignableFrom(prop.PropertyType))
                .Select(prop => (ISqlValue)prop.GetValue(result))
                .ToArray();

            return new QueryCanExecute<T>(copy);
        }
    }

    public class QueryCanOrderBy<TJoinedTables> : QueryCanSelect<TJoinedTables>
    {
        internal QueryCanOrderBy(TJoinedTables joinedTabls, QueryModel queryModel) : base(joinedTabls, queryModel)
        {
        }


        public QueryCanSelect<TJoinedTables> OderBy(params Func<TJoinedTables, SqlOrder>[] orderBys)
        {
            var result = orderBys.Select(element => element(joinedTabls)).ToArray();

            var copy = queryModel.ShallowCopy();
            copy.ordersBy = result;

            return new QueryCanSelect<TJoinedTables>(joinedTabls, copy);
        }

        public QueryCanSelect<TJoinedTables> OderBy(params Func<TJoinedTables, ISqlValue>[] orderBys)
        {
            var result = orderBys.Select(element => new SqlOrder(element(joinedTabls))).ToArray();

            var copy = queryModel.ShallowCopy();
            copy.ordersBy = result;

            return new QueryCanSelect<TJoinedTables>(joinedTabls, copy);
        }

    }

    public class QueryCanHaving<TJoinedTables> : QueryCanOrderBy<TJoinedTables>
    {
        internal QueryCanHaving(TJoinedTables joinedTabls, QueryModel queryModel) : base(joinedTabls, queryModel)
        {
        }

        public QueryCanGroupBy<TJoinedTables> Having(Func<TJoinedTables, ISqlValue<bool>> having)
        {
            var result = having(joinedTabls);

            var copy = queryModel.ShallowCopy();
            copy.having = result;

            return new QueryCanGroupBy<TJoinedTables>(joinedTabls, copy);
        }
    }

    public class QueryCanGroupBy<TJoinedTables> : QueryCanOrderBy<TJoinedTables>
    {
        internal QueryCanGroupBy(TJoinedTables joinedTabls, QueryModel queryModel) : base(joinedTabls, queryModel)
        {
        }


    }

    public static class QueryCabGroupByExtension {

        public static QueryCanHaving<GroupedJoinedTables<TKey, T1>> GroupBy<T1, TKey>(this QueryCanGroupBy<JoinedTables<T1>> target,Func<JoinedTables<T1>, TKey> condition)
        {
            var result = condition(target.joinedTabls);
            var copy = target.queryModel.ShallowCopy();
            copy.groupBy = typeof(TKey).GetProperties()
                 .Where(prop => prop.CanRead && typeof(ISqlValue).IsAssignableFrom(prop.PropertyType))
                 .Select(prop => (ISqlValue)prop.GetValue(result))
                 .ToArray();
            return new QueryCanHaving<GroupedJoinedTables<TKey, T1>>(target.joinedTabls.Group(result), copy);
        }

        public static QueryCanHaving<GroupedJoinedTables<TKey, T1, T2>> GroupBy<T1, T2, TKey>(this QueryCanGroupBy<JoinedTables<T1, T2>> target, Func<JoinedTables<T1, T2>, TKey> condition)
        {
            var result = condition(target.joinedTabls);
            var copy = target.queryModel.ShallowCopy();
            copy.groupBy = typeof(TKey).GetProperties()
                 .Where(prop => prop.CanRead && typeof(ISqlValue).IsAssignableFrom(prop.PropertyType))
                 .Select(prop => (ISqlValue)prop.GetValue(result))
                 .ToArray();
            return new QueryCanHaving<GroupedJoinedTables<TKey, T1, T2>>(target.joinedTabls.Group(result), copy);
        }
    }

    public class QueryCanWhere<TJoinedTables> : QueryCanGroupBy<TJoinedTables> {
        internal QueryCanWhere(TJoinedTables joinedTabls, QueryModel queryModel) : base(joinedTabls, queryModel)
        {
        }

        public QueryCanGroupBy<TJoinedTables> InnerWhere(Func<TJoinedTables, ISqlValue<bool>> condition)
        {
            var result = condition(joinedTabls);

            var copy = queryModel.ShallowCopy();
            copy.where = result;

            return new QueryCanGroupBy<TJoinedTables>(joinedTabls, copy);
        }
    }

    public static class QueryCanWhereExtensions {
        public static QueryCanGroupBy<TJoinedTables> Where<TJoinedTables, T1>(this QueryCanWhere<TJoinedTables> self, Func<T1, ISqlValue<bool>> condition)
            where TJoinedTables : ITableSetContains<T1>
        {
            return self.InnerWhere(x => condition(x.Get<T1>()));
        }
        public static QueryCanGroupBy<TJoinedTables> Where<TJoinedTables, T1, T2>(this QueryCanWhere<TJoinedTables>  self, Func<T1, T2, ISqlValue<bool>> condition)
            where TJoinedTables : ITableSetContains<T1>, ITableSetContains<T2>
        {
            return self.InnerWhere(x=> condition(x.Get<T1>(), x.Get<T2>()));
        } 
    }

    public class Query {

        public static Query<T1> From<T1>()
            where T1 : ISqlTable, new()
        {
            var t1 = new T1();
            return new Query<T1>(new JoinedTables<T1>(t1),new QueryModel() { from = t1});
        }
    }

    public class Query<T1> : QueryCanWhere<JoinedTables<T1>>
    {

        internal Query(JoinedTables<T1> joinedTabls, QueryModel queryModel) : base(joinedTabls, queryModel)
        {
        }

        public Query<T1, T2> Join<T2>(Func<JoinedTables<T1, T2>, ISqlValue<bool>> condition)
            where T2: new ()
        {
            var nextTableSet = joinedTabls.Join<T2>();
            var result = condition(nextTableSet);

            var copy = queryModel.ShallowCopy();
            var nextJoins = copy.joins?.ToList() ?? new List<SqlJoin>();
            nextJoins.Add(new SqlJoin(result, typeof(T2)));
            copy.joins = nextJoins.ToArray();

            return new Query<T1,T2>(nextTableSet, copy);
        }
    }

    public class Query<T1, T2> : QueryCanWhere<JoinedTables<T1, T2>>
    {
        internal Query(JoinedTables<T1, T2> joinedTabls, QueryModel queryModel) : base(joinedTabls, queryModel)
        {
        }
    }

    public interface ITableSetContains<T> {
        T Get();
    }


    public interface ITableSetCollection<T> {
        SqlTableCollection<T> Get();
    }

    public static class TableSetExtensions {
        public static T Get<T>(this ITableSetContains<T> target) => target.Get();
        

        public static SqlTableCollection<T> Get<T>(this ITableSetCollection<T> target) => target.Get();
    }

    public class JoinedTables {}

    public class JoinedTables<T1> : JoinedTables, ITableSetContains<T1>
    {
        protected readonly T1 t1;

        public JoinedTables(T1 t1)
        {
            this.t1 = t1;
        }

        internal JoinedTables<T1, T2> Join<T2>()
            where T2 : new()
        {
            return new JoinedTables<T1, T2>(t1, new T2());
        }

        internal GroupedJoinedTables<TKey, T1> Group<TKey>(TKey group) =>new GroupedJoinedTables<TKey, T1>(group, new SqlTableCollection<T1>(t1));
        T1 ITableSetContains<T1>.Get() => t1;
    }

    public class JoinedTables<T1, T2> : JoinedTables<T1> , ITableSetContains<T2>
    {
        private readonly T2 t2;

        public JoinedTables(T1 t1,T2 t2) : base(t1)
        {
            this.t2 = t2;
        }

        internal new GroupedJoinedTables<TKey, T1, T2> Group<TKey>(TKey group) => new GroupedJoinedTables<TKey, T1, T2>(group, new SqlTableCollection<T1>(t1), new SqlTableCollection<T2>(t2));
        T2 ITableSetContains<T2>.Get() => t2;
    }

    // this is a bit weird columns show up in the key and the collections, you shouldn't access a key that is in the grouping as a collection 
    // can you layer groups? yeah, I think you just need to put a select between each group
    public class GroupedJoinedTables<TKey, T1>: ITableSetContains<TKey>, ITableSetCollection<T1>
    {
        private readonly TKey tkey;
        private readonly SqlTableCollection<T1> t1;

        public GroupedJoinedTables(TKey tkey, SqlTableCollection<T1> t1)
        {
            this.tkey = tkey;
            this.t1 = t1 ?? throw new ArgumentNullException(nameof(t1));
        }

        TKey ITableSetContains<TKey>.Get() => tkey;
        SqlTableCollection<T1> ITableSetCollection<T1>.Get() => t1;
    }

    public class GroupedJoinedTables<TKey, T1, T2> : GroupedJoinedTables<TKey, T1>, ITableSetCollection<T2>
    {

        private readonly SqlTableCollection<T2> t2;

        public GroupedJoinedTables(TKey tkey, SqlTableCollection<T1> t1, SqlTableCollection<T2> t2) : base(tkey, t1)
        {
            this.t2 = t2 ?? throw new ArgumentNullException(nameof(t2));
        }

        SqlTableCollection<T2> ITableSetCollection<T2>.Get() => t2;
    }


    //public class SqlBool { }
    public class SqlTableCollection<T> {
        private T t;

        public SqlTableCollection(T t)
        {
            this.t = t;
        }

        public SqlCollection<T1> Select<T1>(Func<T, T1> getter) where T1 : ISqlCode => new SqlCollection<T1>(getter(t));
    }

    //public interface ISqlCollection<out T> : ISqlCode
    //    where T : ISqlCode
    //{ 

    //}

    public abstract class SqlCollection : ISqlCode
    {
        public abstract string ToCode();
    }

    public class SqlCollection<T> : SqlCollection
        where T: ISqlCode
    {
        private readonly T inner;

        public SqlCollection(T inner)
        {
            this.inner = inner;
        }

        public override string ToCode() => inner.ToCode();
    }

    //public class SqlInt : SqlValue
    //{
    //    public SqlInt(int v)
    //    {
    //    }
    //}

    public interface ISqlCode {
        string ToCode();
    }

    public interface ISqlValue : ISqlCode { }

    public interface ISqlNumericValue: ISqlValue
    { 
    
    }

    public interface ISqlValue<T> : ISqlValue { 
    
    }

    public class IntSqlConstant : ISqlNumericValue
    {
        private readonly int value;

        public IntSqlConstant(int value)
        {
            this.value = value;
        }

        public string ToCode()=> value.ToString();


        
    }

    //public class SqlValue<T>: ISqlValue<T>
    //{
    //    public SqlValue(T v)
    //    {
    //    }
    //}

    public interface ISqlTable  { }

    public abstract class SqlColumn<T> : ISqlValue<T>
    {
        private readonly ISqlTable table;
        private readonly string columnName;

        public SqlColumn(ISqlTable table, string columnName)
        {
            this.table = table ?? throw new ArgumentNullException(nameof(table));
            this.columnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
        }

        public string ToCode() => $"{table.GetType().Name}.{columnName}";
    }

    public class IntSqlColumn : SqlColumn<int>, ISqlNumericValue
    {
        public IntSqlColumn(ISqlTable table, string columnName) : base(table, columnName)
        {
        }
    }

    public class SqlOrder {
        public readonly ISqlValue inner;

        public SqlOrder(ISqlValue sp)
        {
            inner = sp ?? throw new ArgumentNullException(nameof(sp));
        }
    }
}

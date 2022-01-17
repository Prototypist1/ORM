using System;
using System.Collections.Generic;
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

    public static class PlayGround {

        public class Student : ISqlTable{
            public IntSqlColumn Id => new IntSqlColumn(this, nameof(Id));
        }

        public class ClassSessionStudend: ISqlTable
        {
            public IntSqlColumn SudentId => new IntSqlColumn(this, nameof(SudentId));

            public IntSqlColumn ClassSessionId => new IntSqlColumn(this, nameof(ClassSessionId));
        }
        public static void Test() {
            Query.From<Student>()
                 .Join<ClassSessionStudend>(x => x.Get<Student>().Id.IsEquals(x.Get<ClassSessionStudend>().SudentId))
                 .Where(x => x.Get<Student>().Id.IsEquals( new SqlValue<int>(5)))
                 .GroupBy(x => new { StudentId = x.Get<Student>().Id })
                 .Having(x => x.Get<ClassSessionStudend>().Get(y => y.ClassSessionId).Count().IsGreater(new SqlValue<int>(5)))
                 .OderBy(x => x.Get<ClassSessionStudend>().Get(y => y.ClassSessionId).Count()) // 😰 I can't pull from the grouping here, I could just do something like x.Key
                 .Select(x => new { classesAttended = x.Get<ClassSessionStudend>().Get(y => y.ClassSessionId).Count() });
                 
        }
    }

    public static class Sql {
        // do left and right have to be the same type?
        // they probably should be
        public static EqualOperator IsEquals(this ISqlCode left, ISqlCode right) 
        {
            return new EqualOperator(left, right);
        }

        internal static CountOperator Count(this SqlCollection sqlCollection)
        {
            return new CountOperator(sqlCollection);
        }

        internal static GreaterThanOperator IsGreater(this ISqlNumericValue left, ISqlNumericValue right)
        {
            return new GreaterThanOperator(left, right);
        }
    }

    public class QueryCanExecute<T> { 
    
    }

    public class QueryCanSelect<TTableSet> 
    {
        internal readonly TTableSet tableSet;

        public QueryCanSelect(TTableSet tableSet)
        {
            this.tableSet = tableSet;
        }


        public QueryCanExecute<T> Select<T>(Func<TTableSet, T> select)
        {

        }
    }

    public class QueryCanOrderBy<TTableSet> : QueryCanSelect<TTableSet>
    {
        public QueryCanOrderBy(TTableSet tableSet) : base(tableSet)
        {
        }


        public QueryCanSelect<TTableSet> OderBy(params Func<TTableSet, SqlOrder>[] orderBys)
        {

        }

    }

    public class QueryCanHaving<TTableSet> : QueryCanOrderBy<TTableSet>
    {
        public QueryCanHaving(TTableSet tableSet) : base(tableSet)
        {
        }

        public QueryCanGroupBy<TTableSet> Having(params Func<TTableSet, ISqlValue<bool>>[] orderBys)
        {

        }
    }

    public class QueryCanGroupBy<TTableSet> : QueryCanOrderBy<TTableSet>
    {
        public QueryCanGroupBy(TTableSet tableSet) : base(tableSet)
        {
        }


    }

    public static class QueryCabGroupByExtension {

        public static QueryCanHaving<GroupedTableSet<TKey, T1>> GroupBy<T1, TKey>(this QueryCanGroupBy<TableSet<T1>> target,Func<TableSet<T1>, TKey> condition)
        {
            return new QueryCanHaving<GroupedTableSet<TKey, T1>>(target.tableSet.Group(x => condition(x)));
        }

        public static QueryCanHaving<GroupedTableSet<TKey, T1, T2>> GroupBy<T1, T2, TKey>(this QueryCanGroupBy<TableSet<T1, T2>> target, Func<TableSet<T1, T2>, TKey> condition)
        {
            return new QueryCanHaving<GroupedTableSet<TKey, T1, T2>>(target.tableSet.Group(x => condition(x)));
        }
    }

    public class QueryCanWhere<TTableSet> : QueryCanGroupBy<TTableSet> {
        public QueryCanWhere(TTableSet tableSet) : base(tableSet)
        {
        }

        public QueryCanGroupBy<TTableSet> Where(Func<TTableSet, ISqlValue<bool>> condition)
        {

        }
    }

    public class Query {

        public static Query<T1> From<T1>()
        {

        }
    }

    public class Query<T1> : QueryCanWhere<TableSet<T1>>
    {

        public Query(TableSet<T1> tableSet) : base(tableSet)
        {
        }

        public Query<T1, T2> Join<T2>(Func<TableSet<T1, T2>, ISqlValue<bool>> condition)
        {
            return new Query<T1,T2>(tableSet.Join<T2>(x=>condition(x)));
        }
    }

    public class Query<T1, T2> : QueryCanWhere<TableSet<T1, T2>>
    {
        
        public Query(TableSet<T1, T2> tableSet) : base(tableSet)
        {
        }
    }

    //public class QueryJoining<T1,T2> : Query<TableSet<T1,T2>>, ICanJoin, ICanWhere, ICanGroupBy, ICanHaving, ICanOrderBy, ICanSelect { 

    //}

    //public class QueryPostJoin<TTableSet> : Query<TTableSet>,  ICanWhere, ICanGroupBy, ICanHaving, ICanOrderBy, ICanSelect
    //{
    //}

    //public class PostWhere<TTableSet> : Query<TTableSet>,  ICanGroupBy, ICanHaving, ICanOrderBy, ICanSelect
    //{

    //}

    //public class PostGroupBy<TTableSet> : Query<TTableSet>, ICanHaving, ICanOrderBy, ICanSelect
    //{

    //}

    //public class PostHaving<TTableSet> : Query<TableSet>, ICanOrderBy, ICanSelect
    //{

    //}

    //public class PostOrderBy<TTableSet> : Query<TTableSet>, ICanSelect
    //{

    //}

    // maybe I should be less strict
    // I should allow abritray grouping and filtering in any order
    // it will just generate the nested queries for you?

    //public interface ICanJoin { }
    //public interface ICanWhere { }
    //public interface ICanGroupBy { }
    //public interface ICanHaving { }
    //public interface ICanOrderBy { }
    //public interface ICanSelect { }

    //public static class QueryExtensions {




    //    // but this doesn't work
    //    // how do you use ungrouped columns
    //    //public static PostGroupBy<GroupedTableSet<TResultTable, T>> GroupBy<T, TTableSet, TResultTable>(this T target, Func<TTableSet, TResultTable> condition)
    //    //    where T : Query<TTableSet>, ICanGroupBy
    //    //    where TTableSet : TableSet
    //    //{

    //    //}
    //}

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

    public class TableSet {
        //private readonly IReadOnlyDictionary<Type, object> map;

        //public TableSet(IReadOnlyDictionary<Type, object> map)
        //{
        //    this.map = map;
        //}

        //public bool TryGet<T>(out T res) {
        //    if ( map.TryGetValue(typeof(T), out var innerRes))
        //    {
        //        res = (T)innerRes;
        //        return true;
        //    }
        //    res = default;
        //    return false;
        //}
    }

    public class TableSet<T1> : TableSet, ITableSetContains<T1>
    {
        private readonly T1 t1;

        public TableSet(T1 t1)
        {
            this.t1 = t1;
        }

        internal TableSet<T1, T2> Join<T2>(Func<TableSet<T1, T2>, ISqlValue<bool>> condition) { 
        
        }

        internal GroupedTableSet<TKey, T1> Group<TKey>(Func<TableSet<T1>, TKey> group) { 
        
        }

        T1 ITableSetContains<T1>.Get() => t1;
    }

    public class TableSet<T1, T2> : TableSet<T1> , ITableSetContains<T2>
    {
        private readonly T2 t2;

        public TableSet(T1 t1,T2 t2) : base(t1)
        {
            this.t2 = t2;
        }

        T2 ITableSetContains<T2>.Get() => t2;
    }

    // this is a bit weird columns show up in the key and the collections, you shouldn't access a key that is in the grouping as a collection 
    // can you layer groups? yeah, I think you just need to put a select between each group
    public class GroupedTableSet<TKey, T1>: ITableSetContains<TKey>, ITableSetCollection<T1>
    {
        private readonly TKey tkey;
        private readonly SqlTableCollection<T1> t1;

        TKey ITableSetContains<TKey>.Get() => tkey;
        SqlTableCollection<T1> ITableSetCollection<T1>.Get() => t1;
    }

    public class GroupedTableSet<TKey, T1, T2> : GroupedTableSet<TKey, T1>, ITableSetCollection<T2>
    {

        private readonly SqlTableCollection<T2> t2;
        SqlTableCollection<T2> ITableSetCollection<T2>.Get() => t2;
    }


    //public class SqlBool { }
    public class SqlTableCollection<T> {
        public SqlCollection<T1> Get<T1>(Func<T, T1> getter) { }
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


    public class SqlValue<T>: ISqlValue<T>
    {
        public SqlValue(T v)
        {
        }
    }

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
        private readonly ISqlValue inner;

        public SqlOrder(ISqlValue sp)
        {
            inner = sp ?? throw new ArgumentNullException(nameof(sp));
        }

        public static implicit operator SqlOrder(ISqlValue sp) => new SqlOrder(sp);
    }
}

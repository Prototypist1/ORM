using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ORM
{


    //class Table<T> { 
    
    //}



    public class OrmBool {
        public string text;
    }

    // columns are a bit weird
    // sometimes they are just metadata
    // sometimes the hold results
    //public class ColumnMetadata {


    //}

    public class Column {
        public OrmBool Equal(Column column)
        {
            return new OrmBool
            {
                text = $" {(this as IColumnWithMetadata).row.alias}.{(this as IColumnWithMetadata).Name} = {(column as IColumnWithMetadata).row.alias}.{(column as IColumnWithMetadata).Name}"
            };
        }
        public OrmBool NotEqual(Column column)
        {
            return new OrmBool
            {
                text = $" {(this as IColumnWithMetadata).row.alias}.{(this as IColumnWithMetadata).Name} != {(column as IColumnWithMetadata).row.alias}.{(column as IColumnWithMetadata).Name}"
            };
        }

        public static OrmBool operator ==(Column column1, Column column2)
        {
            return column1.Equal(column2);
        }
        public static OrmBool operator !=(Column column1, Column column2)
        {
            return column1.NotEqual(column2);
        }
    }

    public class Column<T>: Column
    {
        public T value;
    }

    internal interface IColumnWithMetadata {
        Row row { get; set; }
        string Name{ get; set; }
    }

    // this could be in a dict off field data 
    public class ColumnWithMetadata<T>: Column<T>, IColumnWithMetadata
    {
        Row IColumnWithMetadata.row { get; set; }
        string IColumnWithMetadata.Name { get; set; }
    }


    internal class Row {
        public string from;
        public string alias;
        public List<IColumnWithMetadata> columns;

        public static Row<T> Build<T>(T sample, Func<string> aliasSrouce)
        {
            return Build(sample,aliasSrouce, typeof(T).Name);
        }

        public static Row<T> Build<T>(Func<string> aliasSrouce)
        {
            Func<System.Reflection.FieldInfo, string> nameSource = (field) =>field.Name;

            return NewMethod<T>(nameSource, aliasSrouce, typeof(T).Name);
        }

        public static Row<T> Build<T>(T sample ,Func<string> aliasSrouce, string from)
        {
            Func<System.Reflection.FieldInfo, string> nameSource = (field) =>
            {
                var value = (IColumnWithMetadata)field.GetValue(sample);
                return value?.Name ?? field.Name;
            };

            return NewMethod<T>(nameSource, aliasSrouce, from);
        }

        private static Row<T> NewMethod<T>(Func<System.Reflection.FieldInfo, string> nameSource, Func<string> aliasSrouce, string from)
        {
            var res = (Row<T>)Activator.CreateInstance(typeof(Row<T>));
            res.pocoRow = (T)Activator.CreateInstance(typeof(T));

            res.alias = aliasSrouce();
            res.from = from;

            res.columns = new List<IColumnWithMetadata>();
            foreach (var field in res.pocoRow.GetType().GetFields()
                .Where(x => typeof(Column).IsAssignableFrom(x.FieldType)))
            {
                var newColum = (IColumnWithMetadata)Activator.CreateInstance(typeof(ColumnWithMetadata<>).MakeGenericType(field.FieldType.GetGenericArguments().First()));
                
                newColum.Name = nameSource(field); 
                newColum.row = res;
                // we need to box our tuple so the change sticks
                // https://stackoverflow.com/questions/59000557/valuetuple-set-fields-via-reflection
                object obj = (object)res.pocoRow;
                field.SetValue(obj, newColum);
                // and then unbox it
                res.pocoRow = (T)obj;
                res.columns.Add(newColum);
            }
            return res;
        }
    }

    internal class Row<T> : Row{
        public T pocoRow;
    }

    public class SelectResult<T> where T : new()
    {
        public SelectBuilder Statement;
        public T Poco => row.pocoRow;
        internal Row<T> row;

        public IEnumerable<T> Execute(SqlConnection sqlConnection)
        {
            var reader = Statement.Execute(sqlConnection);
            while (reader.Read())
            {
                var res = new T();
                foreach (var field in typeof(T).GetFields()
                    .Where(x => typeof(Column).IsAssignableFrom(x.FieldType)))
                {
                    // we populate the data
                    var newColum = (Column)Activator.CreateInstance(field.FieldType);
                    field.FieldType.GetField(nameof(Column<int>.value)).SetValue(newColum, ToType(field.FieldType.GenericTypeArguments[0], reader[field.Name]));
                    // be we also pull metadata maybe...
                }
                yield return res;
            }
        }

        private object? ToType(Type type, object? v)
        {
            if (type == typeof(int))
            {
                return Convert.ToInt32(v);
            }
            if (type == typeof(double))
            {
                return Convert.ToDouble(v);
            }
            if (type == typeof(decimal))
            {
                return Convert.ToDecimal(v);
            }
            if (type == typeof(short))
            {
                return Convert.ToInt16(v);
            }
            if (type == typeof(long))
            {
                return Convert.ToInt64(v);
            }
            if (type == typeof(int?))
            {
                if (v == DBNull.Value)
                {
                    return (int?)null;
                }
                return Convert.ToInt32(v);
            }
            if (type == typeof(double?))
            {
                if (v == DBNull.Value)
                {
                    return (double?)null;
                }
                return Convert.ToDouble(v);
            }
            if (type == typeof(decimal?))
            {
                if (v == DBNull.Value)
                {
                    return (decimal?)null;
                }
                return Convert.ToDecimal(v);
            }
            if (type == typeof(short?))
            {
                if (v == DBNull.Value)
                {
                    return (short?)null;
                }
                return Convert.ToInt64(v);
            }
            if (type == typeof(long?))
            {
                if (v == DBNull.Value)
                {
                    return (long?)null;
                }
                return Convert.ToInt64(v);
            }
            if (type == typeof(string))
            {
                if (v == DBNull.Value)
                {
                    return (string)null;
                }
                return Convert.ToString(v);
            }

            throw new NotImplementedException();
        }

        // this is backwards of what I would expect
        public From<T,Row2> LeftJoin<Row2>(Func<T, Row2, OrmBool> join) {
            return new From<Row2>()
                        .ReverseLeftJoin(
                            this,
                            join);
        }
    }

    public class From<Row1>
    {
        SelectBuilder statement;
        Row1 row1;

        public From()
        {
            statement = new SelectBuilder();
            var rowRow = Row.Build<Row1>(statement.GetAlias);
            statement.BaseEntity = rowRow;
            row1 = rowRow.pocoRow;
        }


        public static From<Row1, Row2>  Join<Row2>(SelectResult<Row2> row2, Func<Row1, Row2, OrmBool> join) where Row2 : new()
        {
            var self = new From<Row1>();
            return self.LeftJoin(row2,join);
        }

        public static From<Row1, Row2> Join<Row2>(Func<Row1, Row2, OrmBool> join) where Row2 : new()
        {
            var self = new From<Row1>();
            return self.LeftJoin(join);
        }

        public From<Row1, Row2> LeftJoin<Row2>(SelectResult<Row2> row2, Func<Row1, Row2, OrmBool> join) where Row2 : new()
        {
            statement.joins.Add((row2.row, join(row1, row2.row.pocoRow)));
            return new From<Row1, Row2>(statement, row1, row2.row.pocoRow);
        }


        public From<Row2, Row1> ReverseLeftJoin<Row2>(SelectResult<Row2> row2, Func<Row2, Row1, OrmBool> join) where Row2 : new()
        {
            statement.joins.Add((row2.row, join(row2.row.pocoRow, row1)));
            return new From<Row2, Row1>(statement, row2.row.pocoRow, row1);
        }

        public From<Row1, Row2> LeftJoin<Row2>(Func<Row1, Row2, OrmBool> join)
        {
            var other = Row.Build<Row2>(statement.GetAlias);
            statement.joins.Add((other, join(row1, other.pocoRow)));
            return new From<Row1, Row2>(statement, row1, other.pocoRow);
        }

        public SelectResult<T> Select<T>(Func<Row1, T> transfrom) where T : new()
        {
            var obj = transfrom(row1);

            statement.toSelect = obj.GetType().GetFields()
                .Where(x => typeof(Column).IsAssignableFrom(x.FieldType))
                .Select(x => x.GetValue(obj))
                .OfType<Column>()
                .ToArray();

            return new SelectResult<T>
            {
                row = Row.Build<T>(obj,statement.GetAlias, $"({statement.Compile()})"),
                Statement = statement
            };
        }
    }

    public class From<Row1, Row2>{
        private SelectBuilder statement;
        Row1 row1;
        Row2 row2;

        public From(SelectBuilder statement, Row1 row1, Row2 row2)
        {
            this.statement = statement;
            this.row1 = row1;
            this.row2 = row2;
        }

        public SelectResult<T> Select<T>(Func<Row1, Row2, T> transfrom) where T : new()
        {
            var obj = transfrom(row1, row2);

            statement.toSelect = obj.GetType().GetFields()
                .Where(x => typeof(Column).IsAssignableFrom(x.FieldType))
                .Select(x => x.GetValue(obj))
                .OfType<Column>()
                .ToArray();

            return new SelectResult<T> { 
                row = Row.Build<T>(obj, statement.GetAlias),
                Statement = statement
            };
        }

        public From<Row1, Row2> GroupBy(Func<Row1, Row2, OrmBool> transfrom)
        {

            statement.having = transfrom(row1, row2);

            return this;

        }
        public From<Row1, Row2> GroupBy(Func<Row1, Row2, Column[]> transfrom) {

            statement.groupBy = transfrom(row1, row2);

            return this;

        }
    }

    //public class Grouped<T>
    //{
    //    private SelectBuilder statement;
    //    private T res;

    //    public Grouped(T t, SelectBuilder statement)
    //    {
    //        this.statement = statement;
    //        this.res = t;
    //    }


    //    public SelectResult<TRes> Select<TRes>(Func<T, TRes> transfrom) where TRes : new()
    //    {
    //        var obj = transfrom(res);

    //        statement.toSelect = obj.GetType().GetFields()
    //            .Where(x => typeof(Column).IsAssignableFrom(x.FieldType))
    //            .Select(x => x.GetValue(obj))
    //            .OfType<Column>()
    //            .ToArray();

    //        return new SelectResult<TRes>
    //        {
    //            row = Row.Build<TRes>(statement.GetAlias),
    //            Statement = statement
    //        };
    //    }
    //}

    public class SelectBuilder {
        internal Row BaseEntity;
        internal List<(Row row, OrmBool joinStatement)> joins = new List<(Row row, OrmBool joinStatement)>();
        internal Column[] toSelect;
        internal OrmBool where;
        internal Column[] groupBy;
        internal OrmBool having;
        //List<Column> ToOrderBy;
        public string Compile() {
            var res = $"select {String.Join(", ", toSelect.Select(x => (x as IColumnWithMetadata).row.alias + "." + (x as IColumnWithMetadata).Name))} from {BaseEntity.from} {BaseEntity.alias}{Environment.NewLine}";

            foreach (var join in joins)
            {
                res += $"inner join {join.row.from} {join.row.alias} on {join.joinStatement.text}{Environment.NewLine}";
            }

            if (where != null) {
                res += $"where {where.text}{Environment.NewLine}";
            }

            if (groupBy != null) {
                res += $"group by  {String.Join(", ", groupBy.Select(x => (x as IColumnWithMetadata).row.alias + "." + (x as IColumnWithMetadata).Name))}{Environment.NewLine}";
            }

            if (having != null)
            {
                res += $"where {having.text}{Environment.NewLine}";
            }

            return res;

        }

        public SqlDataReader Execute(SqlConnection sqlConnection) => new SqlCommand(Compile(), sqlConnection).ExecuteReader();


        int i = 0;
        List<string> aliases = new List<string> { "a", "b", "c" , "d", "e", "f"};
        public string GetAlias() {
            return aliases[i++];
        }
    }

}

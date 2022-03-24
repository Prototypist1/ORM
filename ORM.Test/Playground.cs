using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ORM.Test
{
    public class Student : ISqlTable
    {
        public IntSqlColumn Id => new IntSqlColumn(this, nameof(Id));
    }

    public class ClassSessionStudend : ISqlTable
    {
        public IntSqlColumn SudentId => new IntSqlColumn(this, nameof(SudentId));
        public IntSqlColumn ClassSessionId => new IntSqlColumn(this, nameof(ClassSessionId));
    }

    public class Test
    {

        [Fact]
        public void Run()
        {
            
            var test = Query.From<Student>()
                 .Join<ClassSessionStudend>(x => x.Get<Student>().Id.IsEquals(x.Get<ClassSessionStudend>().SudentId))
                 .Where((Student student) => student.Id.IsEquals(new IntSqlConstant(5)))
                 .GroupBy(x => new { StudentId = x.Get<Student>().Id })
                 .Having(x => x.Get<ClassSessionStudend>().Select(y => y.ClassSessionId).Count().IsGreater(new IntSqlConstant(5)))
                 .OderBy(x => x.Get<ClassSessionStudend>().Select(y => y.ClassSessionId).Count()) // 😰 I can't pull from the grouping here, I could just do something like x.Key
                 .Select(x => new { classesAttended = x.Get<ClassSessionStudend>().Select(y => y.ClassSessionId).Count() })
                 .ToCode();

            var db = 0;

        }
    }
}

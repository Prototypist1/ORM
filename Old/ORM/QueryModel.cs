using System;
using System.Collections.Generic;
using System.Text;

namespace ORM
{
    class MyBool { }

    class QueryModel<T1,T2, TOut>
    {
        Func<T1, T2, MyBool> join;
        Func<T1, T2, MyBool> where;
        // might be able to analyze those two ^ because myBool can know what happened
        // but I can't analyze:
        Func<T1, T2, TOut> select;
    }
}

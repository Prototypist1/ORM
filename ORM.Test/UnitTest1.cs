using System;
using Xunit;

namespace ORM.Test
{
    public class UnitTest1
    {

        class Customer
        {
            public Column<string> CustomerName;
            public Column<int> CustomerId;
        }

        class Order
        {
            public Column<int> OrderId;
            public Column<int> CustomerId;
        }

        class OrderWigit 
        {

            public Column<int> OrderId;
            public Column<int> WigitId;
        }

        class Wigit
        {
            public Column<int> WigitId;
        }

        [Fact]
        public void Test1()
        {
            var x =
            From<Customer>
            .Join<Order>((c, o) => c.CustomerId == o.CustomerId)
            .Select((c, o) => (c.CustomerName, o.OrderId))
            .Statement.Compile();
        }

        [Fact]
        public void Test2()
        {
            var x =
            new From<Customer>()
            .LeftJoin(
                new From<Order>().Select(o => (o.CustomerId,o.OrderId)),
                (c, o) => c.CustomerId.Equal(o.CustomerId))
            .Select((c, o) => (c.CustomerName, o.OrderId))
            .Statement.Compile();
        }

        [Fact]
        public void Test3()
        {
            var x =
            From<Customer>
            .Join<Order>((c, o) => c.CustomerId == o.CustomerId)
            .GroupBy((c,o)=> new Column[] { c.CustomerName, o.OrderId })
            .Select((c,o) => (c.CustomerName, o.OrderId))
            .Statement.Compile();
        }


        [Fact]
        public void Test4()
        {
            var x =
            new From<Order>()
            .Select(o => (o.CustomerId, o.OrderId))
            .LeftJoin<Customer>((o,c) => c.CustomerId.Equal(o.CustomerId))
            .Select((o, c) => (c.CustomerName, o.OrderId))
            .Statement.Compile();
        }

    }
}

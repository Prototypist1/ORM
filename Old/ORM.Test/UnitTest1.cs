using System;
using System.Collections.Generic;
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
            public Column<int> OrderQuantity;
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

            Assert.Equal(@"
select a.CustomerName, b.OrderId 
from Customer a
inner join Order b on  a.CustomerId = b.CustomerId
", x);
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

            Assert.Equal(@"
select a.CustomerName, b.OrderId 
from Customer a
inner join (
select a.CustomerId, a.OrderId 
from Order a
) b on  a.CustomerId = b.CustomerId
", x);
        }

        [Fact]
        public void Test3()
        {
            var x =
            From<Customer>
            .Join<Order>((c, o) => c.CustomerId == o.CustomerId)
            .GroupedSelect((c,o)=> ( c.CustomerName, orderSum: o.OrderQuantity.Sum()))
            .Having((x)=>x.orderSum > 10)
            //.Select((c,o) => (c.CustomerName, o.OrderId))
            .Statement.Compile();

            

            Assert.Equal(@"
select a.CustomerName, SUM(b.OrderQuantity) 
from Customer a
inner join Order b on  a.CustomerId = b.CustomerId
group by  a.CustomerName
having  SUM(c.OrderQuantity) > 10
", x);
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

            Assert.Equal(@"
select a.CustomerName, b.OrderId 
from Customer a
inner join (
select a.CustomerId, a.OrderId 
from Order a
) b on  a.CustomerId = b.CustomerId
", x);
        }

    }
}

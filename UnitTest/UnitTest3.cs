
using Client.Query;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UnitTest
{
    public class Customer
    {
        public string Name { get; set; }

        public List<ContactPerson> ContactPersons { get; set; }
    }

    public class ContactPerson
    {
        public string Name { get; set; }
    }

    public class Test3
    {
        private List<Customer> testObjects;

        private void InitTestObjects()
        {
            testObjects = new List<Customer>()
            {
                new Customer { Name = "Company1" },
                new Customer { Name = "Company2" }
            };

            var contactPersons = new List<ContactPerson>();
            contactPersons.Add(new ContactPerson { Name = "John" });
            contactPersons.Add(new ContactPerson { Name = "Jane" });

            testObjects[0].ContactPersons = contactPersons;

            testObjects[1].ContactPersons = new List<ContactPerson>();
        }

        [Test]
        public void Test1()
        {
            InitTestObjects();

            IQueryable<Customer> queryableTestObjects = testObjects.AsQueryable();

            var queryString = "[ { \"property\":\"contactPersons.Name\", \"operator\":\"Contains\", \"value\":\"Jane\" } ]";

            var query = QueryHelper.GetQuery(queryString);

            var param1 = Expression.Parameter(typeof(Customer), "o");
            var property1 = Expression.Property(param1, "ContactPersons");

            var listItemType = property1.Type.GetProperty("Item").PropertyType;
            var entityProperty = listItemType.GetProperty("Name");

            var param2 = Expression.Parameter(listItemType, "p");

            var call1 = Expression.Call(
                Expression.Property(param2, entityProperty),
                typeof(String).GetMethod("IndexOf", new Type[] { typeof(String), typeof(StringComparison) }),
                    new Expression[] {
                        Expression.Constant("Jane"),
                        Expression.Constant(StringComparison.OrdinalIgnoreCase)
                    }
            );

            var equality = Expression.GreaterThanOrEqual(call1, Expression.Constant(0));

            
            var anyCall = Expression.Call(
                typeof(Enumerable),
                nameof(Enumerable.Any),
                new Type[] { param2.Type },
                property1,
                Expression.Lambda(equality, param2)
            );

            Expression<Func<Customer, bool>> predicate = Expression.Lambda<Func<Customer, bool>>(anyCall, param1);
            //var predicate = QueryHelper.GenerateWhere<Customer>(query.Query);

            var result = queryableTestObjects.Where(predicate).ToList();

            // The above is the same as this:
            // var result = queryableTestObjects.Where(o => o.ContactPersons.Any(p => p.Name.Contains("Jane"))).ToList();

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Name == "Company1");

            Assert.Pass();
        }

        public void Test2()
        {
            InitTestObjects();

            IQueryable<Customer> queryableTestObjects = testObjects.AsQueryable();

            var queryString = "[ { \"operator\":\"AND\", \"operands\":[ { \"property\":\"Name\", \"operator\":\"Contains\", \"value\":\"Company1\" }, { \"property\":\"contactPersons.Name\", \"operator\":\"Contains\", \"value\":\"Jane\" } ] } ]";

            var query = QueryHelper.GetQuery(queryString);
            var predicate = QueryHelper.GenerateWhere<Customer>(query.Query);

            var result = queryableTestObjects.Where(predicate).ToList();

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Name == "Company1");

            Assert.Pass();
        }
    }
}

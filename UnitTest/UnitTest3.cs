
using Client.Query;
using NUnit.Framework;
using System;
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
        }

        [Test]
        public void Test1()
        {
            InitTestObjects();

            IQueryable<Customer> queryableTestObjects = testObjects.AsQueryable();

            var queryString = "[ { \"property\":\"contactPersons.Name\", \"operator\":\"Contains\", \"value\":\"Jane\" } ]";

            var query = QueryHelper.GetQuery(queryString);

            /*
            var predicate = PredicateBuilder.True<Customer>();

            var entityType = typeof(Customer);

            var entityProperty = entityType.GetProperty("ContactPersons", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            var parameter = Expression.Parameter(, "entity");
            Expression<Func<Customer, bool>> exp = Expression.Lambda<Func<Customer, bool>>(expression, parameter);

            predicate = predicate.And(exp);
            */

            //var predicate = QueryHelper.GenerateWhere<Customer>(query.Query);

            var result = queryableTestObjects.Where(o => o.ContactPersons != null && o.ContactPersons.Any(p => p.Name.Contains("Jane"))).ToList();

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

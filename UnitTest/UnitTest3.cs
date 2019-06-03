
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

        public Address Address { get; set; }

        public List<ContactPerson> ContactPersons { get; set; }
    }

    public class Address
    {
        public string City { get; set; }
    }

    public class ContactPerson
    {
        public string Name { get; set; }
        public RoleGroup  RoleGroup { get; set; }
        public List<PhoneNumber> PhoneNumbers { get; set; }
    }

    public class PhoneNumber
    {
        public string Value { get; set; }
    }

    public class RoleGroup
    {
        public List<RoleValue> Roles { get; set; }
    }

    public class RoleValue
    {
        public string Value { get; set; }
    }

    public class Test3
    {
        private List<Customer> testObjects;

        private void InitTestObjects()
        {
            testObjects = new List<Customer>()
            {
                new Customer { Name = "Company1", Address = new Address() { City = "London" } },
                new Customer { Name = "Company2", Address = new Address() { City = "New York" } }
            };

            var contactPersons = new List<ContactPerson>();
            contactPersons.Add(new ContactPerson
            {
                Name = "John",
                PhoneNumbers = new List<PhoneNumber>() {
                    new PhoneNumber() { Value = "123" },
                    new PhoneNumber() { Value = "456" }
                },
                RoleGroup = new RoleGroup()
                {
                    Roles = new List<RoleValue>() { new RoleValue { Value = "editor" }, new RoleValue { Value = "author" } }
                }
            });
            contactPersons.Add(new ContactPerson
            {
                Name = "Jane",
                PhoneNumbers = new List<PhoneNumber>() {
                    new PhoneNumber() { Value = "999" }
                },
                RoleGroup = new RoleGroup()
                {
                    Roles = new List<RoleValue> { new RoleValue { Value = "admin" }, new RoleValue { Value = "editor" } }
                }
            });

            testObjects[0].ContactPersons = contactPersons;

            testObjects[1].ContactPersons = new List<ContactPerson>();
        }

        [Test]
        // Test one-to-one mapping
        public void Test31()
        {
            InitTestObjects();

            IQueryable<Customer> queryableTestObjects = testObjects.AsQueryable();

            var queryString = "[ { \"property\":\"Address.City\", \"operator\":\"Contains\", \"value\":\"London\" } ]";

            var query = QueryHelper.GetQuery(queryString);

            var param1 = Expression.Parameter(typeof(Customer), "o");
            var property1 = Expression.Property(param1, "Address");

            var call1 = Expression.Call(
                Expression.Property(property1, "City"),
                typeof(String).GetMethod("IndexOf", new Type[] { typeof(String), typeof(StringComparison) }),
                    new Expression[] {
                        Expression.Constant("London"),
                        Expression.Constant(StringComparison.OrdinalIgnoreCase)
                    }
            );

            var condition = Expression.GreaterThanOrEqual(call1, Expression.Constant(0));

            Expression<Func<Customer, bool>> predicate = Expression.Lambda<Func<Customer, bool>>(condition, param1);
            //var predicate = QueryHelper.GenerateWhere<Customer>(query.Query);

            var result = queryableTestObjects.Where(predicate).ToList();

            // The above is the same as this:
            // var result = queryableTestObjects.Where(o => o.Address.City.Contains("London")).ToList();

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Name == "Company1");

            Assert.Pass();
        }

        [Test]
        // Test one-to-one mapping: same as Test31 except uses some preliminary code from QueryHelper
        public void Test31B()
        {
            InitTestObjects();

            IQueryable<Customer> queryableTestObjects = testObjects.AsQueryable();

            var queryString = "[ { \"property\":\"Address.City\", \"operator\":\"Contains\", \"value\":\"London\" } ]";

            var query = QueryHelper.GetQuery(queryString);

            var predicate = QueryHelper.GeneratePropertyFilter<Customer>("Address.City", "London");
            //var predicate = QueryHelper.GenerateWhere<Customer>(query.Query);

            var result = queryableTestObjects.Where(predicate).ToList();

            // The above is the same as this:
            // var result = queryableTestObjects.Where(o => o.Address.City.Contains("London")).ToList();

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Name == "Company1");

            Assert.Pass();
        }

        [Test]
        // Test one-to-many mapping
        public void Test32()
        {
            InitTestObjects();

            IQueryable<Customer> queryableTestObjects = testObjects.AsQueryable();

            var queryString = "[ { \"property\":\"ContactPersons.Name\", \"operator\":\"Contains\", \"value\":\"Jane\" } ]";

            var query = QueryHelper.GetQuery(queryString);

            var param1 = Expression.Parameter(typeof(Customer), "o");
            var property1 = Expression.Property(param1, "ContactPersons");

            var listItemType = property1.Type.GetProperty("Item").PropertyType;
            var property2 = listItemType.GetProperty("Name");

            var param2 = Expression.Parameter(listItemType, "p");

            var call1 = Expression.Call(
                Expression.Property(param2, property2),
                typeof(String).GetMethod("IndexOf", new Type[] { typeof(String), typeof(StringComparison) }),
                    new Expression[] {
                        Expression.Constant("Jane"),
                        Expression.Constant(StringComparison.OrdinalIgnoreCase)
                    }
            );

            var condition = Expression.GreaterThanOrEqual(call1, Expression.Constant(0));

            var anyCall = Expression.Call(
                typeof(Enumerable),
                nameof(Enumerable.Any),
                new Type[] { param2.Type },
                property1,
                Expression.Lambda(condition, param2)
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

        [Test]
        // Test one-to-many mapping: same as Test32 except uses some preliminary code from QueryHelper
        public void Test32B()
        {
            InitTestObjects();

            IQueryable<Customer> queryableTestObjects = testObjects.AsQueryable();

            var queryString = "[ { \"property\":\"ContactPersons.Name\", \"operator\":\"Contains\", \"value\":\"Jane\" } ]";

            var query = QueryHelper.GetQuery(queryString);

            var predicate = QueryHelper.GeneratePropertyFilter<Customer>("ContactPersons.Name", "Jane");
            //var predicate = QueryHelper.GenerateWhere<Customer>(query.Query);

            var result = queryableTestObjects.Where(predicate).ToList();

            // The above is the same as this:
            // var result = queryableTestObjects.Where(o => o.ContactPersons.Any(p => p.Name.Contains("Jane"))).ToList();

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Name == "Company1");

            Assert.Pass();
        }

        [Test]
        // Test one-to-many-to-many mapping
        public void Test33()
        {
            InitTestObjects();

            IQueryable<Customer> queryableTestObjects = testObjects.AsQueryable();

            var queryString = "[ { \"property\":\"ContactPersons.PhoneNumbers.Value\", \"operator\":\"Contains\", \"value\":\"999\" } ]";

            var query = QueryHelper.GetQuery(queryString);

            var param1 = Expression.Parameter(typeof(Customer), "o");
            var property1 = Expression.Property(param1, "ContactPersons");

            var listItemType = property1.Type.GetProperty("Item").PropertyType;
            var property2 = listItemType.GetProperty("PhoneNumbers");

            var param2 = Expression.Parameter(listItemType, "p");
            var property2Exp = Expression.Property(param2, "PhoneNumbers");

            var listItemType2 = property2Exp.Type.GetProperty("Item").PropertyType;
            var property3 = listItemType2.GetProperty("Value");
            var param3 = Expression.Parameter(listItemType2, "n");

            var call1 = Expression.Call(
                Expression.Property(param3, property3),
                typeof(String).GetMethod("IndexOf", new Type[] { typeof(String), typeof(StringComparison) }),
                    new Expression[] {
                        Expression.Constant("999"),
                        Expression.Constant(StringComparison.OrdinalIgnoreCase)
                    }
            );

            var condition = Expression.GreaterThanOrEqual(call1, Expression.Constant(0));

            var anyCall = Expression.Call(
                typeof(Enumerable),
                nameof(Enumerable.Any),
                new Type[] { param3.Type },
                property2Exp,
                Expression.Lambda(condition, param3)
            );

            var anyCall2 = Expression.Call(
                typeof(Enumerable),
                nameof(Enumerable.Any),
                new Type[] { param2.Type },
                property1,
                Expression.Lambda(anyCall, param2)
            );

            Expression<Func<Customer, bool>> predicate = Expression.Lambda<Func<Customer, bool>>(anyCall2, param1);
            //var predicate = QueryHelper.GenerateWhere<Customer>(query.Query);

            var result = queryableTestObjects.Where(predicate).ToList();

            // The above is the same as this:
            //var result = queryableTestObjects.Where(o => o.ContactPersons.Any(p => p.PhoneNumbers.Any(n => n.Value.Contains("999")))).ToList();

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Name == "Company1");

            Assert.Pass();
        }

        [Test]
        // Test one-to-many-to-many mapping: same as Test33 except uses some preliminary code from QueryHelper
        public void Test33B()
        {
            InitTestObjects();

            IQueryable<Customer> queryableTestObjects = testObjects.AsQueryable();

            var queryString = "[ { \"property\":\"ContactPersons.PhoneNumbers.Value\", \"operator\":\"Contains\", \"value\":\"999\" } ]";

            var query = QueryHelper.GetQuery(queryString);

            var predicate = QueryHelper.GeneratePropertyFilter<Customer>("ContactPersons.PhoneNumbers.Value", "999");
            //var predicate = QueryHelper.GenerateWhere<Customer>(query.Query);

            var result = queryableTestObjects.Where(predicate).ToList();

            // The above is the same as this:
            //var result = queryableTestObjects.Where(o => o.ContactPersons.Any(p => p.PhoneNumbers.Any(n => n.Value.Contains("999")))).ToList();

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Name == "Company1");

            Assert.Pass();
        }

        [Test]
        // Test one-to-many-to-one-to-many mapping
        public void Test34()
        {
            InitTestObjects();

            IQueryable<Customer> queryableTestObjects = testObjects.AsQueryable();

            var queryString = "[ { \"property\":\"ContactPersons.RoleGroup.Roles\", \"operator\":\"Contains\", \"value\":\"admin\" } ]";

            var query = QueryHelper.GetQuery(queryString);

            var param1 = Expression.Parameter(typeof(Customer), "o");
            var property1 = Expression.Property(param1, "ContactPersons");
            var listItemType = property1.Type.GetProperty("Item").PropertyType;

            var param2 = Expression.Parameter(listItemType, "p");
            var property2Exp = Expression.Property(param2, "RoleGroup");
            var property2 = listItemType.GetProperty("RoleGroup");

            var property3 = property2.PropertyType.GetProperty("Roles");
            var property3Exp = Expression.Property(property2Exp, "Roles");
            var listItemType3 = property3.PropertyType.GetProperty("Item").PropertyType;

            var param4 = Expression.Parameter(listItemType3, "r");
            var property4 = listItemType3.GetProperty("Value");

            var expProp = Expression.Property(param4, property4);

            var call1 = Expression.Call(
                    expProp,
                    typeof(String).GetMethod("IndexOf", new Type[] { typeof(String), typeof(StringComparison) }),
                        new Expression[] {
                        Expression.Constant("admin"),
                        Expression.Constant(StringComparison.OrdinalIgnoreCase)
                        }
                );

            var condition = Expression.GreaterThanOrEqual(call1, Expression.Constant(0));

            var anyCall = Expression.Call(
                typeof(Enumerable),
                nameof(Enumerable.Any),
                new Type[] { param4.Type },
                property3Exp,
                Expression.Lambda(condition, param4)
            );

            var anyCall2 = Expression.Call(
                typeof(Enumerable),
                nameof(Enumerable.Any),
                new Type[] { param2.Type },
                property1,
                Expression.Lambda(anyCall, param2)
            );

            Expression<Func<Customer, bool>> predicate = Expression.Lambda<Func<Customer, bool>>(anyCall2, param1);
            //var predicate = QueryHelper.GenerateWhere<Customer>(query.Query);

            var result = queryableTestObjects.Where(predicate).ToList();

            // The above is the same as this:
            //var result = queryableTestObjects.Where(o => o.ContactPersons.Any(p => p.RoleGroup.Roles.Any(r => r.Value.Contains("admin")))).ToList();

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Name == "Company1");

            Assert.Pass();
        }

        public void Test4()
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

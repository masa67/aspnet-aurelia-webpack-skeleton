
using Client.Query;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace UnitTest.Navigation.NoEF
{
    public class TCustomer
    {
        public string Name { get; set; }

        public TAddress Address { get; set; }

        public List<TContactPerson> ContactPersons { get; set; }
    }

    public class TAddress
    {
        public string City { get; set; }
    }

    public class TContactPerson
    {
        public string Name { get; set; }
        public TRoleGroup  RoleGroup { get; set; }
        public List<TPhoneNumber> PhoneNumbers { get; set; }
    }

    public class TPhoneNumber
    {
        public string Value { get; set; }
    }

    public class TRoleGroup
    {
        public List<TRole> Roles { get; set; }
    }

    public class TRole
    {
        public string Value { get; set; }
    }

    public class UTNavigationNoEF
    {
        private List<TCustomer> testObjects;

        private void InitTestObjects()
        {
            testObjects = new List<TCustomer>()
            {
                new TCustomer { Name = "Company1", Address = new TAddress() { City = "London" } },
                new TCustomer { Name = "Company2", Address = new TAddress() { City = "New York" } }
            };

            var contactPersons = new List<TContactPerson>();
            contactPersons.Add(new TContactPerson
            {
                Name = "John",
                PhoneNumbers = new List<TPhoneNumber>() {
                    new TPhoneNumber() { Value = "123" },
                    new TPhoneNumber() { Value = "456" }
                },
                RoleGroup = new TRoleGroup()
                {
                    Roles = new List<TRole>() { new TRole { Value = "editor" }, new TRole { Value = "author" } }
                }
            });
            contactPersons.Add(new TContactPerson
            {
                Name = "Jane",
                PhoneNumbers = new List<TPhoneNumber>() {
                    new TPhoneNumber() { Value = "999" }
                },
                RoleGroup = new TRoleGroup()
                {
                    Roles = new List<TRole> { new TRole { Value = "admin" }, new TRole { Value = "editor" } }
                }
            });

            testObjects[0].ContactPersons = contactPersons;

            testObjects[1].ContactPersons = new List<TContactPerson>();
        }

        [Test]
        // Test one-to-one mapping
        public void Test1()
        {
            InitTestObjects();

            IQueryable<TCustomer> queryableTestObjects = testObjects.AsQueryable();

            var queryString = "[ { \"property\":\"Address.City\", \"operator\":\"Contains\", \"value\":\"London\" } ]";

            var query = QueryHelper.GetQuery(queryString);

            var param1 = Expression.Parameter(typeof(TCustomer), "o");
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

            Expression<Func<TCustomer, bool>> predicate = Expression.Lambda<Func<TCustomer, bool>>(condition, param1);
            //var predicate = QueryHelper.GenerateWhere<Customer>(query.Query);

            var result = queryableTestObjects.Where(predicate).ToList();

            // The above is the same as this:
            // var result = queryableTestObjects.Where(o => o.Address.City.Contains("London")).ToList();

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Name == "Company1");

            Assert.Pass();
        }

        [Test]
        // Test one-to-one mapping: same as Test1 except uses some preliminary code from QueryHelper
        public void Test1B()
        {
            InitTestObjects();

            IQueryable<TCustomer> queryableTestObjects = testObjects.AsQueryable();

            var queryString = "[ { \"property\":\"Address.City\", \"operator\":\"Contains\", \"value\":\"London\" } ]";

            var query = QueryHelper.GetQuery(queryString);

            var predicate = QueryHelper.GeneratePropertyFilter<TCustomer>("Address.City", "London");
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
        public void Test2()
        {
            InitTestObjects();

            IQueryable<TCustomer> queryableTestObjects = testObjects.AsQueryable();

            var queryString = "[ { \"property\":\"ContactPersons.Name\", \"operator\":\"Contains\", \"value\":\"Jane\" } ]";

            var query = QueryHelper.GetQuery(queryString);

            var param1 = Expression.Parameter(typeof(TCustomer), "o");
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

            Expression<Func<TCustomer, bool>> predicate = Expression.Lambda<Func<TCustomer, bool>>(anyCall, param1);
            //var predicate = QueryHelper.GenerateWhere<Customer>(query.Query);

            var result = queryableTestObjects.Where(predicate).ToList();

            // The above is the same as this:
            // var result = queryableTestObjects.Where(o => o.ContactPersons.Any(p => p.Name.Contains("Jane"))).ToList();

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Name == "Company1");

            Assert.Pass();
        }

        [Test]
        // Test one-to-many mapping: same as Test2 except uses some preliminary code from QueryHelper
        public void Test2B()
        {
            InitTestObjects();

            IQueryable<TCustomer> queryableTestObjects = testObjects.AsQueryable();

            var queryString = "[ { \"property\":\"ContactPersons.Name\", \"operator\":\"Contains\", \"value\":\"Jane\" } ]";

            var query = QueryHelper.GetQuery(queryString);

            var predicate = QueryHelper.GeneratePropertyFilter<TCustomer>("ContactPersons.Name", "Jane");
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
        public void Test3()
        {
            InitTestObjects();

            IQueryable<TCustomer> queryableTestObjects = testObjects.AsQueryable();

            var queryString = "[ { \"property\":\"ContactPersons.PhoneNumbers.Value\", \"operator\":\"Contains\", \"value\":\"999\" } ]";

            var query = QueryHelper.GetQuery(queryString);

            var param1 = Expression.Parameter(typeof(TCustomer), "o");
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

            Expression<Func<TCustomer, bool>> predicate = Expression.Lambda<Func<TCustomer, bool>>(anyCall2, param1);
            //var predicate = QueryHelper.GenerateWhere<Customer>(query.Query);

            var result = queryableTestObjects.Where(predicate).ToList();

            // The above is the same as this:
            //var result = queryableTestObjects.Where(o => o.ContactPersons.Any(p => p.PhoneNumbers.Any(n => n.Value.Contains("999")))).ToList();

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Name == "Company1");

            Assert.Pass();
        }

        [Test]
        // Test one-to-many-to-many mapping: same as Test3 except uses some preliminary code from QueryHelper
        public void Test3B()
        {
            InitTestObjects();

            IQueryable<TCustomer> queryableTestObjects = testObjects.AsQueryable();

            var queryString = "[ { \"property\":\"ContactPersons.PhoneNumbers.Value\", \"operator\":\"Contains\", \"value\":\"999\" } ]";

            var query = QueryHelper.GetQuery(queryString);

            var predicate = QueryHelper.GeneratePropertyFilter<TCustomer>("ContactPersons.PhoneNumbers.Value", "999");
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
        public void Test4()
        {
            InitTestObjects();

            IQueryable<TCustomer> queryableTestObjects = testObjects.AsQueryable();

            var queryString = "[ { \"property\":\"ContactPersons.RoleGroup.Roles\", \"operator\":\"Contains\", \"value\":\"admin\" } ]";

            var query = QueryHelper.GetQuery(queryString);

            var param1 = Expression.Parameter(typeof(TCustomer), "o");
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

            Expression<Func<TCustomer, bool>> predicate = Expression.Lambda<Func<TCustomer, bool>>(anyCall2, param1);
            //var predicate = QueryHelper.GenerateWhere<Customer>(query.Query);

            var result = queryableTestObjects.Where(predicate).ToList();

            // The above is the same as this:
            //var result = queryableTestObjects.Where(o => o.ContactPersons.Any(p => p.RoleGroup.Roles.Any(r => r.Value.Contains("admin")))).ToList();

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Name == "Company1");

            Assert.Pass();
        }

        [Test]
        // Test one-to-many-to-one-to-many mapping: same as Test4 except property filter generation moved to QueryHelper
        public void Test4B()
        {
            InitTestObjects();

            IQueryable<TCustomer> queryableTestObjects = testObjects.AsQueryable();

            var queryString = "[ { \"property\":\"ContactPersons.RoleGroup.Roles.Value\", \"operator\":\"Contains\", \"value\":\"admin\" } ]";

            var query = QueryHelper.GetQuery(queryString);

            var predicate = QueryHelper.GeneratePropertyFilter<TCustomer>("ContactPersons.RoleGroup.Roles.Value", "admin");
            //var predicate = QueryHelper.GenerateWhere<Customer>(query.Query);

            var result = queryableTestObjects.Where(predicate).ToList();

            // The above is the same as this:
            //var result = queryableTestObjects.Where(o => o.ContactPersons.Any(p => p.RoleGroup.Roles.Any(r => r.Contains("admin")))).ToList();

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Name == "Company1");

            Assert.Pass();
        }

        public void Test4()
        {
            InitTestObjects();

            IQueryable<TCustomer> queryableTestObjects = testObjects.AsQueryable();

            var queryString = "[ { \"operator\":\"AND\", \"operands\":[ { \"property\":\"Name\", \"operator\":\"Contains\", \"value\":\"Company1\" }, { \"property\":\"contactPersons.Name\", \"operator\":\"Contains\", \"value\":\"Jane\" } ] } ]";

            var query = QueryHelper.GetQuery(queryString);
            var predicate = QueryHelper.GenerateWhere<TCustomer>(query.Query);

            var result = queryableTestObjects.Where(predicate).ToList();

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Name == "Company1");

            Assert.Pass();
        }
    }
}

using Client;
using Client.Query;
using Client.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System;

namespace UnitTest.Navigation.EF
{
    public class UTNavigationEF
    {
        private DbContextOptions<CustomerContext> _options;

        private void InitTest()
        {
            _options = new DbContextOptionsBuilder<CustomerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using (var context = new CustomerContext(_options))
            {
                context.Customers.Add(new Customer
                {
                    Id = 1,
                    Name = "Company1",
                    NameExtension = "Company1 extension"
                });

                context.Customers.Add(new Customer
                {
                    Id = 2,
                    Name = "Company2",
                    NameExtension = "Company2 extension"
                });

                context.Customers.Add(new Customer
                {
                    Id = 3,
                    Name = "Company3"
                });

                context.Addresses.Add(new Address
                {
                    Id = 1,
                    City = "London",
                    CustomerId = 1

                });

                context.Addresses.Add(new Address
                {
                    Id = 2,
                    City = "New York",
                    CustomerId = 2
                });

                context.Addresses.Add(new Address
                {
                    Id = 3,
                    City = "Tokyo",
                    CustomerId = 3
                });

                context.ContactPersons.Add(new ContactPerson
                {
                    Id = 1,
                    Name = "John",
                    CustomerId = 1
                });

                context.ContactPersons.Add(new ContactPerson
                {
                    Id = 2,
                    Name = "Jane",
                    CustomerId = 1
                });

                context.PhoneNumbers.Add(new PhoneNumber
                {
                    Id = 1,
                    Value = "123",
                    ContactPersonId = 1
                });

                context.PhoneNumbers.Add(new PhoneNumber
                {
                    Id = 2,
                    Value = "456",
                    ContactPersonId = 1
                });

                context.PhoneNumbers.Add(new PhoneNumber
                {
                    Id = 3,
                    Value = "999",
                    ContactPersonId = 2
                });

                context.RoleGroups.Add(new RoleGroup
                {
                    Id = 1,
                    ContactPersonId = 1
                });

                context.RoleGroups.Add(new RoleGroup
                {
                    Id = 2,
                    ContactPersonId = 2
                });

                context.Roles.Add(new Role
                {
                    Id = 1,
                    Value = "editor",
                    RoleGroupId = 1
                });

                context.Roles.Add(new Role
                {
                    Id = 2,
                    Value = "author",
                    RoleGroupId = 1
                });

                context.Roles.Add(new Role
                {
                    Id = 3,
                    Value = "admin",
                    RoleGroupId = 2
                });

                context.Roles.Add(new Role
                {
                    Id = 4,
                    Value = "editor",
                    RoleGroupId = 2
                });

                context.SaveChanges();
            }
        }

        [Test]
        public void Test1()
        {
            InitTest();

            using (var context = new CustomerContext(_options))
            {
                var predicate = QueryHelper.GenerateNavigationFilter<Customer>(new FieldParameter("Address.City", "London", FieldOperator.Contains));

                var result = context.Customers.Where(predicate).ToList();

                Assert.IsTrue(result.Count == 1);
                Assert.IsTrue(result[0].Name == "Company1");

                Assert.Pass();
            }
        }

        [Test]
        public void Test10()
        {
            InitTest();

            using (var context = new CustomerContext(_options))
            {
                Assert.IsFalse(QueryHelper.IsNavigationProperty<Customer>(context, "Name"));
                Assert.IsTrue(QueryHelper.IsNavigationProperty<Customer>(context, "Address"));

                try
                {
                    QueryHelper.IsNavigationProperty<Customer>(context, "NonExistent");
                }
                catch (Exception ex)
                {
                    Assert.Pass();
                }

                Assert.Fail();
            }
        }
    }
}

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
        private IConfigurationRoot _configuration;
        private DbContextOptions<CustomerContext> _options;

        private void InitTest()
        {
            var settingsDir = Directory.GetCurrentDirectory();
            settingsDir = Path.Combine(settingsDir, @"..\..\..\..\Client");

            var builder = new ConfigurationBuilder()
                .SetBasePath(settingsDir)
                .AddJsonFile("appsettings.json");

            _configuration = builder.Build();
            _options = new DbContextOptionsBuilder<CustomerContext>()
                .UseSqlServer(_configuration.GetConnectionString("Default"))
                .Options;
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

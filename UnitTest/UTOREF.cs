using Client;
using Client.Models;
using Client.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnitTest
{
    class UTOREF
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
                var queryString = "[ { \"operator\":\"OR\", \"operands\":[ { \"property\":\"Name\", \"operator\":\"Contains\", \"value\":\"Company1\" }, { \"property\":\"Name\", \"operator\":\"Contains\", \"value\":\"Company2\" } ] } ]";

                var query = QueryHelper.GetQuery(queryString);
                var predicate = QueryHelper.GenerateWhere<Customer>(query.Query);

                var result = context.Customers.Where(predicate).ToList();

                Assert.IsTrue(result.Count == 2);
                Assert.IsTrue(result[0].Name == "Company1");
                Assert.IsTrue(result[1].Name == "Company2");

                Assert.Pass();
            }
        }
    }
}

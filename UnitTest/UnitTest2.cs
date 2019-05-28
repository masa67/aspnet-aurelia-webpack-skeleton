
using Client.Query;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace UnitTest
{
    public class TestClass
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string PhoneMobile { get; set; }
    }

    public class Test2
    {
        /*
        private generatePredicate()
        {
            var predicate = PredicateBuilder.False<TestClass>();

            predicate.Or(t => t.Phone.Contains("123"));
            predicate.Or(t => t.PhoneMobile.Contains("123"));

            var predicate2 = PredicateBuilder.True<TestClass>();
            predicate.And(predicate);
            predicate.And
        }
        */

        [Test]
        public void Test1()
        {
            var testObjects = new List<TestClass>()
            {
                new TestClass { Name = "John", Phone = "123", PhoneMobile = "999" }, // match
                new TestClass { Name = "Johnny", Phone = "999", PhoneMobile = "123" }, // match
                new TestClass { Name = "John", Phone = "999", PhoneMobile = "999" }, // no match
                new TestClass { Name = "Jane", Phone = "999", PhoneMobile = "123" }, // no match
            };

            IQueryable<TestClass> queryableTestObjects = testObjects.AsQueryable();

            var queryString = "[ { \"operator\":\"AND\", \"operands\":[ { \"property\":\"Name\", \"operator\":\"Contains\", \"value\":\"John\" }, { \"operator\":\"OR\", \"operands\":[ { \"property\":\"Phone\", \"operator\":\"Contains\", \"value\":\"123\" }, { \"property\":\"PhoneMobile\", \"operator\":\"Contains\", \"value\":\"123\" } ] } ] } ]";

            var query = QueryHelper.GetQuery(queryString);
            var predicate = QueryHelper.GenerateWhere<TestClass>(query.Query);

            var result = queryableTestObjects.Where(predicate).ToList();

            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result[0].Name == "John" && result[0].Phone == "123");
            Assert.IsTrue(result[1].Name == "Johnny" && result[1].PhoneMobile == "123");

            Assert.Pass();
        }
    }
}

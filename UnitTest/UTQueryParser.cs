using Client.Query;
using NUnit.Framework;

namespace UnitTest.QueryParser
{
    public class UTQueryParser
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var queryString = "[ { \"property\":\"name\", \"operator\":\"Contains\", \"value\":\"As. Oy\" }, { \"property\":\"phone\", \"operator\":\"Contains\", \"value\":\"123\" } ]";

            var result = QueryHelper.GetQuery(queryString);

            Assert.IsTrue(result.Success);

            Assert.Pass();
        }

        [Test]
        public void Test2()
        {
            var queryString = "[ { \"operator\":\"OR\", \"operands\":[ { \"property\":\"phone\", \"operator\":\"Contains\", \"value\":\"123\" }, { \"property\":\"phoneMobile\", \"operator\":\"Contains\", \"value\":\"123\" } ] } ]";

            var result = QueryHelper.GetQuery(queryString);

            Assert.IsTrue(result.Success);

            Assert.Pass();
        }

        [Test]
        public void Test3()
        {
            var queryString = "[ { \"operator\":\"AND\", \"operands\":[ { \"property\":\"name\", \"operator\":\"Contains\", \"value\":\"Pekka\" }, { \"operator\":\"OR\", \"operands\":[ { \"property\":\"phone\", \"operator\":\"Contains\", \"value\":\"123\" }, { \"property\":\"phoneMobile\", \"operator\":\"Contains\", \"value\":\"123\" } ] } ] } ]";

            var result = QueryHelper.GetQuery(queryString);

            Assert.IsTrue(result.Success);

            Assert.Pass();
        }
    }
}
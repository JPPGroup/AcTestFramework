using System;
using System.Reflection;
using BaseTestLibrary;
using NUnit.Framework;

namespace ExampleTests
{
    public class SampleTestFixture : BaseTest
    {
        public override Guid FixtureGuid { get; } = Guid.NewGuid();
        public override string DrawingFile => @"C:\Test.dwg";
        public override string AssemblyPath => Assembly.GetExecutingAssembly().Location;
        public override string AssemblyType => typeof(SampleTestFixture).FullName;

        [Test]
        public void TestMethod1()
        {
            var initialData = new TestData1
            {
                Value1 = 3,
                Value2 = 2
            };

            var response = RunTest("Method1", initialData);
            Assert.True(response.Result, "Ran in core console");

            var responseData = response.Data as int?;
            Assert.NotNull(responseData, "Should have response data");

            Assert.AreEqual(1, responseData, "value passed");
        }

        public int? Method1(object testData)
        {
            if (!(testData is TestData1 data)) return null;

            return data.Value1 - data.Value2;
        }

        [Test]
        public void TestMethod2()
        {
            var initialData = new TestData2
            {
                Value1 = 5,
                Value2 = 4
            };

            var response = RunTest("Method2", initialData);
            Assert.True(response.Result, "Ran in core console");

            var responseData = response.Data as int?;
            Assert.NotNull(responseData, "Should have response data");

            Assert.AreEqual(9, responseData, "value passed");
        }

        public int? Method2(object testData)
        {
            if (!(testData is TestData2 data)) return null;

            return data.Value1 + data.Value2;
        }

        [Test]
        public void TestMethod3()
        {
            var initialData = new TestData3
            {
                Value1 = 1,
                Value2 = 2,
                Value3 = 3,
                Value4 = 4,
                Value5 = 5
            };

            var response = RunTest("Method3", initialData);
            Assert.True(response.Result, "Ran in core console");

            var responseData = response.Data as TestData3;
            Assert.NotNull(responseData, "Should have response data");

            Assert.AreEqual(10, responseData.Value1, "value 1 passed");
            Assert.AreEqual(20, responseData.Value2, "value 2 passed");
            Assert.AreEqual(30, responseData.Value3, "value 3 passed");
            Assert.AreEqual(40, responseData.Value4, "value 4 passed");
            Assert.AreEqual(50, responseData.Value5, "value 5 passed");
        }

        public TestData3 Method3(object testData)
        {
            if (!(testData is TestData3 data)) return null;

            return new TestData3
            {
                Value1 = data.Value1 * 10,
                Value2 = data.Value2 * 10,
                Value3 = data.Value3 * 10,
                Value4 = data.Value4 * 10,
                Value5 = data.Value5 * 10
            };
        }
    }
}

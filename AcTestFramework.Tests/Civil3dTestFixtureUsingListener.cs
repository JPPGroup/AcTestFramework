using System;
using System.Reflection;
using Jpp.AcTestFramework;
using NUnit.Framework;

namespace AcTestFramework.Tests
{
    [TestFixture]
    public class Civil3dTestFixtureUsingListener : Civil3dTestFixture
    {
        public Civil3dTestFixtureUsingListener()
            : base(new Civil3dFixtureArguments(Assembly.GetExecutingAssembly(),
                typeof(Civil3dTestFixtureUsingListener), "") { ClientTimeout = 240000 })
        {

        }

        [Test]
        public void TestMethod_SubtractTwoValues()
        {
            var values = new[] { 3.0, 2.0 };
            var response = RunTest<double?>(nameof(Method_SubtractTwoValues), values);
            Assert.AreEqual(1, response, "Failed to subtract two values.");
        }

        public static double? Method_SubtractTwoValues(object testData)
        {
            if (!(testData is double[] data)) return null;
            return data[0] - data[1];
        }

        [Test]
        public void TestMethod_AddTwoValues()
        {
            var values = new[] { 5.0, 4.0 };
            var response = RunTest<double>(nameof(Method_AddTwoValues), values);
            Assert.AreEqual(9, response, "Failed to add two values.");
        }

        public static double? Method_AddTwoValues(object testData)
        {
            if (!(testData is double[] data)) return null;
            return data[0] + data[1];
        }

        [Test]
        public void TestMethod_MultiplyFiveValuesByTen()
        {
            var values = new[] { 1, 2, 3, 4, 5 };
            var response = RunTest<TestData1>(nameof(Method_MultiplyFiveValuesByTen), values);

            Assert.AreEqual(10, response.Value1, "Failed to multiply value 1 by 10");
            Assert.AreEqual(20, response.Value2, "Failed to multiply value 2 by 10");
            Assert.AreEqual(30, response.Value3, "Failed to multiply value 3 by 10");
            Assert.AreEqual(40, response.Value4, "Failed to multiply value 4 by 10");
            Assert.AreEqual(50, response.Value5, "Failed to multiply value 5 by 10");
        }

        public static TestData1 Method_MultiplyFiveValuesByTen(int[] data)
        {
            return new TestData1
            {
                Value1 = data[0] * 10,
                Value2 = data[1] * 10,
                Value3 = data[2] * 10,
                Value4 = data[3] * 10,
                Value5 = data[4] * 10
            };
        }

        [Test]
        public void TestMethod_Int()
        {
            int response = RunTest<int>(nameof(Method_Int));
            Assert.AreEqual(2, response, "Failed to return true.");
        }

        public static int Method_Int()
        {
            return 2;
        }


        [Test]
        public void TestMethod_BoolTrue()
        {
            var response = RunTest<bool>(nameof(Method_BoolTrue));
            Assert.True(response, "Failed to return true.");
        }

        public static bool Method_BoolTrue()
        {
            return true;
        }

        [Test]
        public void TestMethod_BoolFalse()
        {
            var response = RunTest<bool>(nameof(Method_BoolFalse));
            Assert.False(response, "Failed to return false.");
        }

        public static bool Method_BoolFalse()
        {
            return false;
        }

        [Test]
        public void TestMethod_ReverseString()
        {
            const string initial = "testing";
            const string expected = "gnitset";

            var response = RunTest<string>(nameof(Method_ReverseString), initial);
            Assert.AreEqual(expected, response, "Failed to reverse string.");
        }

        public static string Method_ReverseString(object testData)
        {
            if (!(testData is string s)) return "";
            var charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}

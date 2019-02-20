using System.Reflection;
using Jpp.AcTestFramework;
using NUnit.Framework;

namespace ExampleTests
{
    public class TestFixtureUsingListenerWithDrawing : BaseNUnitTestFixture
    {
        public TestFixtureUsingListenerWithDrawing() : base(Assembly.GetExecutingAssembly(), typeof(TestFixtureUsingListenerWithDrawing), @"C:\Test.dwg") { }

        [Test]
        public void TestMethod_SomethingToTest()
        {
            //TODO: Need to write some drawing tests....
            Assert.Pass("This should pass.");
        }
    }
}

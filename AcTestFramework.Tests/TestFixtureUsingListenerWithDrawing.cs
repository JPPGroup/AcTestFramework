using System.Reflection;
using Jpp.AcTestFramework;
using NUnit.Framework;

namespace AcTestFramework.Tests
{
    public class TestFixtureUsingListenerWithDrawing : BaseNUnitTestFixture
    {
        private const bool DEBUG = true;
        private const string DRAWING_FILE = "";
        private const string INITIAL_LIBRARY = "";

        public TestFixtureUsingListenerWithDrawing() : base(Assembly.GetExecutingAssembly(), typeof(TestFixtureUsingListenerWithDrawing), DRAWING_FILE, INITIAL_LIBRARY, DEBUG) { }

        [Test]
        public void TestMethod_SomethingToTest()
        {
            //TODO: Need to write some drawing tests....
            Assert.Pass("This should pass.");
        }
    }
}

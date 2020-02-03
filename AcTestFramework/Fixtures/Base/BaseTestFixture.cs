using Jpp.AcTestFramework.Pipe;
using Jpp.AcTestFramework.Serialization;
using NUnit.Framework;
using System;

namespace Jpp.AcTestFramework
{
    public abstract class BaseTestFixture
    {
        private readonly BaseFixtureArguments arguments;
        private readonly FileLogger logger;
        private readonly Client pipeClient;
        private readonly Runner process;

        protected BaseTestFixture(BaseFixtureArguments arguments)
        {
            this.arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));

            var fixtureId = Guid.NewGuid().ToString();
            
            logger = new FileLogger(arguments.Directory, FileLogger.LogType.TestFixture, arguments.IsDebug);
            pipeClient = new Client(fixtureId, arguments.ClientTimeout, logger);
            
            process = new Runner(fixtureId, arguments);
        }

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            logger.Entry("Test fixture setup started");

            Setup();

            process.Run();

            InitiateNamedPipeClient();

            logger.Entry("Test fixture setup completed");
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            logger.Entry("Test fixture tear down started");

            TearDown();

            logger.Entry("Test fixture tear down completed");
        }
        
        public virtual void Setup() { }

        private void InitiateNamedPipeClient()
        {
            var startData = CreateRequestStart();
            var message = new CommandMessage { Command = Commands.Start, Data = startData };

            var started = pipeClient.RunCommand(message);

            if (started == null || !(bool)started) throw new ArgumentException("Failed start command.");
        }

        private RequestStart CreateRequestStart()
        {
            return new RequestStart
            {
                InitialLibrary = arguments.InitialLibrary,
                TestLibrary = arguments.AssemblyPath,
                TestType = arguments.AssemblyType
            };
        }

        private void TearDown()
        {
            try
            {
                var message = new CommandMessage { Command = Commands.Stop };
                pipeClient.RunCommand(message);
            }
            catch (Exception e)
            {
                logger.Exception(e);
            }
            finally
            {
                process.Stop();
            }
        }

        protected T RunTest<T>(string test, object data)
        {
            logger.Entry($"Test {test} started");

            var request = new RequestTest {Name = test, Data = data};
            var message = new CommandMessage { Command = Commands.TestCase, Data = request };

            var response = pipeClient.RunCommand(message) as ResponseTest;

            logger.Entry($"Test {test} completed");

            Assert.NotNull(response, "Null response from test command.");
            Assert.True(response.Result, "Failed result from test command.");
            if (response.Data is T responseData) return responseData;

            Assert.Fail("Invalid response data from test command.");
            return default;
        }

        protected T RunTest<T>(string test)
        {
            return RunTest<T>(test, null);
        }
    }
}

using System;
using System.Threading;
using Jpp.AcTestFramework.Pipe;
using Jpp.AcTestFramework.Serialization;
using NUnit.Framework;

namespace Jpp.AcTestFramework
{
    public abstract class BaseNUnitTestFixture
    {
        public virtual bool ShowCommandWindow { get; } = false;
        public virtual string CoreConsole { get; } = @"C:\Program Files\Autodesk\AutoCAD 2019\accoreconsole.exe";
        public virtual double TearDownWaitSeconds { get; } = 1;
        public virtual int ClientTimeout { get; } = 4000;

        public abstract Guid FixtureGuid { get; }
        public abstract string DrawingFile { get; }
        public abstract bool HasDrawing { get; }
        public abstract string AssemblyPath { get; }
        public abstract string AssemblyType { get; }

        private string _testDrawingFile = "";
        private string _testScriptFile = "";
        private int _coreConsoleProcessId;
        private Client _pipeClient;

        [OneTimeSetUp]
        public void BaseSetup()
        {
            _testDrawingFile = Utilities.CreateDrawingFile(FixtureGuid, DrawingFile);
            _testScriptFile = Utilities.CreateScriptFile(FixtureGuid);

            _coreConsoleProcessId = HasDrawing ? CoreConsoleRunner.Run(CoreConsole, _testDrawingFile, _testScriptFile, 1000, ShowCommandWindow) : CoreConsoleRunner.Run(CoreConsole, _testScriptFile, 1000, ShowCommandWindow);

            _pipeClient = new Client(FixtureGuid.ToString(), ClientTimeout);

            var startData = new RequestStart { Path = AssemblyPath, Type = AssemblyType};
            var message = new CommandMessage { Command = Commands.Start , Data = startData };

            if (!(bool) _pipeClient.RunCommand(message)) throw new ArgumentException("Failed start command.");
        }

        [OneTimeTearDown]
        public void BaseTearDown()
        {
            var message = new CommandMessage {Command = Commands.Stop};
            _pipeClient.RunCommand(message);

            Thread.Sleep(TimeSpan.FromSeconds(TearDownWaitSeconds)); //Wait 1 secs for gracefully shut down before killing...
            if (_coreConsoleProcessId > 0) Utilities.KillProcess(_coreConsoleProcessId);

            CleanUpFile();            
        }

        protected T RunTest<T>(string test, object data)
        {
            var request = new RequestTest {Name = test, Data = data};
            var message = new CommandMessage { Command = Commands.TestCase, Data = request };

            var response = _pipeClient.RunCommand(message) as ResponseTest;
            Assert.NotNull(response, "Null response from test command.");
            Assert.True(response.Result, "Failed result from test command.");

            if (response.Data is T responseData) return responseData;

            Assert.Fail("Invalid response data from test command.");
            return default(T);
        }

        protected T RunTest<T>(string test)
        {
            return RunTest<T>(test, null);
        }

        private void CleanUpFile()
        {
            Utilities.DeleteIfExists(_testDrawingFile);
            Utilities.DeleteIfExists(_testScriptFile);
        }
    }
}

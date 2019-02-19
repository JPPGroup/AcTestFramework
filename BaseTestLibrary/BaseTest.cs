using System;
using System.Threading;
using BaseTestLibrary.Pipe;
using BaseTestLibrary.Serialization;
using NUnit.Framework;

namespace BaseTestLibrary
{
    public abstract class BaseTest
    {      
        public abstract Guid FixtureGuid { get; }
        public abstract string DrawingFile { get; }
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

            _coreConsoleProcessId = CoreConsole.Run(_testDrawingFile, _testScriptFile, 1000, false);

            _pipeClient = new Client(FixtureGuid.ToString());

            var startData = new StartData {Path = AssemblyPath, Type = AssemblyType};
            var message = new CommandMessage { Command = Commands.Start , Data = startData };

            if (!(bool) _pipeClient.RunCommand(message)) throw new ArgumentException("Failed to start server...");
        }

        [OneTimeTearDown]
        public void BaseTearDown()
        {
            var message = new CommandMessage {Command = Commands.Stop};
            _pipeClient.RunCommand(message);

            Thread.Sleep(TimeSpan.FromSeconds(1)); //Wait 1 secs for gracefully shut down before killing...
            if (_coreConsoleProcessId > 0) Utilities.KillProcess(_coreConsoleProcessId);

            CleanUpFile();            
        }

        protected T RunTest<T>(string test, object data)
        {
            var request = new TestRequest {Name = test, Data = data};
            var message = new CommandMessage { Command = Commands.TestCase, Data = request };

            var response = _pipeClient.RunCommand(message) as TestResponse;
            Assert.NotNull(response, "Not null response from core console");
            Assert.True(response.Result, "Result from core console");

            if (response.Data is T responseData) return responseData;

            Assert.Fail("Invalid response data");
            return default(T);
        }

        private void CleanUpFile()
        {
            Utilities.DeleteIfExists(_testDrawingFile);
            Utilities.DeleteIfExists(_testScriptFile);
        }
    }
}

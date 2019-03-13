using System;
using System.IO;
using System.Reflection;
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
        
        public string FixtureId { get; }
        public string DrawingFile { get; }
        public bool HasDrawing { get; }
        public string AssemblyPath { get; }
        public string AssemblyType { get; }
        public string InitialLibrary { get; }

        private string _testDrawingFile = "";
        private string _testScriptFile = "";
        private int _coreConsoleProcessId;
        private Client _pipeClient;

        protected BaseNUnitTestFixture(Assembly fixtureAssembly, Type fixtureType, string initialLibrary = "")
        {
            FixtureId = Guid.NewGuid().ToString();
            AssemblyPath = fixtureAssembly.Location;
            AssemblyType = fixtureType.FullName;
            DrawingFile = "";            
            HasDrawing = false;

            if (string.IsNullOrEmpty(initialLibrary)) return;

            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            InitialLibrary = currentDir != null ? Path.Combine(currentDir, initialLibrary) : "";
        }

        protected BaseNUnitTestFixture(Assembly fixtureAssembly, Type fixtureType, string drawingFile, string initialLibrary = "")
        {
            FixtureId = Guid.NewGuid().ToString();
            AssemblyPath = fixtureAssembly.Location;
            AssemblyType = fixtureType.FullName;
            DrawingFile = drawingFile;
            InitialLibrary = initialLibrary;
            HasDrawing = true;

            if (string.IsNullOrEmpty(initialLibrary)) return;

            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            InitialLibrary = currentDir != null ? Path.Combine(currentDir, initialLibrary) : "";
        }

        [OneTimeSetUp]
        public void BaseSetup()
        {
            _testDrawingFile = Utilities.CreateDrawingFile(FixtureId, DrawingFile);
            _testScriptFile = Utilities.CreateScriptFile(FixtureId);

            Setup();

            _coreConsoleProcessId = HasDrawing 
                ? CoreConsoleRunner.Run(CoreConsole, _testDrawingFile, _testScriptFile, 1000, ShowCommandWindow) 
                : CoreConsoleRunner.Run(CoreConsole, _testScriptFile, 1000, ShowCommandWindow);

            _pipeClient = new Client(FixtureId, ClientTimeout);
            
            var startData = new RequestStart { InitialLibrary = InitialLibrary, TestLibrary = AssemblyPath, TestType = AssemblyType};
            var message = new CommandMessage { Command = Commands.Start , Data = startData };

            if (!(bool) _pipeClient.RunCommand(message)) throw new ArgumentException("Failed start command.");
        }

        public virtual void Setup() { }

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

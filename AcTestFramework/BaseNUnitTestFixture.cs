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

        private readonly bool _isDebug;
        private readonly FileLogger _logger;

        protected BaseNUnitTestFixture(Assembly fixtureAssembly, Type fixtureType, bool isDebug = false, string initialLibrary = "")
        {
            FixtureId = Guid.NewGuid().ToString();
            AssemblyPath = fixtureAssembly.Location;
            AssemblyType = fixtureType.FullName;
            DrawingFile = "";            
            HasDrawing = false;
            _isDebug = isDebug;
          
            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            InitialLibrary = string.IsNullOrEmpty(initialLibrary) ? "" : Path.Combine(currentDir ?? throw new InvalidOperationException(), initialLibrary);

            _logger = new FileLogger(currentDir, FileLogger.LogType.Client, _isDebug);
        }

        protected BaseNUnitTestFixture(Assembly fixtureAssembly, Type fixtureType, string drawingFile, bool isDebug = false, string initialLibrary = "")
        {
            FixtureId = Guid.NewGuid().ToString();
            AssemblyPath = fixtureAssembly.Location;
            AssemblyType = fixtureType.FullName;
            DrawingFile = drawingFile;
            HasDrawing = true;
            _isDebug = isDebug;

            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            InitialLibrary = string.IsNullOrEmpty(initialLibrary) ? "" : Path.Combine(currentDir ?? throw new InvalidOperationException(), initialLibrary);

            _logger = new FileLogger(currentDir, FileLogger.LogType.Client, _isDebug);
        }

        [OneTimeSetUp]
        public void BaseSetup()
        {   
            _logger.Entry("Base setup started");
            _testDrawingFile = Utilities.CreateDrawingFile(FixtureId, DrawingFile);
            _testScriptFile = Utilities.CreateScriptFile(FixtureId, _isDebug);

            Setup();

            _coreConsoleProcessId = HasDrawing 
                ? CoreConsoleRunner.Run(CoreConsole, _testDrawingFile, _testScriptFile, 1000, ShowCommandWindow) 
                : CoreConsoleRunner.Run(CoreConsole, _testScriptFile, 1000, ShowCommandWindow);

            _pipeClient = new Client(FixtureId, ClientTimeout, _logger);
            
            var startData = new RequestStart { InitialLibrary = InitialLibrary, TestLibrary = AssemblyPath, TestType = AssemblyType};
            var message = new CommandMessage { Command = Commands.Start , Data = startData };

            if (!(bool) _pipeClient.RunCommand(message)) throw new ArgumentException("Failed start command.");

            _logger.Entry("Base setup completed");
        }

        public virtual void Setup() { }

        [OneTimeTearDown]
        public void BaseTearDown()
        {
            _logger.Entry("Base tear down started");

            try
            {
                var message = new CommandMessage { Command = Commands.Stop };
                _pipeClient.RunCommand(message);
            }
            catch (Exception e)
            {
                _logger.Exception(e);
            }
            finally
            {
                Thread.Sleep(TimeSpan.FromSeconds(TearDownWaitSeconds)); //Wait 1 secs for gracefully shut down before killing...
                if (_coreConsoleProcessId > 0) Utilities.KillProcess(_coreConsoleProcessId);

                CleanUpFile();

                _logger.Entry("Base tear down completed");
            }            
        }

        protected T RunTest<T>(string test, object data)
        {
            _logger.Entry($"Test {test} started");

            var request = new RequestTest {Name = test, Data = data};
            var message = new CommandMessage { Command = Commands.TestCase, Data = request };

            var response = _pipeClient.RunCommand(message) as ResponseTest;

            _logger.Entry($"Test {test} completed");

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

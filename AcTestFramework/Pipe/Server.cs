using System;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Jpp.AcTestFramework.Serialization;

namespace Jpp.AcTestFramework.Pipe
{
    public sealed class Server
    {
        private string _pipeName;
        private NamedPipeServerStream _pipeServer;
        private Assembly _assembly;
        private Type _type;
        private object _testLibObj;
        private readonly FileLogger _logger;

        public Server(bool isDebug)
        {
            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _logger = new FileLogger(currentDir, FileLogger.LogType.Server, isDebug);
        }

        public void Listen(string pipeName)
        {
            _logger.Entry("Listen started");
            try
            {
                _pipeName = pipeName;
                _pipeServer = new NamedPipeServerStream(pipeName);

                BeginWaitingForConnection();
            }
            catch (Exception e)
            {
                _logger.Exception(e);
            }
        }

        private void BeginWaitingForConnection()
        {
            _logger.Entry("Wait for connection started");

            if (_pipeServer == null) return;

            _pipeServer.WaitForConnection();

            _logger.Entry("Connection received");

            IFormatter readFormatter = new BinaryFormatter { Binder = new DeserializationBinder() };
            IFormatter writeFormatter = new BinaryFormatter();

            var message = (CommandMessage)readFormatter.Deserialize(_pipeServer);
            object response;

            _logger.Entry($"{message.Command} command received");

            switch (message.Command)
            {
                case Commands.Start:
                    response = DoStartUp(message.Data);
                    break;
                case Commands.Stop:
                    response = true;
                    break;
                case Commands.TestCase:
                    response = DoTest(message.Data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            message.Data = response;
            writeFormatter.Serialize(_pipeServer, message);

            _logger.Entry("Response sent");

            _pipeServer.Close();
            _pipeServer = null;

            if (message.Command == Commands.Stop) return;

            _pipeServer = new NamedPipeServerStream(_pipeName);

            BeginWaitingForConnection();
        }

        private object DoStartUp(object msgData)
        {
            try
            {
                if (!(msgData is RequestStart data)) return false;

                if (!string.IsNullOrEmpty(data.InitialLibrary)) Assembly.LoadFrom(data.InitialLibrary);

                _assembly = Assembly.LoadFrom(data.TestLibrary);
                _type = _assembly.GetType(data.TestType);
                _testLibObj = Activator.CreateInstance(_type);

                return true;
            }
            catch (Exception e)
            {
                _logger.Exception(e);
                return false;
            }
        }

        private ResponseTest DoTest(object msgData)
        {
            try
            {
                if (!(msgData is RequestTest testReq)) return new ResponseTest { Result = false };

                var methodInfo = _type.GetMethod(testReq.Name);
                if (methodInfo == null) return new ResponseTest { Result = false };

                var parameters = testReq.Data == null ? null : new[] { testReq.Data };
                var result = methodInfo.Invoke(_testLibObj, parameters);

                return new ResponseTest { Result = true, Data = result };
            }
            catch (Exception e)
            {
                _logger.Exception(e);
                return new ResponseTest { Result = false };
            }          
        }
    }
}

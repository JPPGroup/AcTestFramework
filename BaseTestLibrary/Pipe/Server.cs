using System;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using BaseTestLibrary.Serialization;

namespace BaseTestLibrary.Pipe
{
    internal sealed class Server
    {
        private string _pipeName;
        private NamedPipeServerStream _pipeServer;
        private Assembly _assembly;
        private Type _type;
        private object _testLibObj;

        public void Listen(string pipeName)
        {
            try
            {
                _pipeName = pipeName;
                _pipeServer = new NamedPipeServerStream(pipeName);

                BeginWaitingForConnection();
            }
            catch
            {
                // ignored
            }
        }

        private void BeginWaitingForConnection()
        {
            if (_pipeServer == null) return;

            _pipeServer.WaitForConnection();

            IFormatter readFormatter = new BinaryFormatter { Binder = new DeserializationBinder() };
            IFormatter writeFormatter = new BinaryFormatter();

            var message = (CommandMessage)readFormatter.Deserialize(_pipeServer);
            object response;
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
                if (!(msgData is StartData data)) return false;

                _assembly = Assembly.LoadFrom(data.Path);
                _type = _assembly.GetType(data.Type);
                _testLibObj = Activator.CreateInstance(_type);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private TestResponse DoTest(object msgData)
        {
            if (!(msgData is TestRequest testReq)) return new TestResponse {Result = false};

            var methodInfo = _type.GetMethod(testReq.Name, new[] { typeof(object) });
            if (methodInfo == null) return new TestResponse { Result = false };
            
            var parameters = new[] { testReq.Data };
            var result = methodInfo.Invoke(_testLibObj, parameters);

            return new TestResponse { Result = true, Data = result};
        }
    }
}

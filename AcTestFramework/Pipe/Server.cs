using System;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using AcTestFramework.Serialization;

namespace AcTestFramework.Pipe
{
    public sealed class Server
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
                if (!(msgData is RequestStart data)) return false;

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
            catch (Exception)
            {
                return new ResponseTest {Result = false};
            }          
        }
    }
}

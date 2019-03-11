using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Jpp.AcTestFramework.Serialization;

namespace Jpp.AcTestFramework.Pipe
{
    public sealed class Client
    {
        private readonly string _pipeName;
        private readonly int _timeout;

        public Client(string pipeName, int timeout)
        {
            _pipeName = pipeName;
            _timeout = timeout;
        }

        public object RunCommand(CommandMessage message)
        {
            CommandMessage response;

            using (var pipeClient = new NamedPipeClientStream(_pipeName))
            {
                pipeClient.Connect(_timeout);                        
           
                IFormatter writeFormatter = new BinaryFormatter();
                IFormatter readFormatter = new BinaryFormatter { Binder = new DeserializationBinder() };

                writeFormatter.Serialize(pipeClient, message);

                response = (CommandMessage)readFormatter.Deserialize(pipeClient);
            }
           
            return response.Data;
        }
    }
}

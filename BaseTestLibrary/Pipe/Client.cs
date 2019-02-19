using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using BaseTestLibrary.Serialization;

namespace BaseTestLibrary.Pipe
{
    public sealed class Client
    {
        private readonly string _pipeName;

        public Client(string pipeName)
        {
            _pipeName = pipeName;
        }

        public object RunCommand(CommandMessage message)
        {
            CommandMessage response;

            using (var pipeClient = new NamedPipeClientStream(_pipeName))
            {
                pipeClient.Connect(4000);

                IFormatter writeFormatter = new BinaryFormatter();
                IFormatter readFormatter = new BinaryFormatter { Binder = new DeserializationBinder() };

                writeFormatter.Serialize(pipeClient, message);

                response = (CommandMessage)readFormatter.Deserialize(pipeClient);
            }
           
            return response.Data;
        }
    }
}

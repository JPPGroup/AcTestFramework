using System;
using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Jpp.AcTestFramework.Serialization;

namespace Jpp.AcTestFramework.Pipe
{
    public sealed class Client
    {
        private const int RETRY = 3;
        private readonly string _pipeName;
        private readonly int _timeout;
        private readonly FileLogger _logger;

        public Client(string pipeName, int timeout, FileLogger logger)
        {
            _pipeName = pipeName;
            _timeout = timeout;
            _logger = logger;
        }

        public object RunCommand(CommandMessage message)
        {
            _logger.Entry($"Command {message.Command} started");

            CommandMessage response;

            using (var pipeClient = new NamedPipeClientStream(_pipeName))
            {
                var i = 0;
                while (true)
                {
                    try
                    {
                        pipeClient.Connect(_timeout);
                        break;
                    }
                    catch (Exception e)
                    {
                        i++;

                        if (i <= RETRY)
                        {
                            _logger.Entry($"Retrying to connect: attempt {i}");
                            continue;
                        }
                        _logger.Exception(e);
                        return null;
                    }                   
                }
                         
                IFormatter writeFormatter = new BinaryFormatter();
                IFormatter readFormatter = new BinaryFormatter { Binder = new DeserializationBinder() };

                writeFormatter.Serialize(pipeClient, message);

                response = (CommandMessage)readFormatter.Deserialize(pipeClient);
            }

            _logger.Entry($"Command {message.Command} completed");

            return response.Data;
        }
    }
}

﻿using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Jpp.AcTestFramework.Serialization;

namespace Jpp.AcTestFramework.Pipe
{
    public sealed class Client
    {
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
                pipeClient.Connect(_timeout);                        
           
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

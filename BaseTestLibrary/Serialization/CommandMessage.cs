using System;

namespace BaseTestLibrary.Serialization
{
    [Serializable]
    public class CommandMessage
    {
        public Commands Command { get; set; }
        public object Data { get; set; }
    }

    public enum Commands
    {
        Start = 0,
        Stop = 1,
        TestCase = 2
    }
}

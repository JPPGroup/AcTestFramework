using System;

namespace Jpp.AcTestFramework.Serialization
{
    [Serializable]
    public sealed class RequestTest
    {
        public string Name { get; set; }
        public object Data { get; set; }
    }
}

using System;

namespace Jpp.AcTestFramework.Serialization
{
    [Serializable]
    public sealed class ResponseTest
    {
        public bool Result { get; set; }
        public object Data { get; set; }
    }
}

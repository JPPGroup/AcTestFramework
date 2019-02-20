using System;

namespace AcTestFramework.Serialization
{
    [Serializable]
    public sealed class ResponseTest
    {
        public bool Result { get; set; }
        public object Data { get; set; }
    }
}

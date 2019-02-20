using System;

namespace AcTestFramework.Serialization
{
    [Serializable]
    public sealed class RequestStart
    {
        public string Path { get; set; }
        public string Type { get; set; }
    }
}

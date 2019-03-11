using System;

namespace Jpp.AcTestFramework.Serialization
{
    [Serializable]
    public sealed class RequestStart
    {
        public string InitialLibrary { get; set; }
        public string TestLibrary { get; set; }
        public string TestType { get; set; }
    }
}

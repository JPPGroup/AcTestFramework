using System;

namespace BaseTestLibrary.Serialization
{
    [Serializable]
    public class TestRequest
    {
        public string Name { get; set; }
        public object Data { get; set; }
    }
}

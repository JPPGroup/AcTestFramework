using System;

namespace BaseTestLibrary.Serialization
{
    [Serializable]
    public class TestResponse
    {
        public bool Result { get; set; }
        public object Data { get; set; }
    }
}

using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Jpp.AcTestFramework.Serialization
{
    internal sealed class DeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            var currentAssemblyInfo = Assembly.GetExecutingAssembly().FullName;
            var currentAssemblyName = currentAssemblyInfo.Split(',')[0];

            if (assemblyName.StartsWith(currentAssemblyName)) assemblyName = currentAssemblyInfo;

            var typeToDeserialize = Type.GetType($"{typeName}, {assemblyName}");
            return typeToDeserialize;
        }
    }
}
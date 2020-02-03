using System;
using System.Reflection;

namespace Jpp.AcTestFramework
{
    public class Civil3dFixtureArguments : AutoCadFixtureArguments
    {
        public Civil3dFixtureArguments(Assembly fixtureAssembly, Type fixtureType, string initialLibrary) : base(fixtureAssembly, fixtureType, initialLibrary)
        {
            ClientTimeout = 40000;
        }

        public override AppTypes AppType { get; } = AppTypes.Civil3d;
    }
}

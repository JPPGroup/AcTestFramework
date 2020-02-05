using System;
using System.Reflection;

namespace Jpp.AcTestFramework
{
    public class Civil3dFixtureArguments : AutoCadFixtureArguments
    {
        public Civil3dFixtureArguments(Assembly fixtureAssembly, Type fixtureType, string initialLibrary) : base(fixtureAssembly, fixtureType, initialLibrary)
        {
        }

        public override AppTypes AppType { get; } = AppTypes.Civil3d;
        public override int ClientTimeout { get; set; } = 8000;
    }
}

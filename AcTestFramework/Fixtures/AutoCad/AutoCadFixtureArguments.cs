using System;
using System.Reflection;

namespace Jpp.AcTestFramework
{
    public class AutoCadFixtureArguments : BaseFixtureArguments
    {
        public AutoCadFixtureArguments(Assembly fixtureAssembly, Type fixtureType, string initialLibrary) : base(fixtureAssembly, fixtureType, initialLibrary)
        {
            ApplicationPath = $"{GetPath()}\\{ACAD_EXE}";
        }

        public override string ApplicationPath { get; }
        public override AppTypes AppType { get; } = AppTypes.Full;
        public override int WaitBeforeKillSeconds { get; } = 5;
        public override int ClientTimeout { get; set; } = 8000;
    }
}

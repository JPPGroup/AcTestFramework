using System;
using System.Reflection;

namespace Jpp.AcTestFramework
{
    public class CoreConsoleFixtureArguments : BaseFixtureArguments
    {
        public CoreConsoleFixtureArguments(Assembly fixtureAssembly, Type fixtureType, string initialLibrary) : base(fixtureAssembly, fixtureType, initialLibrary)
        {
            ApplicationPath = $"{GetPath()}\\{CORE_EXE}";
        }

        public override string ApplicationPath { get; }
        public override AppTypes AppType { get; } = AppTypes.CoreConsole;
    }
}

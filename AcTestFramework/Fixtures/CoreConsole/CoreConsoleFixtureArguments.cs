using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jpp.AcTestFramework
{
    public class CoreConsoleFixtureArguments : BaseFixtureArguments
    {
        public CoreConsoleFixtureArguments(Assembly fixtureAssembly, Type fixtureType, string initialLibrary) : base(fixtureAssembly, fixtureType, initialLibrary)
        {
        }

        public override string ApplicationPath { get; } = CORE_PATH;
        public override AppTypes AppType { get; } = AppTypes.CoreConsole;
    }
}

﻿using System;
using System.Reflection;

namespace Jpp.AcTestFramework
{
    public class AutoCadFixtureArguments : BaseFixtureArguments
    {
        public AutoCadFixtureArguments(Assembly fixtureAssembly, Type fixtureType, string initialLibrary) : base(fixtureAssembly, fixtureType, initialLibrary)
        {
            ClientTimeout = 8000;
        }

        public override string ApplicationPath { get; } = ACAD_PATH;
        public override AppTypes AppType { get; } = AppTypes.Full;
        public override int WaitBeforeKillSeconds { get; } = 5;
    }
}

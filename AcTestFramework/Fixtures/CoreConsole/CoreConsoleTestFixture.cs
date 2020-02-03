using NUnit.Framework;
using System;
using System.Reflection;

namespace Jpp.AcTestFramework
{
    public abstract class CoreConsoleTestFixture : BaseTestFixture
    {
        protected CoreConsoleTestFixture(CoreConsoleFixtureArguments arguments) : base(arguments)
        {
        }
    }
}

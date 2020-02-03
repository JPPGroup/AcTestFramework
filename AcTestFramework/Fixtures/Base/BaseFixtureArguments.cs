using System;
using System.IO;
using System.Reflection;

namespace Jpp.AcTestFramework
{
    public enum AppTypes { CoreConsole, Full, Civil3d }

    public abstract class BaseFixtureArguments
    {
        protected const string ACAD_PATH = @"C:\Program Files\Autodesk\AutoCAD 2019\acad.exe";
        protected const string CORE_PATH = @"C:\Program Files\Autodesk\AutoCAD 2019\accoreconsole.exe";

        public Assembly FixtureAssembly { get; }
        public Type FixtureType { get; }
        public string InitialLibrary { get; }

        public string DrawingFile { get; set; } = "";
        public bool IsDebug { get; set; } = true;
        public int ClientTimeout { get; set; } = 4000;

        public string AssemblyPath => FixtureAssembly.Location;
        public string AssemblyType => FixtureType.FullName;
        public string Directory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public string ListenLocation => Assembly.GetExecutingAssembly().Location;

        public abstract string ApplicationPath { get;}
        public abstract AppTypes AppType { get; }
        public virtual int WaitForExitMilliseconds { get; } = 1000;
        public virtual int WaitBeforeKillSeconds { get; } = 1;
        public virtual bool DisplayWindow { get; } = false;
        
        protected BaseFixtureArguments(Assembly fixtureAssembly, Type fixtureType, string initialLibrary)
        {
            FixtureAssembly = fixtureAssembly ?? throw new ArgumentNullException(nameof(fixtureAssembly));
            FixtureType = fixtureType ?? throw new ArgumentNullException(nameof(fixtureType));

            InitialLibrary = string.IsNullOrEmpty(initialLibrary) 
                ? "" 
                : Path.Combine(Directory ?? throw new ArgumentNullException(nameof(Directory)), initialLibrary);
        }
    }
}

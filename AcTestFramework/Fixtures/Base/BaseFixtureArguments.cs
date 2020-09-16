using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security;
using Microsoft.Win32;

namespace Jpp.AcTestFramework
{
    public enum AppTypes { CoreConsole, Full, Civil3d }

    public abstract class BaseFixtureArguments
    {
        protected const string ACAD_EXE = @"acad.exe";
        protected const string CORE_EXE = @"accoreconsole.exe";

#if Ac2019
        protected const string RELEASE = "R23.0";
        protected const string VERSION_ID = "ACAD-2001";
        protected const string CIV_VERSION_ID = "ACAD-2000";
#endif

#if Ac2020
        protected const string RELEASE = "R23.1";
        protected const string VERSION_ID = "ACAD-3001";
        protected const string CIV_VERSION_ID = "ACAD-3000";
#endif

#if Ac2021
        protected const string RELEASE = "R24.0";
        protected const string VERSION_ID = "ACAD-4101";
        protected const string CIV_VERSION_ID = "ACAD-4100";
#endif

        public Assembly FixtureAssembly { get; }
        public Type FixtureType { get; }
        public string InitialLibrary { get; }

        public string DrawingFile { get; set; } = "";
        public bool IsDebug { get; set; } = true;
        
        public string AssemblyPath => FixtureAssembly.Location;
        public string AssemblyType => FixtureType.FullName;
        public string Directory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public string ListenLocation => Assembly.GetExecutingAssembly().Location;

        public abstract string ApplicationPath { get;}
        public abstract AppTypes AppType { get; }

        public virtual int WaitForExitMilliseconds { get; } = 1000;
        public virtual int WaitBeforeKillSeconds { get; } = 1;
        public virtual bool DisplayWindow { get; } = false;
        public virtual int ClientTimeout { get; set; } = 4000;

        protected BaseFixtureArguments(Assembly fixtureAssembly, Type fixtureType, string initialLibrary)
        {
            FixtureAssembly = fixtureAssembly ?? throw new ArgumentNullException(nameof(fixtureAssembly));
            FixtureType = fixtureType ?? throw new ArgumentNullException(nameof(fixtureType));

            InitialLibrary = string.IsNullOrEmpty(initialLibrary) 
                ? "" 
                : Path.Combine(Directory ?? throw new ArgumentNullException(nameof(Directory)), initialLibrary);
        }

        protected string GetPath()
        {
            string keyPath = $"SOFTWARE\\Autodesk\\AutoCAD\\{RELEASE}\\{VERSION_ID}:409";
            string civilKeyPath = $"SOFTWARE\\Autodesk\\AutoCAD\\{RELEASE}\\{CIV_VERSION_ID}:409";

            try
            {
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (RegistryKey rk = hklm.OpenSubKey(keyPath))
                {
                    if (rk != null)
                    {
                        string result = (string) rk.GetValue("AcadLocation", null);

                        if (!string.IsNullOrEmpty(result))
                            return result;
                    }
                }
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (RegistryKey rk = hklm.OpenSubKey(civilKeyPath))
                {
                    if (rk == null)
                        throw new NullReferenceException($"Registry key {civilKeyPath} not found");

                    string result = (string)rk.GetValue("AcadLocation", null);

                    if (!string.IsNullOrEmpty(result))
                        return result;

                    throw new NullReferenceException($"Key AcadLocation not found");
                }
            }
            catch (SecurityException e)
            {
                Console.WriteLine("Unable to access registry.");
                Console.WriteLine(e);
                throw;
            }
        }
    }
}

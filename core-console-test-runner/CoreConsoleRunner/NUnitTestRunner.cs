using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnitLite;
using Autodesk.AutoCAD.Runtime;
using CoreConsoleRunner;

[assembly: CommandClass(typeof(NUnitTestRunner))]

namespace CoreConsoleRunner
{
    public class NUnitTestRunner
    {
        /// <summary> This command runs the NUnit tests in this assembly. </summary>
        [CommandMethod("RunTests", CommandFlags.Session)]
        public void RunTests()
        {
            var files = TestAssembliesFromCommandLineArguments();

            if (files == null) return;

            foreach (var file in files)
            {
                var assemblyDir = Path.GetDirectoryName(file);
                var testAssembly = Assembly.LoadFrom(file);
                if (assemblyDir == null) continue;

                var testReportDir = Path.Combine(assemblyDir, @"Report-NUnit");
                if (!Directory.Exists(testReportDir)) Directory.CreateDirectory(testReportDir);

                var fileInputXml = Path.Combine(testReportDir, $"{testAssembly.GetName().Name}.Report-NUnit.xml");
                var nUnitArgs = new List<string> { "--result=" + fileInputXml }.ToArray();

                new AutoRun(testAssembly).Execute(nUnitArgs);
            }
        }

        private static IEnumerable<string> TestAssembliesFromCommandLineArguments()
        {
            var args = Environment.GetCommandLineArgs();
            var path = "";
            foreach (var arg in args)
            {
                if (!arg.ToLower().StartsWith(@"/tests:")) continue;

                path = arg.Replace(@"/tests:", "");
                break;
            }

            if (string.IsNullOrEmpty(path)) return null;

            return from f in Directory.EnumerateFiles(path) where f.ToLower().EndsWith("tests.dll") select f;
        }
    }
}

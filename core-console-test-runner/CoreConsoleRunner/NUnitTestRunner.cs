using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;
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
            if (DebuggerFromCommandLineArguments()) Debugger.Launch();

            var files = TestAssembliesFromCommandLineArguments();
            TestRunner(files);
        }

        [CommandMethod("RunTestsFrom", CommandFlags.Session)]
        public void RunTestsFrom()
        {
            if (DebuggerFromCommandLineArguments()) Debugger.Launch();

            var files = TestAssembliesFromPrompt();
            TestRunner(files);
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

        private static IEnumerable<string> TestAssembliesFromPrompt()
        {
            var result = Application.DocumentManager.MdiActiveDocument.Editor.GetString("Enter path: ");
            var path = result.StringResult;

            if (string.IsNullOrEmpty(path)) return null;

            return from f in Directory.EnumerateFiles(path) where f.ToLower().EndsWith("tests.dll") select f;
        }

        private static bool DebuggerFromCommandLineArguments()
        {
            var args = Environment.GetCommandLineArgs();
            return args.Any(arg => arg.ToLower().StartsWith(@"/debug"));
        }

        private static void TestRunner(IEnumerable<string> files)
        {
            if (files == null) return;

            foreach (var file in files)
            {
                var assemblyDir = Path.GetDirectoryName(file);
                var testAssembly = Assembly.LoadFrom(file);
                if (assemblyDir == null) continue;

                var testReportDir = Path.Combine(assemblyDir, @"Report-NUnit");
                if (!Directory.Exists(testReportDir)) Directory.CreateDirectory(testReportDir);

                var fileInputXml = Path.Combine(testReportDir, $"{testAssembly.GetName().Name}.Report-NUnit.xml");
                var nUnitArgs = new List<string> { "--process=SINGLE", "--result=" + fileInputXml }.ToArray();

                new AutoRun(testAssembly).Execute(nUnitArgs);
            }
        }
    }
}

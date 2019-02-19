using System;
using System.Diagnostics;
using System.IO;

namespace BaseTestLibrary
{
    internal static class CoreConsole
    {
        private const string CORE_CONSOLE = @"C:\Program Files\Autodesk\AutoCAD 2019\accoreconsole.exe";

        internal static int Run(string drawingFilePath, string scriptFilePath, int maxWaitInMilliseconds, bool showConsoleWindow = true)
        {
            if (!File.Exists(CORE_CONSOLE)) throw new ArgumentException("Location of accoreconsole.exe not found !");
                    
            var applicationArguments = $"/i \"{drawingFilePath}\" /s \"{scriptFilePath}\" /l en-US";
            var processObj = new Process { StartInfo = {FileName = CORE_CONSOLE, Arguments = applicationArguments} };

            if (showConsoleWindow)
            {
                processObj.StartInfo.UseShellExecute = true;
                processObj.StartInfo.CreateNoWindow = false;
            }
            else
            {
                processObj.StartInfo.UseShellExecute = false;
                processObj.StartInfo.CreateNoWindow = true;
                processObj.StartInfo.RedirectStandardOutput = true;
            }

            processObj.Start();
            processObj.WaitForExit(maxWaitInMilliseconds);

            return processObj.Id;
        } 
    }
}

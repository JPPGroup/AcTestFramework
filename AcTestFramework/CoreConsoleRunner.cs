using System;
using System.Diagnostics;
using System.IO;

namespace Jpp.AcTestFramework
{
    internal static class CoreConsoleRunner
    {        
        internal static int Run(string appPath, string drawingFilePath, string scriptFilePath, int maxWaitInMilliseconds, bool showConsoleWindow)
        {
            if (!File.Exists(appPath)) throw new ArgumentException("Location of application exe not found.");
            
            var hasDrawing = !string.IsNullOrEmpty(drawingFilePath);

            var applicationArguments = hasDrawing ? $"/i \"{drawingFilePath}\" /s \"{scriptFilePath}\" /isolate /l en-gb" : $"/s \"{scriptFilePath}\" /isolate /l en-gb";
            var processObj = new Process { StartInfo = {FileName = appPath, Arguments = applicationArguments} };

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

        internal static int Run(string appPath, string scriptFilePath, int maxWaitInMilliseconds, bool showConsoleWindow)
        {
            return Run(appPath, null, scriptFilePath, maxWaitInMilliseconds, showConsoleWindow);        
        }

    }
}

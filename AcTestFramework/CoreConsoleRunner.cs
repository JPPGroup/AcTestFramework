using System;
using System.Diagnostics;
using System.IO;

namespace Jpp.AcTestFramework
{
    internal static class CoreConsoleRunner
    {
        private static FileLogger _logger;

        internal static int Run(FileLogger logger, string appPath, string drawingFilePath, string scriptFilePath, int maxWaitInMilliseconds, bool showConsoleWindow)
        {
            _logger = logger;

            if (!File.Exists(appPath)) throw new ArgumentException("Location of application exe not found.");
            
            var hasDrawing = !string.IsNullOrEmpty(drawingFilePath);

            var applicationArguments = hasDrawing ? $"/i \"{drawingFilePath}\" /s \"{scriptFilePath}\" /isolate /l en-gb" : $"/s \"{scriptFilePath}\" /isolate /l en-gb";
            var processObj = new Process { StartInfo = {FileName = appPath, Arguments = applicationArguments } };

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
                processObj.StartInfo.RedirectStandardError = true;
            }

            processObj.OutputDataReceived += CaptureOutput;
            processObj.ErrorDataReceived += CaptureError;

            processObj.Start();
            processObj.BeginOutputReadLine();
            processObj.BeginErrorReadLine();
            processObj.WaitForExit(maxWaitInMilliseconds);

            return processObj.Id;
        }

        internal static int Run(FileLogger logger, string appPath, string scriptFilePath, int maxWaitInMilliseconds, bool showConsoleWindow)
        {
            return Run(logger, appPath, null, scriptFilePath, maxWaitInMilliseconds, showConsoleWindow);        
        }


        private static void CaptureOutput(object sender, DataReceivedEventArgs e)
        {
            ShowOutput(e.Data);
        }

        private static void CaptureError(object sender, DataReceivedEventArgs e)
        {
            ShowOutput(e.Data);
        }

        private static void ShowOutput(string data)
        {
            if (data != null)
            {
                _logger.Entry(data);
            }
        }

    }
}

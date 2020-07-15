using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Jpp.AcTestFramework.Properties;

namespace Jpp.AcTestFramework
{
    internal class Runner
    {
        private const string SCRIPT_FILE_EXT = "scr";
        private const string DRAWING_FILE_EXT = "dwg";

        private readonly FileLogger logger;
        private readonly BaseFixtureArguments arguments;
        private readonly string fixtureId;

        private int processId;
        private string tempDrawingFile;
        private string tempScriptFile;

        public Runner(string fixtureId, BaseFixtureArguments arguments)
        {
            if (string.IsNullOrEmpty(fixtureId))
            {
                throw new ArgumentNullException(nameof(fixtureId));
            }

            this.fixtureId = fixtureId;
            this.arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
            logger = new FileLogger(arguments.Directory, FileLogger.LogType.Runner, arguments.IsDebug);
        }

        internal void Run()
        {
            logger.Entry("Process starting...");

            if (!File.Exists(arguments.ApplicationPath)) throw new ArgumentException($"Location of application exe not found - searched for {arguments.ApplicationPath}");
            
            GenerateFiles();

            var process = CreateProcess();

            process.OutputDataReceived += CaptureOutput;
            process.ErrorDataReceived += CaptureError;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit(arguments.WaitForExitMilliseconds);

            processId = process.Id;
        }

        internal void Stop()
        {
            logger.Entry("Process stopping...");
            Thread.Sleep(TimeSpan.FromSeconds(arguments.WaitBeforeKillSeconds));
            if (processId > 0) KillProcess();

            RemoveFiles();
        }

        private Process CreateProcess()
        {
            var process = new Process {StartInfo = { FileName = arguments.ApplicationPath, Arguments = CreateArguments() } };

            if (arguments.DisplayWindow)
            {
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = false;
            }
            else
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
            }

            return process;
        }

        private void CaptureOutput(object sender, DataReceivedEventArgs e)
        {
            ShowOutput(e.Data);
        }

        private void CaptureError(object sender, DataReceivedEventArgs e)
        {
            ShowOutput(e.Data);
        }

        private void ShowOutput(string data)
        {
            if (data != null) logger.Entry(data);
        }

        private void KillProcess()
        {
            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                if (process.Id != processId) continue;
                process.Kill();
                break;
            }
        }

        private void GenerateFiles()
        {
            tempDrawingFile = CreateDrawingFile();
            tempScriptFile = CreateScriptFile();
        }

        private void RemoveFiles()
        {
            DeleteIfExists(tempDrawingFile);
            DeleteIfExists(tempScriptFile);
        }

        private string CreateDrawingFile()
        {
            if (string.IsNullOrEmpty(arguments.DrawingFile)) return "";

            var fileName = $"{fixtureId}.{DRAWING_FILE_EXT}";
            var filePath = Path.Combine(arguments.Directory, fileName);

            DeleteIfExists(filePath);
            Directory.SetCurrentDirectory(arguments.Directory);
            File.Copy(arguments.DrawingFile, filePath);

            return filePath;
        }

        private string CreateScriptFile()
        {
            var scriptFileName = $"{fixtureId}.{SCRIPT_FILE_EXT}";
            var scriptFilePath = Path.Combine(arguments.Directory, scriptFileName);
            var listenLibFilePath = arguments.ListenLocation;
            var content = Resources.CommandScriptTemplate
                .Replace("<DLLPATH>", listenLibFilePath)
                .Replace("<CUSTOMCOMMAND>", "START_LISTENER")
                .Replace("<DEBUG>", arguments.IsDebug.ToString())
                .Replace("<TESTGUID>", fixtureId);

            DeleteIfExists(scriptFilePath);

            TextWriter tw = new StreamWriter(scriptFilePath, false);
            tw.Write(content);
            tw.Flush();
            tw.Close();

            return scriptFilePath;
        }

        private static bool DeleteIfExists(string filePath)
        {
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    if (File.Exists(filePath)) File.Delete(filePath);
                    return true;
                }
                catch
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }

            return false;
        }

        private string CreateArguments()
        {
            var hasDrawing = !string.IsNullOrEmpty(tempDrawingFile);

            switch (arguments.AppType)
            {
                case AppTypes.CoreConsole:
                    return CoreConsoleString(hasDrawing);
                case AppTypes.Full:
                    return FullString(hasDrawing);
                case AppTypes.Civil3d:
                    return C3dString(hasDrawing);
                default:
                    throw new ArgumentOutOfRangeException(nameof(arguments.AppType));
            }
        }

        private string CoreConsoleString(bool hasDrawing)
        {
            return hasDrawing
                ? $"/i \"{tempDrawingFile}\" /s \"{tempScriptFile}\" /l en-gb"
                : $"/s \"{tempScriptFile}\" /l en-gb";
        }

        private string FullString(bool hasDrawing)
        {
#if Ac2021
            return hasDrawing
                ? $"/P AutoCad /product ACAD /language en-gb \"{tempDrawingFile}\" /b \"{tempScriptFile}\""
                : $"/P AutoCad /product ACAD /language en-gb  /t \"No Template - Metric\" /b \"{tempScriptFile}\"";
#else 
            return hasDrawing
                ? $"/product ACAD /language en-gb \"{tempDrawingFile}\" /b \"{tempScriptFile}\""
                : $"/product ACAD /language en-gb  /t \"No Template - Metric\" /b \"{tempScriptFile}\"";
#endif
        }

        private string C3dString(bool hasDrawing)
        {
            var dir = Path.GetDirectoryName(arguments.ApplicationPath);
            var aecBase = Path.Combine(dir ?? throw new ArgumentNullException(nameof(dir)), "AecBase.dbx");
            return hasDrawing
                ? $"/ld \"{aecBase}\" /p \"<<C3D_Metric>>\" /Product \"C3D\" /language en-gb \"{tempDrawingFile}\" /b \"{tempScriptFile}\""
                : $"/ld \"{aecBase}\" /p \"<<C3D_Metric>>\" /Product \"C3D\" /language en-gb /t \"No Template - Metric\"  /b \"{tempScriptFile}\"";
        }
    }
}

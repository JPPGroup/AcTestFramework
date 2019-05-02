﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Jpp.AcTestFramework.Properties;

namespace Jpp.AcTestFramework
{
    /// <summary>
    /// A set of utility methods
    /// </summary>
    public static class Utilities
    {
        private const string SCRIPT_FILE_EXT = "scr";
        private const string DRAWING_FILE_EXT = "dwg";
       
        private static string GetExecutingDirectoryByAssemblyLocation() => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string GetExecutingAssemblyLocation() => Assembly.GetExecutingAssembly().Location;

        /// <summary>
        /// Creates a copy of a drawing
        /// </summary>
        /// <param name="fixtureId">Id for drawing file name</param>
        /// <param name="drawingFile">Original drawing file location</param>
        /// <returns>Location of newly created drawing</returns>
        public static string CreateDrawingFile(string fixtureId, string drawingFile)
        {
            if (string.IsNullOrEmpty(drawingFile)) return "";

            var fileName = $"{fixtureId}.{DRAWING_FILE_EXT}";
            var filePath = Path.Combine(GetExecutingDirectoryByAssemblyLocation(), fileName);

            DeleteIfExists(filePath);
            Directory.SetCurrentDirectory(GetExecutingDirectoryByAssemblyLocation());
            File.Copy(drawingFile, filePath);

            return filePath;
        }

        /// <summary>
        /// Creates a AutoCAD Core Console script
        /// </summary>
        /// <param name="fixtureId">Id for script file name</param>
        /// <param name="isDebug"></param>
        /// <returns>Location of newly created drawing</returns>
        public static string CreateScriptFile(string fixtureId, bool isDebug)
        {
            var scriptFileName = $"{fixtureId}.{SCRIPT_FILE_EXT}";
            var scriptFilePath = Path.Combine(GetExecutingDirectoryByAssemblyLocation(), scriptFileName);
            var listenLibFilePath = GetExecutingAssemblyLocation();
            var content = Resources.CommandScriptTemplate
                .Replace("<DLLPATH>", listenLibFilePath)
                .Replace("<CUSTOMCOMMAND>", "START_LISTENER")
                .Replace("<DEBUG>", isDebug.ToString())
                .Replace("<TESTGUID>", fixtureId);

            DeleteIfExists(scriptFilePath);

            TextWriter tw = new StreamWriter(scriptFilePath, false);
            tw.Write(content);
            tw.Flush();
            tw.Close();

            return scriptFilePath;
        }

        internal static bool DeleteIfExists(string filePath)
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

        internal static void KillProcess(int pid)
        {
            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                if (process.Id != pid) continue;
                process.Kill();
                break;
            }
        }
    }
}

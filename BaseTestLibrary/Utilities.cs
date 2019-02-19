using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using BaseTestLibrary.Properties;

namespace BaseTestLibrary
{
    /// <summary>
    /// A set of utility methods
    /// </summary>
    internal class Utilities
    {
        private const string SCRIPT_FILE_EXT = "scr";
        private const string DRAWING_FILE_EXT = "dwg";

        public static string GetExecutingDirectoryByAssemblyLocation() => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string GetExecutingAssemblyLocation() => Assembly.GetExecutingAssembly().Location;

        internal static string CreateDrawingFile(Guid fixtureGuid, string drawingFile)
        {
            var fileName = $"{fixtureGuid.ToString()}.{DRAWING_FILE_EXT}";
            var filePath = Path.Combine(GetExecutingDirectoryByAssemblyLocation(), fileName);
            DeleteIfExists(filePath);

            File.Copy(drawingFile, filePath);

            return filePath;
        }

        internal static string CreateScriptFile(Guid fixtureGuid)
        {
            var scriptFileName = $"{fixtureGuid.ToString()}.{SCRIPT_FILE_EXT}";
            var scriptFilePath = Path.Combine(GetExecutingDirectoryByAssemblyLocation(), scriptFileName);
            var listenLibFilePath = GetExecutingAssemblyLocation();
            var content = Resources.CommandScriptTemplate.Replace("<DLLPATH>", listenLibFilePath).Replace("<CUSTOMCOMMAND>", "START_LISTENER").Replace("<TESTGUID>", fixtureGuid.ToString());

            DeleteIfExists(scriptFilePath);

            TextWriter tw = new StreamWriter(scriptFilePath, false);
            tw.Write(content);
            tw.Flush();
            tw.Close();

            return scriptFilePath;
        }

        internal static bool DeleteIfExists(string filePath)
        {
            try
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
            catch
            {
                return false;
            }
            return true;
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

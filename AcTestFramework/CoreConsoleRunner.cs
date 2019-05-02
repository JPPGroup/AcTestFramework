using System;
using System.Diagnostics;
using System.IO;
using System.DirectoryServices.AccountManagement;
using System.Security;

namespace Jpp.AcTestFramework
{
    internal static class CoreConsoleRunner
    {
        private const string AC_CORE_USER = "AcCore";
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

            bool userExists;
            using (var pc = new PrincipalContext(ContextType.Machine))
            {
                var up = UserPrincipal.FindByIdentity(pc, IdentityType.SamAccountName, AC_CORE_USER);

                userExists = (up != null);
            }

            if (userExists)
            {
                var securePwd = new SecureString();
                securePwd.AppendChar('C');
                securePwd.AppendChar('e');
                securePwd.AppendChar('d');
                securePwd.AppendChar('a');
                securePwd.AppendChar('r');
                securePwd.AppendChar('b');
                securePwd.AppendChar('a');
                securePwd.AppendChar('r');
                securePwd.AppendChar('n');
                securePwd.AppendChar('1');
                securePwd.AppendChar('2');
                securePwd.AppendChar('3');

                processObj.StartInfo.UserName = AC_CORE_USER;
                processObj.StartInfo.Password = securePwd;
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

using System;
using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace Jpp.AcTestFramework.Pipe
{
    public class Listener
    {
        [CommandMethod("START_LISTENER")]
        public static void StartListeners()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;

            var promptDebug = ed.GetString("\nIs debug: ");
            if (promptDebug.Status != PromptStatus.OK) return;

            var debug = Convert.ToBoolean(promptDebug.StringResult);

            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            using (var log = new FileLogger(currentDir, debug))
            {
                var promptListener = ed.GetString("\nEnter the listener id: ");
                if (promptListener.Status != PromptStatus.OK) return;

                var pipeServer = new Server(log);
                pipeServer.Listen(promptListener.StringResult);
            }                                           
        }
    }
}

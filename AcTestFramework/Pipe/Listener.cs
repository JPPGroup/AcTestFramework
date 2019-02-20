using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace AcTestFramework.Pipe
{
    public class Listener
    {
        [CommandMethod("START_LISTENER")]
        public static void StartListeners()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var pipeServer = new Server();

            var promptResult = ed.GetString("\nEnter the parameter: ");
            if (promptResult.Status != PromptStatus.OK) return;

            pipeServer.Listen(promptResult.StringResult);
        }
    }
}

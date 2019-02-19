using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace BaseTestLibrary.Pipe
{
    public class Listener
    {
        private readonly Server _pipeServer;

        public Listener()
        {
            _pipeServer = new Server();
        }

        [CommandMethod("START_LISTENER")]
        public void StartListeners()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;

            var promptResult = ed.GetString("\nEnter the parameter: ");
            if (promptResult.Status != PromptStatus.OK) return;

            _pipeServer.Listen(promptResult.StringResult);
        }
    }
}

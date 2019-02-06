using System;
using System.IO;
using Autodesk.AutoCAD.DatabaseServices;
using BoilerPlate.AutoCAD.UnitTests.Helpers;
using NUnit.Framework;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace BoilerPlate.AutoCAD.UnitTests
{
    public abstract class BaseTests
    {
        protected void ExecuteActionDwg(string pDrawingPath, params Action<Database, Transaction>[] pActionList)
        {
            bool buildDefaultDrawing;

            if (string.IsNullOrEmpty(pDrawingPath))
            {
                buildDefaultDrawing = true;
            }
            else
            {
                buildDefaultDrawing = false;

                if (!File.Exists(pDrawingPath))
                {
                    Assert.Fail("The file '{0}' doesn't exist", pDrawingPath);
                    return;
                }
            }

            Exception exceptionToThrown = null;

            var doc = Application.DocumentManager.MdiActiveDocument;
            using (doc.LockDocument())
            {
                using (var db = new Database(buildDefaultDrawing, false))
                {
                    if (!string.IsNullOrEmpty(pDrawingPath))
                        db.ReadDwgFile(pDrawingPath, FileOpenMode.OpenForReadAndWriteNoShare, true, null);

                    using (new WorkingDatabaseSwitcher(db))
                    {
                        foreach (var item in pActionList)
                        {
                            using (var tr = db.TransactionManager.StartTransaction())
                            {
                                try
                                {
                                    item(db, tr);
                                }
                                catch (Exception ex)
                                {
                                    exceptionToThrown = ex;
                                    tr.Commit();
                                    break;
                                }

                                tr.Commit();
                            }
                        }
                    }
                }
            }

            if (exceptionToThrown != null)
            {
                throw exceptionToThrown;
            }
        }

    }
}

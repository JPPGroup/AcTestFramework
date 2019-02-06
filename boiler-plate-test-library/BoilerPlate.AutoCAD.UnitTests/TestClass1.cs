using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using BoilerPlate.AutoCAD.UnitTests.Helpers;
using NUnit.Framework;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace BoilerPlate.AutoCAD.UnitTests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class TestClass1 : BaseTests
    {
        [Test]
        public void Test_method_should_pass()
        {
            Assert.Pass("Test that should pass did not pass");
        }


        [Test]
        public void Test_method_should_fail()
        {
            Assert.Fail("This test was supposed to fail.");
        }

        [Test]
        public void Test_method_existing_drawing()
        {
            //Use existing drawing

            var drawingPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Drawings", "DrawingTest.dwg");
            const long lineId = 526;

            Action<Database, Transaction> action1 = (db, trans) =>
            {
                if (!db.TryGetObjectId(new Handle(lineId), out var objectId))
                {
                    Assert.Fail("ObjectID doesn't exist");
                }

                var line = trans.GetObject(objectId, OpenMode.ForWrite) as Line;

                Assert.IsNotNull(line, "Line didn't found");

                line.Erase();
            };

            Action<Database, Transaction> action2 = (db, trans) =>
            {
                //Check in another transaction if the line was erased

                if (db.TryGetObjectId(new Handle(lineId), out var objectId))
                {
                    Assert.IsTrue(objectId.IsErased, "Line didn't erased");
                }
            };

            ExecuteActionDwg(drawingPath, action1, action2);
        }


        [Test]
        public void Test_method_new_drawing()
        {
            //Use a new drawing

            long lineId = 0;

            Action<Database, Transaction> action1 = (db, trans) =>
            {
                var line = new Line(new Point3d(0, 0, 0), new Point3d(100, 100, 100));

                var blockTable = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                var modelSpace = (BlockTableRecord)trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                var objectId = modelSpace.AppendEntity(line);
                trans.AddNewlyCreatedDBObject(line, true);

                lineId = objectId.Handle.Value;
            };

            Action<Database, Transaction> action2 = (db, trans) =>
            {
                //Check in another transaction if the line was created

                if (!db.TryGetObjectId(new Handle(lineId), out _))
                {
                    Assert.Fail("Line didn't created");
                }
            };

            ExecuteActionDwg(null, action1, action2);
        }

        [Test]
        public void Test_method_name()
        {
            Action<Database, Transaction> action1 = (db, trans) =>
            {
                DBText dbText = new DBText { TextString = "cat" };
                string testMe;

                ObjectId dbTextObjectId = DbEntity.AddToModelSpace(dbText, db);
                dbText.TextString = "dog";

                DBText testText = trans.GetObject(dbTextObjectId, OpenMode.ForRead) as DBText;
                testMe = testText != null ? testText.TextString : string.Empty;

                // Assert
                StringAssert.AreEqualIgnoringCase("dog", testMe, "DBText string was not changed to \"dog\".");
                StringAssert.AreNotEqualIgnoringCase("cat", testMe, "DBText string was not changed.");
            };

            ExecuteActionDwg(null, action1);

        }

        [Test]
        public void Old_Test_That_Used_to_Crash_2016_and_not_2013_but_I_fixed_it()
        {
            // Arrange
            Database db = HostApplicationServices.WorkingDatabase;
            Document doc = Application.DocumentManager.GetDocument(db);

            string testMe;
            // Act
            using (doc.LockDocument())
            {
                using (var tx = db.TransactionManager.StartTransaction())
                {
                    using (DBText dbText = new DBText { TextString = "cat" })
                    {
                        ObjectId dbTextObjectId = DbEntity.AddToModelSpace(dbText, db);
                        dbText.TextString = "dog";

                        var testText = dbTextObjectId.Open(OpenMode.ForRead, false) as DBText;
                        testMe = testText != null
                                     ? testText.TextString
                                     : string.Empty;
                    }
                    tx.Commit();
                }
            }
            // Assert
            StringAssert.AreEqualIgnoringCase("dog", testMe, "DBText string was not changed to \"dog\".");
            StringAssert.AreNotEqualIgnoringCase("cat", testMe, "DBText string was not changed.");
        }

        [Test, TestCaseSource(typeof(SampleTestsData), "ParametersTest")]
        public void Test_method_TestCaseSource(Point3d pPoint1, Point3d pPoint2, Point3d pPointResult)
        {
            List<Point3d> listPoints = new List<Point3d>() { pPoint1, pPoint2 };
            Point3d centerPoint = new Point3d(listPoints.Average(p => p.X), listPoints.Average(p => p.Y), listPoints.Average(p => p.Z));

            Assert.IsTrue(centerPoint.DistanceTo(pPointResult) < 1E-5);

        }

    }

}

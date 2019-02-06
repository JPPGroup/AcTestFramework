using System.Collections;
using Autodesk.AutoCAD.Geometry;
using NUnit.Framework;

namespace BoilerPlate.AutoCAD.UnitTests
{
    public class SampleTestsData
    {
        public static IEnumerable ParametersTest
        {
            get
            {
                const string description = "Test the function Average Point";
                const string category = "Geometry Functions";

                yield return new TestCaseData(
                        new Point3d(0, 0, 0),
                        new Point3d(1000, 1000, 1000),
                        new Point3d(500, 500, 500))
                    .SetDescription(description)
                    .SetCategory(category);

                yield return new TestCaseData(
                        new Point3d(0, 0, 0),
                        new Point3d(300, 300, 0),
                        new Point3d(150, 150, 0))
                    .SetDescription(description)
                    .SetCategory(category);
            }
        }

    }
}

using System;
using Autodesk.AutoCAD.DatabaseServices;

namespace BoilerPlate.AutoCAD.UnitTests.Helpers
{
    internal sealed class WorkingDatabaseSwitcher : IDisposable
    {
        Database _oldDb = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="db">Target database.</param>
        public WorkingDatabaseSwitcher(Database db)
        {
            _oldDb = HostApplicationServices.WorkingDatabase;
            HostApplicationServices.WorkingDatabase = db;
        }

        public void Dispose()
        {
            HostApplicationServices.WorkingDatabase = _oldDb;
        }
    }
}

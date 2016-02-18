using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity.SqlServer;

namespace server_api
{


    public class AlmanacRepository : IDisposable
    {
        private AirUDBCOE db;

        public AlmanacRepository()
        {
            db = new AirUDBCOE();
        }

        public AlmanacRepository(AirUDBCOE existingContext)
        {
            db = existingContext;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
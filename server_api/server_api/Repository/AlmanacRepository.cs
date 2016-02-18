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

        public IEnumerable<Daily> GetNDailiesByStationID(string stationId, int n){

            DateTime dateNDaysInBack = DateTime.Today.AddDays(-n);

            return from d in db.Dailies
                          where d.Station.Id == stationId &&
                                d.Time > dateNDaysInBack
                          select d;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
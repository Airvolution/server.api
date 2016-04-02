using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity.SqlServer;
using server_api.Models;

namespace server_api
{


    public class AlmanacRepository : IDisposable
    {
        private ApplicationContext db;

        public AlmanacRepository()
        {
            db = new ApplicationContext();
        }

        public AlmanacRepository(ApplicationContext existingContext)
        {
            db = existingContext;
        }

        public IEnumerable<Daily> GetNDailiesByStationID(string stationId, int n){

            DateTime dateNDaysInBack = DateTime.UtcNow.Date.AddDays(-n);

            return from d in db.Dailies
                          where d.Station.Id == stationId &&
                                d.Date > dateNDaysInBack
                          select d;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
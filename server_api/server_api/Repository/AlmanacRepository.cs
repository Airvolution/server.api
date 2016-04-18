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

        public IEnumerable<Daily> GetNDailiesByStationID(string stationId, int daysBack){

            DateTime dateNDaysInBack = DateTime.UtcNow.Date.AddDays(-daysBack);
            var dailies = from d in db.Dailies
                          where d.Station.Id == stationId &&
                                d.Date > dateNDaysInBack
                                orderby d.Date descending
                          select d;

            return this.PadDailies(dailies, daysBack);
        }

        private IEnumerable<Daily> PadDailies(IEnumerable<Daily> dailies, int daysBack)
        {
            var pad = daysBack - dailies.Count();
            List<Daily> padding = new List<Daily>();
            for (int i = 0; i < pad; i++)
            {
                Daily daily = new Daily();
                //This is kinda sucky but makes plots work on the front end. Supposedly this will never happen with good data.
                daily.MaxAQI = -1;
                daily.MinAQI = -1;
                daily.AvgAQI = -1;
                daily.MinCategory = -1;
                daily.MaxCategory = -1;
                daily.MaxParameter = null;
                daily.MinParameter = null;
                padding.Add(daily);
            }
            var result = dailies.Concat(padding);
            return result;

            
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
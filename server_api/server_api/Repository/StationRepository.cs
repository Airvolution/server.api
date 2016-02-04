using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace server_api {
    public class StationsRepository : IDisposable {
        private AirUDBCOE db;

        public StationsRepository() {
            db = new AirUDBCOE();
        }

        public bool StationExists(string stationID) {
            if (db.Stations.Find(stationID) == null) {
                return false;
            }
            return true;
        }

        public IEnumerable<DataPoint> GetDataPointsFromStation(string stationID) {
            IEnumerable<DataPoint> data = from point in db.DataPoints
                                          where point.Station.Id == stationID
                                          select point;
            return data;
        }

        public void Dispose() {
            db.Dispose();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace server_api
{
    public class StationsRepository : IDisposable
    {
        private AirUDBCOE db;

        public StationsRepository()
        {
            db = new AirUDBCOE();
        }

        public StationsRepository(AirUDBCOE existingContext)
        {
            db = existingContext;
        }

        public bool StationExists(string stationID)
        {
            if (db.Stations.Find(stationID) == null)
            {
                return false;
            }
            return true;
        }

        public IEnumerable<Station> StationLocations(decimal latMin, decimal latMax, decimal lngMin, decimal lngMax)
        {
            IEnumerable<Station> data = from station in db.Stations
                                        where station.Lat >= latMin && station.Lat <= latMax && station.Lng >= lngMin && station.Lng <= lngMax
                                        select station;
            return data;
        }



        public IEnumerable<DataPoint> GetLatestDataPointsFromStation(string stationID)
        {
            List<DataPoint> data = (from points in db.DataPoints
                                           where points.Station.Id == stationID
                                           group points by points.Parameter.Name into paramPoints
                                           select new
                                           {
                                               dataPoints = paramPoints.OrderByDescending(a => a.Time).FirstOrDefault()
                                           }).Select(c => c.dataPoints).ToList();

            return data;
        }

        public bool SetDataPointsFromStation(DataPoint[] dataSet)
        {
            string stationId = dataSet[0].Station.Id;
            Station dataSetStation = db.Stations.Find(stationId);
            
            
            if (dataSetStation == null)
            {
                return false;
            }
            
            Dictionary<string, Parameter> existingParameters = new Dictionary<string, Parameter>();
            //Dictionary<Tuple<string, string>, Parameter> existingParameters2 = new Dictionary<Tuple<string, string>, Parameter>();            

            foreach (Parameter p in db.Parameters.ToList())
            {                
                existingParameters.Add(p.Name + p.Unit, p);
                //existingParameters2.Add(new Tuple<string,string>(p.Name, p.Unit), p);
            }

            
            Parameter tempParameter = null;
            DataPoint latestPoint = dataSet[0];
            //Parameter tempParameter2 = null;
            //for (int i = 0; i < 10000000; i++)
            //{
            foreach (DataPoint point in dataSet)
            {
                // Okay
                //Tuple<string, string> tempKey2 = new Tuple<string, string>(point.Parameter.Name, point.Parameter.Unit);
                //existingParameters2.TryGetValue(tempKey2, out tempParameter);                

                // Terrible idea, orders of magnitude slower (due to repeated hits to DB)
                //point.Parameter = db.Parameters.Find(point.Parameter.Name, point.Parameter.Unit);
                
                // Best - Negligible slow down
                existingParameters.TryGetValue(point.Parameter.Name + point.Parameter.Unit, out tempParameter);
                point.Parameter = tempParameter;

                point.Station = dataSetStation;

                if (latestPoint.Time < point.Time)
                {
                    latestPoint = point;
                }
            }
            //}

            dataSetStation.Indoor = latestPoint.Indoor;
            dataSetStation.Lat = latestPoint.Lat;
            dataSetStation.Lng = latestPoint.Lng;
           
            db.DataPoints.AddRange(dataSet);
            db.SaveChanges();

            return true;
        }

        public IEnumerable<DataPoint> GetDataPointsFromStation(string stationID)
        {
            IEnumerable<DataPoint> data = from point in db.DataPoints
                                          where point.Station.Id == stationID
                                          select point;
            return data;
        }

        public IEnumerable<DataPoint> GetDataPointsFromStationAfterTime(string stationID, DateTime after)
        {
            return GetDataPointsFromStationBetweenTimes(stationID, after, DateTime.Now);
        }

        public IEnumerable<DataPoint> GetDataPointsFromStationBetweenTimes(string stationID, DateTime after, DateTime before)
        {
            IEnumerable<DataPoint> data = from point in db.DataPoints
                                          where point.Station.Id == stationID &&
                                                point.Time > after &&
                                                point.Time < before
                                          select point;
            return data;
        }

        public bool DeleteStation(string stationID)
        {
            Station station = db.Stations.SingleOrDefault(s => s.Id == stationID);

            if (ReferenceEquals(station, null))
            {
                return false;
            }

            db.Stations.Remove(station);

            return true;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
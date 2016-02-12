using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace server_api
{

    public class DataPointComparer : IEqualityComparer<DataPoint>
    {

        public bool Equals(DataPoint x, DataPoint y)
        {
            return (x.Time.Equals(y.Time) &&
                   x.Parameter.Name.Equals(y.Parameter.Name) &&
                   x.Parameter.Unit.Equals(y.Parameter.Unit) &&
                   x.Station.Id.Equals(y.Station.Id)) ||
                   (x.Time.Equals(y.Time) &&
                   x.Station_Id.Equals(y.Station_Id) &&
                   x.Parameter_Name.Equals(y.Parameter_Name) &&
                   x.Parameter_Unit.Equals(y.Parameter_Unit));
        }

        public int GetHashCode(DataPoint obj)
        {
            string id = "";
            string p_name = "";
            string p_unit = "";

            if (obj.Station_Id != null)
                id = obj.Station_Id;
            else
                id = obj.Station.Id;

            if (obj.Parameter_Name != null)
                p_name = obj.Parameter_Name;
            else
                p_name = obj.Parameter.Name;

            if (obj.Parameter_Unit != null)
                p_unit = obj.Parameter_Unit;
            else
                p_unit = obj.Parameter.Unit;

            return StringComparer.InvariantCultureIgnoreCase.GetHashCode(id + 
                                                                         p_name + 
                                                                         p_unit);
        }
    }

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

        public Station GetStation(string stationID)
        {
            if (!StationExists(stationID))
            {
                return null;
            }
            return db.Stations.Find(stationID);
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

        public IEnumerable<DataPoint> SetDataPointsFromStation(DataPoint[] dataSet)
        {
            string stationId = dataSet[0].Station.Id;
            Station dataSetStation = db.Stations.Find(stationId);

            IEnumerable<DataPoint> existingDataPointsList = GetDataPointsFromStation(stationId);
            //IEnumerable<DataPoint> existingDataPointsList = GetDataPointsFromStationAfterTime(stationId, DateTime.UtcNow.AddHours(-2));
            DataPointComparer comparer = new DataPointComparer();
            HashSet<DataPoint> exisitingDataPoints = new HashSet<DataPoint>(comparer);

            List<DataPoint> addingDataPoints = new List<DataPoint>();

            foreach (DataPoint d in existingDataPointsList)
            {
                exisitingDataPoints.Add(d);
            }

            
            if (dataSetStation == null)
            {
                return null;
            }

            Dictionary<string, Parameter> existingParameters = new Dictionary<string, Parameter>();           

            foreach (Parameter p in db.Parameters.ToList())
            {                
                existingParameters.Add(p.Name + p.Unit, p);
            }

            
            Parameter tempParameter = null;
            DataPoint latestPoint = dataSet[0];

            foreach (DataPoint point in dataSet)
            {
                // Best - Negligible slow down
                existingParameters.TryGetValue(point.Parameter.Name + point.Parameter.Unit, out tempParameter);
                point.Parameter = tempParameter;
                point.Station = dataSetStation;
                point.Indoor = dataSetStation.Indoor;

                if (latestPoint.Time < point.Time)
                {
                    latestPoint = point;
                }

                if (!exisitingDataPoints.Contains(point))
                    addingDataPoints.Add(point);
            }

            dataSetStation.Indoor = latestPoint.Indoor;
            dataSetStation.Lat = latestPoint.Lat;
            dataSetStation.Lng = latestPoint.Lng;
           
            db.DataPoints.AddRange(addingDataPoints);
            db.SaveChanges();

            return addingDataPoints;
        }

        public IEnumerable<DataPoint> GetDataPointsFromStation(string stationID)
        {
            IEnumerable<DataPoint> data = from point in db.DataPoints
                                          where point.Station.Id == stationID
                                          select point;
            return data;
        }

        public IEnumerable<DataPoint> GetDataPointsFromStation(string stationID, string parameter)
        {
            IEnumerable<DataPoint> data = from point in db.DataPoints
                                          where point.Station.Id == stationID
                                          where point.Parameter.Name == parameter
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
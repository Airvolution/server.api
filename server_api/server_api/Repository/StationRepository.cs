using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace server_api
{

    public class DataPointComparer : IEqualityComparer<DataPoint>
    {
        public string setNonNull(string a, string b)
        {
            if (a != null)
                return a;
            else
                return b;
        }

        public bool Equals(DataPoint x, DataPoint y)
        {
            string sIdX = setNonNull(x.Station_Id, x.Station.Id);
            string pNameX = setNonNull(x.Parameter_Name, x.Parameter.Name);
            string pUnitX = setNonNull(x.Parameter_Unit, x.Parameter.Unit);

            string sIdY = setNonNull(y.Station_Id, y.Station.Id);
            string pNameY = setNonNull(y.Parameter_Name, y.Parameter.Name);
            string pUnitY = setNonNull(y.Parameter_Unit, y.Parameter.Unit);


            return x.Time.Equals(y.Time) &&
                   sIdX.Equals(sIdY) &&
                   pNameX.Equals(pNameY) &&
                   pUnitX.Equals(pUnitY);
        }

        public int GetHashCode(DataPoint obj)
        {
            string sId = setNonNull(obj.Station_Id, obj.Station.Id);
            string pName = setNonNull(obj.Parameter_Name, obj.Parameter.Name);
            string pUnit = setNonNull(obj.Parameter_Unit, obj.Parameter.Unit);

            return StringComparer.InvariantCultureIgnoreCase.GetHashCode(sId + 
                                                                         pName + 
                                                                         pUnit);
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
                point.Parameter_Name = tempParameter.Name;
                point.Parameter_Unit = tempParameter.Unit;

                point.Station = dataSetStation;
                point.Station_Id = dataSetStation.Id;

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity.SqlServer;

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


        public Station GetNearestStation(decimal lat, decimal lng)
        {
            double latD = (double)lat;
            double lngD = (double)lng;

            var result = (from outer in
                             (from s in db.Stations
                              select new
                              {
                                  Distance = (3959 * SqlFunctions.Acos(SqlFunctions.Cos(SqlFunctions.Radians(lat)) * 
                                                     SqlFunctions.Cos(SqlFunctions.Radians(s.Lat)) * 
                                                     SqlFunctions.Cos(SqlFunctions.Radians(s.Lng) - 
                                                     SqlFunctions.Radians(lng)) + 
                                                     SqlFunctions.Sin(SqlFunctions.Radians(lat)) * 
                                                     SqlFunctions.Sin((SqlFunctions.Radians(s.Lat))))),
                                  s
                              })
                         orderby outer.Distance
                         select outer.s).FirstOrDefault();

            return result;
        }

        public IEnumerable<Station> GetStationsWithinRadiusMiles(decimal lat, decimal lng, double radius)
        {
            double latD = (double)lat;
            double lngD = (double)lng;

            var result = from outer in
                             (from s in db.Stations
                              select new
                              {
                                  Distance = (3959 * SqlFunctions.Acos(SqlFunctions.Cos(SqlFunctions.Radians(lat)) *
                                                     SqlFunctions.Cos(SqlFunctions.Radians(s.Lat)) *
                                                     SqlFunctions.Cos(SqlFunctions.Radians(s.Lng) -
                                                     SqlFunctions.Radians(lng)) +
                                                     SqlFunctions.Sin(SqlFunctions.Radians(lat)) *
                                                     SqlFunctions.Sin((SqlFunctions.Radians(s.Lat))))),
                                  s
                              })
                         where outer.Distance < radius
                         orderby outer.Distance
                         select outer.s;                                                 

            return result;
        }

        public IEnumerable<Station> StationLocations(decimal latMin, decimal latMax, decimal lngMin, decimal lngMax)
        {
            IEnumerable<Station> data = from station in db.Stations
                                        where station.Lat >= latMin && station.Lat <= latMax && station.Lng >= lngMin && station.Lng <= lngMax
                                        where !(station.Lat == 0 && station.Lng == 0)
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

            db.Configuration.AutoDetectChangesEnabled = false;

            //IEnumerable<DataPoint> existingDataPointsList = GetDataPointsFromStation(stationId);
            IEnumerable<DataPoint> existingDataPointsList = GetDataPointsFromStationAfterTimeUtc(stationId, DateTime.UtcNow.AddHours(-2));
            DataPointComparer comparer = new DataPointComparer();
            HashSet<DataPoint> exisitingDataPoints = new HashSet<DataPoint>(comparer);
            Dictionary<string, DataPoint> latestDataPointsForEachParameter = new Dictionary<string, DataPoint>();
            List<DataPoint> addingDataPoints = new List<DataPoint>();

            DataPoint outPoint;

            // Determine which datapoints are already in database
            foreach (DataPoint d in existingDataPointsList)
            {
                exisitingDataPoints.Add(d);

                if (latestDataPointsForEachParameter.TryGetValue(d.Parameter_Name, out outPoint))
                {
                    if (outPoint.Time <= d.Time)
                        latestDataPointsForEachParameter[d.Parameter_Name] = d;
                }
                else
                    latestDataPointsForEachParameter[d.Parameter_Name] = d;
            }

            if (dataSetStation == null)
            {
                return null;
            }

            // Find the existing parameters in station
            Dictionary<string, Parameter> existingParameters = new Dictionary<string, Parameter>();           
            foreach (Parameter p in db.Parameters.ToList())
            {                
                existingParameters.Add(p.Name + p.Unit, p);
            }

            
            Parameter tempParameter = null;
            DataPoint latestPoint = dataSet[0];

            DataPoint maxAQIPoint = null;
            int maxAQI = 0;

            DataPoint minAQIPoint = null;
            int minAQI = -999;

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

                // GETTING LATEST OF EACH PARAMETER
                if (latestDataPointsForEachParameter.TryGetValue(point.Parameter_Name, out outPoint))
                {
                    if (outPoint.Time <= point.Time)
                        latestDataPointsForEachParameter[point.Parameter_Name] = point;
                }
                else
                    latestDataPointsForEachParameter[point.Parameter_Name] = point;



                latestDataPointsForEachParameter[point.Parameter_Name] = point;

                if (!exisitingDataPoints.Contains(point))
                {
                    if (point.AQI != -999 && point.Parameter != null)
                    {
                        if (maxAQI < point.AQI)
                        {
                            maxAQI = point.AQI;
                            maxAQIPoint = point;
                        }                        

                        if (minAQI == -999)
                        {
                            minAQI = point.AQI;
                            minAQIPoint = point;
                        }
                        else if (point.AQI < minAQI)
                        {
                            minAQI = point.AQI;
                            minAQIPoint = point;
                        }
                    }

                    addingDataPoints.Add(point);
                    exisitingDataPoints.Add(point);
                }
            }


            // Maks Latest Value    
            DataPoint maksAQIPoint = null;
            int maksAQI = 0;

            foreach (DataPoint p in latestDataPointsForEachParameter.Values)
            {
                if (p.AQI > maksAQI)
                {
                    maksAQI = p.AQI;
                    maksAQIPoint = p;
                }
            }

            db.Configuration.AutoDetectChangesEnabled = true;
            
            dataSetStation.Indoor = latestPoint.Indoor;
            dataSetStation.Lat = latestPoint.Lat;
            dataSetStation.Lng = latestPoint.Lng;


            Daily sDaily = db.Dailies.Find(DateTime.UtcNow.Date, stationId);

            // Min Daily Value
            if (minAQIPoint != null && minAQI != -999)
            {
                if (sDaily == null)
                {
                    sDaily = new Daily();
                    sDaily.Station = dataSetStation;
                    sDaily.Date = DateTime.UtcNow.Date;
                    sDaily.MinAQI = minAQIPoint.AQI;
                    sDaily.MinParameter = minAQIPoint.Parameter;
                    sDaily.MinCategory = minAQIPoint.Category;
                    db.Dailies.Add(sDaily);                    
                }
                else if (sDaily.MinAQI > minAQI)
                {
                    sDaily.MinAQI = minAQIPoint.AQI;
                    sDaily.MinParameter = minAQIPoint.Parameter;
                    sDaily.MinCategory = minAQIPoint.Category;
                }
                db.SaveChanges();
            }

            sDaily = db.Dailies.Find(DateTime.UtcNow.Date, stationId);

            // Max Daily Value
            if (maxAQIPoint != null)
            {                              
                if (sDaily.MaxAQI <= maxAQI)
                {
                    sDaily.MaxAQI = maxAQI;
                    sDaily.MaxParameter = maxAQIPoint.Parameter;
                    sDaily.MaxCategory = maxAQIPoint.Category;
                }
                db.SaveChanges();
            }
            
            

            // Macks Latest Value
            if (maksAQIPoint != null)
            {
                dataSetStation.AQI = maksAQIPoint.AQI;
                dataSetStation.Parameter = maksAQIPoint.Parameter;
            }
            

            // Average
            if (sDaily != null)
            {
                DateTime nextDay = sDaily.Date.AddDays(1);
                sDaily.AvgAQI = (from d in db.DataPoints
                                 where d.Station_Id==dataSetStation.Id && d.Time > sDaily.Date && d.Time < nextDay && d.AQI > 0
                                 select d.AQI).Average();
            }
            

            

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

        public IEnumerable<DataPoint> GetDataPointsFromStation2(string[] stationID, string[] parameter)
        {
            IEnumerable<DataPoint> data = db.DataPoints
                                            .Where(s => stationID.Contains(s.Station.Id))
                                            .Where(p => parameter.Contains(p.Parameter.Name));

            return data;
        }

        public IEnumerable<DataPoint> GetDataPointsFromStationAfterTime(string stationID, DateTime after)
        {
            return GetDataPointsFromStationBetweenTimes(stationID, after, DateTime.Now);
        }

        public IEnumerable<DataPoint> GetDataPointsFromStationAfterTimeUtc(string stationID, DateTime after)
        {
            return GetDataPointsFromStationBetweenTimes(stationID, after, DateTime.UtcNow);
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
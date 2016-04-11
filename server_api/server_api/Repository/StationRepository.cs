using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity.SqlServer;
using server_api.Models;

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

            string sIdY = setNonNull(y.Station_Id, y.Station.Id);
            string pNameY = setNonNull(y.Parameter_Name, y.Parameter.Name);


            return x.Time.Equals(y.Time) &&
                   sIdX.Equals(sIdY) &&
                   pNameX.Equals(pNameY);
        }

        public int GetHashCode(DataPoint obj)
        {
            string sId = setNonNull(obj.Station_Id, obj.Station.Id);
            string pName = setNonNull(obj.Parameter_Name, obj.Parameter.Name);

            return StringComparer.InvariantCultureIgnoreCase.GetHashCode(sId +  pName);
        }
    }

    public class StationsRepository : IDisposable
    {
        private ApplicationContext db;

        public StationsRepository()
        {
            db = new ApplicationContext();
        }

        public StationsRepository(ApplicationContext ctx)
        {
            db = ctx;
        }

        public bool StationExists(string stationID)
        {
            if (db.Stations.Find(stationID) == null)
            {
                return false;
            }
            return true;
        }

        public Object CreateStation(Station station)
        {
            if (!StationExists(station.Id))
            {
                UnregisteredStation unregisteredStation = db.UnregisteredStations.Where(s => station.Id == s.Id).FirstOrDefault();
                if (ReferenceEquals(unregisteredStation, null))
                {
                    return "Unknown station. Please make sure it is powered on and connected to a network.";
                }

                // remove from the unregistered table and move to registered table
                db.UnregisteredStations.Remove(unregisteredStation); 
                db.Stations.Add(station);
                db.SaveChanges();
                return db.Stations.Find(station.Id);
            }
            else
            {
                return "Station already registered. But hey, we love you too!";
            }            
        }

        public Station UpdateStation(Station station, Station update)
        {
            station.Agency = update.Agency;
            station.City = update.City;
            station.Indoor = update.Indoor;
            station.Location = update.Location;
            station.Name = update.Name;
            station.Postal = update.Postal;
            station.Purpose = update.Purpose;
            station.State = update.State;
            station.Type = update.Type;
            db.SaveChanges();
            return station;
        }

        public Station GetStation(string stationID)
        {
            if (!StationExists(stationID))
            {
                return null;
            }
            return db.Stations.Find(stationID);
        }

        public IEnumerable<Station> GetMultipleStations(IEnumerable<string> ids)
        {
            var result = from station in db.Stations 
                         where ids.Contains(station.Id) 
                         select station;
            return result;
        }

        public IEnumerable<Station> GetUserStations(string user_id)
        {
            var result = from station in db.Stations
                         where station.User_Id == user_id
                         select station;
            return result;
        }
        public Station GetNearestStation(double lat, double lng)
        {
            var result = (from outer in
                             (from s in db.Stations
                              select new
                              {
                                  Distance = (3959 * SqlFunctions.Acos(SqlFunctions.Cos(SqlFunctions.Radians(lat)) * 
                                                     SqlFunctions.Cos(SqlFunctions.Radians(s.Location.Latitude)) * 
                                                     SqlFunctions.Cos(SqlFunctions.Radians(s.Location.Longitude) - 
                                                     SqlFunctions.Radians(lng)) + 
                                                     SqlFunctions.Sin(SqlFunctions.Radians(lat)) * 
                                                     SqlFunctions.Sin((SqlFunctions.Radians(s.Location.Latitude))))),
                                  s
                              })
                         orderby outer.Distance
                         where outer.Distance != null
                         select outer.s);

            var value = result.First();

            return value;
        }

        public IEnumerable<Station> GetStationsWithinRadiusMiles(double lat, double lng, double radius)
        {
            var result = from outer in
                             (from s in db.Stations
                              select new
                              {
                                  Distance = (3959 * SqlFunctions.Acos(SqlFunctions.Cos(SqlFunctions.Radians(lat)) *
                                                     SqlFunctions.Cos(SqlFunctions.Radians(s.Location.Latitude)) *
                                                     SqlFunctions.Cos(SqlFunctions.Radians(s.Location.Longitude) -
                                                     SqlFunctions.Radians(lng)) +
                                                     SqlFunctions.Sin(SqlFunctions.Radians(lat)) *
                                                     SqlFunctions.Sin((SqlFunctions.Radians(s.Location.Latitude))))),
                                  s
                              })
                         where outer.Distance < radius
                         where outer.Distance != null
                         orderby outer.Distance
                         select outer.s;                                                 

            return result;
        }

        public IEnumerable<Station> StationLocations(double latMin, double latMax, double lngMin, double lngMax)
        {
            IEnumerable<Station> data = from station in db.Stations
                                        where station.Location.Latitude >= latMin && station.Location.Latitude <= latMax && station.Location.Longitude >= lngMin && station.Location.Longitude <= lngMax
                                        where !(station.Location.Latitude == 0 && station.Location.Longitude == 0)
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

        

        public IEnumerable<DataPoint> GetDataPointsFromStation(string stationID)
        {
            IEnumerable<DataPoint> data = from point in db.DataPoints
                                          where point.Station.Id == stationID
                                          orderby point.Time ascending
                                          select point;
            return data;
        }

        public IEnumerable<DataPoint> GetDataPointsFromStation(string[] stationID, string[] parameter)
        {
            IEnumerable<DataPoint> data = db.DataPoints
                                            .Where(s => stationID.Contains(s.Station.Id))
                                            .Where(p => parameter.Contains(p.Parameter.Name))
                                            .OrderBy(p => p.Time);

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
                                                point.Time >= after &&
                                                point.Time <= before
                                          orderby point.Time ascending
                                          select point;
            return data;
        }

        public IEnumerable<DataPoint> SetDataPointsFromStation(DataPoint[] dataSet)
        {
            string stationId = dataSet[0].Station.Id;
            Station dataSetStation = db.Stations.Find(stationId);
            if (dataSetStation == null)
            {
                return null;
            }

            DateTime startTime = dataSet[0].Time;
            DateTime today = startTime.Date;
            DateTime nextDay = startTime.AddDays(1);
            DateTime endTime = dataSet[dataSet.Length - 1].Time;
            DateTime twoHoursAgo = DateTime.UtcNow.AddHours(-2);

            if (startTime.Date != endTime.Date)
            {
                IEnumerable<DataPoint> newTailSet = from d in dataSet
                                                    where d.Time >= endTime.Date
                                                    select d;
                IEnumerable<DataPoint> newHeadSet = from d in dataSet
                                                    where d.Time < endTime.Date
                                                    select d;

                return SetDataPointsFromStation(newHeadSet.ToArray()).Concat(SetDataPointsFromStation(newTailSet.ToArray()));
            }

            db.Configuration.AutoDetectChangesEnabled = false;

            // Get values currently in database.
            DataPointComparer comparer = new DataPointComparer();
            HashSet<DataPoint> exisitingDataPoints = new HashSet<DataPoint>(comparer);
            exisitingDataPoints.UnionWith(GetDataPointsFromStationBetweenTimes(stationId, startTime, nextDay));

            // Latest DataPoints for each Parameter
            List<DataPoint> addingDataPoints = new List<DataPoint>();
                                             
            // Find the existing parameters in station
            Dictionary<string, Parameter> existingParameters = new Dictionary<string, Parameter>();
            foreach (Parameter p in db.Parameters.ToList())
            {
                existingParameters.Add(p.Name, p);
            }

            Parameter tempParameter = null;
            DataPoint latestPoint = dataSet[0];            

            //DataPoint outPoint;
            foreach (DataPoint point in dataSet)
            {
                // Best - Negligible slow down
                existingParameters.TryGetValue(point.Parameter.Name, out tempParameter);
                point.Parameter = tempParameter;
                point.Parameter_Name = tempParameter.Name;

                point.Station = dataSetStation;
                point.Station_Id = dataSetStation.Id;

                point.Indoor = dataSetStation.Indoor;

                if (!exisitingDataPoints.Contains(point))
                {
                    addingDataPoints.Add(point);
                    exisitingDataPoints.Add(point);
                }
            }

            db.Configuration.AutoDetectChangesEnabled = true;

            db.DataPoints.AddRange(addingDataPoints);
            db.SaveChanges();

            dataSetStation = db.Stations.Find(stationId);

            dataSetStation.Indoor = latestPoint.Indoor;
            dataSetStation.Location = latestPoint.Location;

            Daily sDaily = db.Dailies.Find(today, stationId);

            if (sDaily == null)
            {
                // if count of values for this station with values greater than or equal to zero > 0
                int number = (from d in db.DataPoints
                              where d.Station_Id == dataSetStation.Id && d.Time >= today && d.Time < nextDay
                              select d).Count();

                if (number > 0)
                {
                    sDaily = new Daily();
                    sDaily.Station = dataSetStation;
                    sDaily.Date = today;
                    db.Dailies.Add(sDaily);
                    db.SaveChanges();
                }                
            }

            sDaily = db.Dailies.Find(today, stationId);
            
            if (sDaily != null)
            {
                // Average
                sDaily.AvgAQI = (from d in db.DataPoints
                                 where d.Station_Id == dataSetStation.Id && d.Time >= today && d.Time < nextDay && d.AQI > 0
                                 select d.AQI).Average();

                // Min
                DataPoint min = (from d in db.DataPoints
                                 where d.Station_Id == dataSetStation.Id && d.Time >= today && d.Time < nextDay && d.AQI > 0
                                 orderby d.AQI ascending
                                 select d).FirstOrDefault();

                sDaily.MinAQI = min.AQI;
                sDaily.MinCategory = min.Category;
                sDaily.MinParameter = min.Parameter;

                // Max
                DataPoint max = (from d in db.DataPoints
                                 where d.Station_Id == dataSetStation.Id && d.Time >= today && d.Time < nextDay && d.AQI > 0
                                 orderby d.AQI descending
                                 select d).FirstOrDefault();

                sDaily.MaxAQI = max.AQI;
                sDaily.MaxCategory = max.Category;
                sDaily.MaxParameter = max.Parameter;
            }

            var latestPoints = (from points in db.DataPoints
                                where points.Station.Id == stationId
                                where points.Time > twoHoursAgo
                                group points by points.Parameter.Name into paramPoints
                                select new
                                {
                                    dataPoints = paramPoints.OrderByDescending(a => a.Time).FirstOrDefault()
                                }).Select(c => c.dataPoints);

            int latestMaxAQI = -1;
            DataPoint maxLatest = null;

            foreach (DataPoint d in latestPoints)
            {
                if (d.AQI > latestMaxAQI)
                {
                    latestMaxAQI = d.AQI;
                    maxLatest = d;
                }
            }

            if (maxLatest != null)
            {
                dataSetStation.AQI = maxLatest.AQI;
                dataSetStation.Location = maxLatest.Location;
                dataSetStation.Parameter = maxLatest.Parameter;
            }           

            
            db.SaveChanges();
            return addingDataPoints;
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

        public void AddThirdPartyDevice(UnregisteredStation station)
        {
            Station existingStation = db.Stations.Where(s => station.Id == s.Id).FirstOrDefault();
            if (!ReferenceEquals(existingStation, null))
            {
                return; // already registered
            }

            UnregisteredStation unregisteredStation = db.UnregisteredStations.Where(s => station.Id == s.Id).FirstOrDefault();
            if (!ReferenceEquals(unregisteredStation, null))
            {
                return; // already in the unregistered list
            }

            db.UnregisteredStations.Add(station);
            db.SaveChanges();
        }

        public void Dispose()
        {
            db.Dispose();
        }

    }
}
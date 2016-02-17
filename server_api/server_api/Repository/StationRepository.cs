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

        public double degreesToRadians(double deg) {
		    double rad = deg * Math.PI/180; // radians = degrees * pi/180
            return rad;
	    } 

        public double distanceBetweenLats(decimal latA, decimal lngA, decimal latB, decimal lngB){
            double latADouble = degreesToRadians(Convert.ToDouble(latA));
            double latBDouble = degreesToRadians(Convert.ToDouble(latB));
            double lngADouble = degreesToRadians(Convert.ToDouble(lngA));
            double lngBDouble = degreesToRadians(Convert.ToDouble(lngB));

            double dLat = latBDouble - latADouble;
            double dLng = lngBDouble - lngADouble;


            /*
             http://andrew.hedges.name/experiments/haversine/             
                dlon = lon2 - lon1 
                dlat = lat2 - lat1 
                a = (sin(dlat/2))^2 + cos(lat1) * cos(lat2) * (sin(dlon/2))^2 
                c = 2 * atan2( sqrt(a), sqrt(1-a) ) 
                d = R * c (where R is the radius of the Earth)
             */
            double r = 3961; //3961 miles or 6373 Km
            double a = (Math.Pow(Math.Sin(dLat / 2), 2) + (Math.Cos(latADouble) * Math.Cos(latBDouble) * Math.Pow(Math.Sin(dLng / 2), 2)));
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double d = r * c;

            

            return d;
            //return Math.Sqrt(difLat*difLat + difLng*difLng);
        }

        public double distanceBetweenLatsSimple(decimal latA, decimal lngA, decimal latB, decimal lngB)
        {
            double difLat = Convert.ToDouble(latA - latB);
            double difLng = Convert.ToDouble(lngA - lngB);


            return Math.Sqrt(difLat * difLat + difLng * difLng);
        }

        public Station GetNearestStation(decimal lat, decimal lng)
        {
            //decimal latMin = -180;
            //decimal latMax = 180;
            //decimal lngMin = -180;
            //decimal lngMax = 180;

            decimal latMin = lat - .3m;
            decimal latMax = lat + .3m;
            decimal lngMin = lng - .3m;
            decimal lngMax = lng + .3m;

            IEnumerable<Station> stationsInRange = from s in db.Stations
                                                   where s.Lat > latMin && s.Lat < latMax
                                                   where s.Lng > lngMin && s.Lng < lngMax
                                                   select s;

            Dictionary<double, Station> distanceAndStations = new Dictionary<double, Station>();

            foreach (Station s in stationsInRange)
            {
                distanceAndStations.Add(distanceBetweenLatsSimple(lat, lng, s.Lat, s.Lng), s);
            }

            Station top = (from item in distanceAndStations
                                           orderby item.Key ascending
                                           select item.Value).FirstOrDefault();


            return top;
        }

        public IEnumerable<Station> GetStationsWithinRadiusMiles(decimal lat, decimal lng, double radius)
        {
            /*
            SELECT id, ( 3959 * acos( cos( radians(37) ) * cos( radians( lat ) ) 
            * cos( radians( lng ) - radians(-122) ) + sin( radians(37) ) * sin(radians(lat)) ) ) AS distance 
            FROM markers 
            HAVING distance < 25 
            ORDER BY distance 
            LIMIT 0 , 20;
            */
            double latD = (double)lat;
            double lngD = (double)lng;

            var result = from outer in
                             (from s in db.Stations
                              select new
                              {
                                  Distance = (3959 * SqlFunctions.Acos(SqlFunctions.Cos(SqlFunctions.Radians(lat)) * SqlFunctions.Cos(SqlFunctions.Radians(s.Lat)) * SqlFunctions.Cos(SqlFunctions.Radians(s.Lng) - SqlFunctions.Radians(lng)) + SqlFunctions.Sin(SqlFunctions.Radians(lat)) * SqlFunctions.Sin((SqlFunctions.Radians(s.Lat))))),
                                  s

                              })
                         where outer.Distance < radius
                         select outer.s;                                                 

            return result;
        }

        public dynamic GetNearestStationsAndDistance(decimal lat, decimal lng)
        {
            IEnumerable<Station> stationsInRange = from s in db.Stations
                                                   where s.Lat > -180 && s.Lat < 180
                                                   where s.Lng > -180 && s.Lng < 180
                                                   select s;

            List<Tuple<double, Station>> distanceAndStations = new List<Tuple<double, Station>>();

            foreach (Station s in stationsInRange)
            {
                distanceAndStations.Add(new Tuple<double, Station>(distanceBetweenLats(lat, lng, s.Lat, s.Lng), s));
            }

            dynamic topTen = (from item in distanceAndStations
                                           orderby item.Item1 ascending
                                           select item).Take(10);


            return topTen;
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
            db.Configuration.AutoDetectChangesEnabled = false;

            string stationId = dataSet[0].Station.Id;
            Station dataSetStation = db.Stations.Find(stationId);

            //IEnumerable<DataPoint> existingDataPointsList = GetDataPointsFromStation(stationId);
            IEnumerable<DataPoint> existingDataPointsList = GetDataPointsFromStationAfterTimeUtc(stationId, DateTime.UtcNow.AddHours(-2));
            DataPointComparer comparer = new DataPointComparer();
            HashSet<DataPoint> exisitingDataPoints = new HashSet<DataPoint>(comparer);
            Dictionary<string, DataPoint> latestDataPointsForEachParameter = new Dictionary<string, DataPoint>();
            List<DataPoint> addingDataPoints = new List<DataPoint>();

            DataPoint outPoint;
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
                    addingDataPoints.Add(point);
                    exisitingDataPoints.Add(point);
                }
            }

            DataPoint largestPoint = null;
            int largestAQI = 0;

            foreach (DataPoint p in latestDataPointsForEachParameter.Values)
            {
                if (p.AQI > largestAQI)
                {
                    largestAQI = p.AQI;
                    largestPoint = p;
                }
            }
            db.Configuration.AutoDetectChangesEnabled = true;
            
            dataSetStation.Indoor = latestPoint.Indoor;
            dataSetStation.Lat = latestPoint.Lat;
            dataSetStation.Lng = latestPoint.Lng;
            if (largestPoint != null)
            {
                dataSetStation.AQI = largestPoint.AQI;
                dataSetStation.Parameter = largestPoint.Parameter;
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
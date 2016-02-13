using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using server_api.Models;
using System.IO;
using System.Xml.Serialization;
using System.Globalization;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace server_api.Controllers
{
    /// <summary>
    /// The API Controller that interacts with the Web App
    /// </summary>
    public class FrontEndController : ApiController
    {
        private AuthRepository _auth_repo = null;
        private AirUDBCOE _db_repo = null;

        public FrontEndController()
        {
            _auth_repo = new AuthRepository();
            _db_repo = new AirUDBCOE();
        }

        /*
         * + = Working as expected
         * - = Needs work
         * 
         * // Testing Methods (DONE)
         * + ServerTest() - This method simply returns successful.
         * + ServerAndDatabaseTest() - This method performs a database query and returns result.
         * 
         * // Map View (DONE)
         * + GetAllDataPointsForDevice() - Returns all datapoints for a Station given a DeviceID. (Line Chart)
         * + GetLatestDataFromSingleAMSDevice() - Returns the latest datapoints for a single AMS station based on specified DeviceID. (Details Panel)
         * + GetAllDevicesInMapRange() -  Returns the AMS DeviceStates for all AMS devices within specified MapParameters. (Map)
         * 
         * // Heat Map View (DONE)
         * + GetAllDevicesInMapRange() -  Returns the AMS DeviceStates for all AMS devices within specified MapParameters. (Map)
         * + GetLatestValuesForSpecifiedPollutantInMapRange - Returns the values of the pollutants within the specified map range. (HeatMap Layer)
         * 
         * // Station Compare View (DONE)
         * + GetAllDataPointsForDevice() - Returns all datapoints for a Station given a DeviceID. (Compare Chart)
         * 
         * // Station Registration View (DONE)
         * + DeviceRegistration() - Registers an AMS station.
         * 
         * // Station Settings View
         * + GetUsersDeviceStates() - Returns the set of DeviceStates associated with the given user email.
         * - UpdateDeviceState() - Updates a single AMS StationState from the "my devices" settings web page.
         *   - Need output format
         * 
         * // User Registration View (DONE)
         * + UserRegistration() - Validates user is not already in database and if not, creates new User in database.
         * 
         * // User Login View (DONE)
         * + UserLogin() - Validates user based on Email and Pass.
         */

        /// <summary>
        ///   This is a testing method. 
        ///   
        ///   This method simply returns successful.
        /// </summary>
        /// <returns></returns>
        [Route("frontend/servertest")]
        [HttpGet]
        public IHttpActionResult ServerTest()
        {
            return BadRequest("Yikes!");
            //return Ok("Success");
        }

        static SwaggerAQIData cacheAQI = null;

        /// <summary>
        /// Response Example:
        /// {
        ///    "DateObserved": "2015-12-06 ",
        ///    "HourObserved": 17,
        ///    "LocalTimeZone": "MST",
        ///    "ReportingArea": "Salt Lake City",
        ///    "StateCode": "UT",
        ///    "Latitude": 40.777,
        ///    "Longitude": -111.93,
        ///    "ParameterName": "O3",
        ///    "AQI": 17,
        ///    "Category": {
        ///        "Number": 1,
        ///        "Name": "Good"
        /// }
        //}
        /// </summary>
        /// <returns></returns>
        [ResponseType(typeof(SwaggerAQIData))]
        [Route("frontend/aqi")]
        [HttpGet]
        public IHttpActionResult GetAQI()
        {
            HttpWebRequest request = WebRequest.Create("http://www.airnowapi.org/aq/observation/zipCode/current/?format=application/json&zipCode=84102&distance=25&API_KEY=1CD19983-D26A-46F2-8022-6A6E16A991F7") as HttpWebRequest;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                string jsonString = reader.ReadToEnd();

                var json = JsonConvert.DeserializeObject<SwaggerAQIData[]>(jsonString);

                dynamic aqiData = json;

                int returnIndex = 0;
                int maxIndex = 0;
                int maxAQI = 0;
                foreach (dynamic returnElement in aqiData)
                {
                    int tempAQI = returnElement.AQI;
                    if (tempAQI > maxAQI)
                    {
                        maxAQI = tempAQI;
                        returnIndex = maxIndex;
                    }

                    maxIndex++;
                }
        
                cacheAQI = json[returnIndex];
                
                return Ok(cacheAQI);
            }
            else if (cacheAQI != null)
            {
                return Ok(cacheAQI);
            }
            else
            {
                return NotFound();
            }
        }

        static SwaggerDAQData[] dataArray = new SwaggerDAQData[11];
        static DateTime cacheDateTimeStamp = new DateTime();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerDAQData>))]
        [Route("frontend/daq")]
        [HttpGet]
        public IHttpActionResult GetSingleDAQStationData()
        {
            string[] apiUrls = new string[] 
                { "http://air.utah.gov/xmlFeed.php?id=boxelder",   // "Box Elder County"
                  "http://air.utah.gov/xmlFeed.php?id=cache",      // "Cache County"
                  "http://air.utah.gov/xmlFeed.php?id=p2",         // Carbon/"Price"
                  "http://air.utah.gov/xmlFeed.php?id=bv",         // "Davis County"
                  "http://air.utah.gov/xmlFeed.php?id=rs",         // "Duchesne County"
                  "http://air.utah.gov/xmlFeed.php?id=slc",        // "Salt Lake County"
                  "http://air.utah.gov/xmlFeed.php?id=tooele",     // "Tooele County"
                  "http://air.utah.gov/xmlFeed.php?id=v4",         // "Uintah County"
                  "http://air.utah.gov/xmlFeed.php?id=utah",       // "Utah County"
                  "http://air.utah.gov/xmlFeed.php?id=washington", // "Washington County"
                  "http://air.utah.gov/xmlFeed.php?id=weber"       // "Weber County"
                };

            Tuple<double, double>[] gpsLocations = new Tuple<double, double>[] 
            { new Tuple<double, double>(41.510544, -112.014640), 
              new Tuple<double, double>(41.737159, -111.836706),
              new Tuple<double, double>(39.598401, -110.811250),
              new Tuple<double, double>(40.979952, -111.887608),
              new Tuple<double, double>(40.163389, -110.402936),
              new Tuple<double, double>(40.734280, -111.871593), 
              new Tuple<double, double>(40.530786, -112.298464),
              new Tuple<double, double>(40.455679, -109.528717),
              new Tuple<double, double>(40.296847, -111.695003),
              new Tuple<double, double>(37.096288, -113.568486),
              new Tuple<double, double>(41.222803, -111.973789) };

            int timeDiffMinutes = (DateTime.Now.TimeOfDay - cacheDateTimeStamp.TimeOfDay).Minutes;

            if (dataArray[0] == null || timeDiffMinutes > 35)
            {
                for (int i = 0; i < apiUrls.Length; i++)
                {
                    HttpWebRequest request = WebRequest.Create(apiUrls[i]) as HttpWebRequest;
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    Stream stream = response.GetResponseStream();
                    XmlSerializer serializer = new XmlSerializer(typeof(SwaggerDAQData));
                    StreamReader reader = new StreamReader(stream);
                    SwaggerDAQData data = (SwaggerDAQData)serializer.Deserialize(reader);

                    data.site.latitude = gpsLocations[i].Item1;
                    data.site.longitude = gpsLocations[i].Item2;

                    for (int j = 0; j < data.site.data.Length; j++)
                    {
                        DateTime wrongDateTime = DateTime.ParseExact(data.site.data[j].date, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime correctDateTime = wrongDateTime.AddHours(1);
                        data.site.data[j].date = correctDateTime.ToString("MM/dd/yyyy HH:mm:ss");
                    }

  
                    cacheDateTimeStamp = DateTime.ParseExact(data.site.data[0].date, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);


                    dataArray[i] = data;  
                }
            }

            return Ok(dataArray);
        }

        static Dictionary<string, List<SwaggerPollutantList>> swaggerDAQDataDictCache = new Dictionary<string, List<SwaggerPollutantList>>();
        static Dictionary<string, string> apiUrlDict = new Dictionary<string, string>();
        static Dictionary<string, DateTime> cacheDateTimeStampDict = new Dictionary<string, DateTime>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [Route("frontend/daqChart")]
        [HttpPost]
        public IHttpActionResult GetDAQChartData([FromBody]string name)
        {
            if(swaggerDAQDataDictCache.Count == 0)
            {
                swaggerDAQDataDictCache.Add("Box Elder County", null);
                swaggerDAQDataDictCache.Add("Cache County", null);
                swaggerDAQDataDictCache.Add("Price", null);
                swaggerDAQDataDictCache.Add("Davis County", null);
                swaggerDAQDataDictCache.Add("Duchesne County", null);
                swaggerDAQDataDictCache.Add("Salt Lake County", null);
                swaggerDAQDataDictCache.Add("Tooele County", null);
                swaggerDAQDataDictCache.Add("Uintah County", null);
                swaggerDAQDataDictCache.Add("Utah County", null);
                swaggerDAQDataDictCache.Add("Washington County", null);
                swaggerDAQDataDictCache.Add("Weber County", null);

                cacheDateTimeStampDict.Add("Box Elder County", new DateTime());
                cacheDateTimeStampDict.Add("Cache County", new DateTime());
                cacheDateTimeStampDict.Add("Price", new DateTime());
                cacheDateTimeStampDict.Add("Davis County", new DateTime());
                cacheDateTimeStampDict.Add("Duchesne County", new DateTime());
                cacheDateTimeStampDict.Add("Salt Lake County", new DateTime());
                cacheDateTimeStampDict.Add("Tooele County", new DateTime());
                cacheDateTimeStampDict.Add("Uintah County", new DateTime());
                cacheDateTimeStampDict.Add("Utah County", new DateTime());
                cacheDateTimeStampDict.Add("Washington County", new DateTime());
                cacheDateTimeStampDict.Add("Weber County", new DateTime());

                apiUrlDict.Add("Box Elder County", "http://air.utah.gov/xmlFeed.php?id=boxelder");
                apiUrlDict.Add("Cache County", "http://air.utah.gov/xmlFeed.php?id=cache");
                apiUrlDict.Add("Price", "http://air.utah.gov/xmlFeed.php?id=p2");
                apiUrlDict.Add("Davis County", "http://air.utah.gov/xmlFeed.php?id=bv");
                apiUrlDict.Add("Duchesne County", "http://air.utah.gov/xmlFeed.php?id=rs");
                apiUrlDict.Add("Salt Lake County", "http://air.utah.gov/xmlFeed.php?id=slc");
                apiUrlDict.Add("Tooele County", "http://air.utah.gov/xmlFeed.php?id=tooele");
                apiUrlDict.Add("Uintah County", "http://air.utah.gov/xmlFeed.php?id=v4");
                apiUrlDict.Add("Utah County", "http://air.utah.gov/xmlFeed.php?id=utah");
                apiUrlDict.Add("Washington County", "http://air.utah.gov/xmlFeed.php?id=washington");
                apiUrlDict.Add("Weber County", "http://air.utah.gov/xmlFeed.php?id=weber");
            }

            List<SwaggerPollutantList> currentData = swaggerDAQDataDictCache[name];
            DateTime currentDate = cacheDateTimeStampDict[name];

            int timeDiffMinutes = (DateTime.Now.TimeOfDay - currentDate.TimeOfDay).Minutes;

            if (currentData == null || timeDiffMinutes > 35)
            {
                string url = apiUrlDict[name];

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream stream = response.GetResponseStream();
                XmlSerializer serializer = new XmlSerializer(typeof(SwaggerDAQData));
                StreamReader reader = new StreamReader(stream);
                SwaggerDAQData data = (SwaggerDAQData)serializer.Deserialize(reader);

                List<SwaggerPollutantList> pollutantDataList = new List<SwaggerPollutantList>();

                List<string> dates = new List<string>();
                SwaggerPollutantList ozone = new SwaggerPollutantList("Ozone ppm");
                SwaggerPollutantList pm25 = new SwaggerPollutantList("PM 2.5 ug/m^3");
                SwaggerPollutantList no2 = new SwaggerPollutantList("NO2 ppm");
                SwaggerPollutantList temperature = new SwaggerPollutantList("Temperature F");
                SwaggerPollutantList co = new SwaggerPollutantList("CO ppm");

                foreach (var dataSet in data.site.data)
                {
                    DateTime wrongDateTime = DateTime.ParseExact(dataSet.date, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    DateTime correctDateTime = wrongDateTime.AddHours(1);

                    long dateMilliseconds = ConvertDateTimeToMilliseconds(correctDateTime);

                    if (dataSet.ozone != "")
                    {
                        ozone.values.Add(new object[2]);
                        ozone.values.Last()[0] = dateMilliseconds;
                        ozone.values.Last()[1] = Decimal.Parse(dataSet.ozone);
                    }
                    else
                    {
                        ozone.values.Add(new object[2]);
                        ozone.values.Last()[0] = dateMilliseconds;
                        ozone.values.Last()[1] = 0.0;
                    }

                    if (dataSet.pm25 != "")
                    {
                        pm25.values.Add(new object[2]);
                        pm25.values.Last()[0] = dateMilliseconds;
                        pm25.values.Last()[1] = Decimal.Parse(dataSet.pm25);
                    }
                    else
                    {
                        pm25.values.Add(new object[2]);
                        pm25.values.Last()[0] = dateMilliseconds;
                        pm25.values.Last()[1] = 0.0;
                    }

                    if (dataSet.no2 != "")
                    {
                        no2.values.Add(new object[2]);
                        no2.values.Last()[0] = dateMilliseconds;
                        no2.values.Last()[1] = Decimal.Parse(dataSet.no2);
                    }
                    else
                    {
                        no2.values.Add(new object[2]);
                        no2.values.Last()[0] = dateMilliseconds;
                        no2.values.Last()[1] = 0.0;
                    }

                    if (dataSet.temperature != "")
                    {
                        temperature.values.Add(new object[2]);
                        temperature.values.Last()[0] = dateMilliseconds;
                        temperature.values.Last()[1] = Decimal.Parse(dataSet.temperature);
                    }
                    else
                    {
                        temperature.values.Add(new object[2]);
                        temperature.values.Last()[0] = dateMilliseconds;
                        temperature.values.Last()[1] = 0.0;
                    }

                    if (dataSet.co != "")
                    {
                        co.values.Add(new object[2]);
                        co.values.Last()[0] = dateMilliseconds;
                        co.values.Last()[1] = Decimal.Parse(dataSet.co);
                    }
                    else
                    {
                        co.values.Add(new object[2]);
                        co.values.Last()[0] = dateMilliseconds;
                        co.values.Last()[1] = 0.0;
                    }
                }

                if (ozone.values.Count != 0)
                {
                    pollutantDataList.Add(ozone);
                }

                if (pm25.values.Count != 0)
                {
                    pollutantDataList.Add(pm25);
                }

                if (no2.values.Count != 0)
                {
                    pollutantDataList.Add(no2);
                }

                if (temperature.values.Count != 0)
                {
                    pollutantDataList.Add(temperature);
                }

                if (co.values.Count != 0)
                {
                    pollutantDataList.Add(co);
                }

                cacheDateTimeStampDict[name] = DateTime.ParseExact(data.site.data[0].date, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                swaggerDAQDataDictCache[name] = pollutantDataList;
            }

            return Ok(swaggerDAQDataDictCache[name]);
        }

        /// <summary>
        ///   This is a testing method. 
        ///   
        ///   This method simply returns successful.
        /// </summary>
        /// <returns></returns>
        [Route("frontend/servertestpost")]
        [HttpPost]
        public HttpResponseMessage ServerTestPost([FromBody]string name)
        {
            var message = Request.CreateResponse(HttpStatusCode.OK);
            message.Content = new StringContent("Success: " + name);
            return message;
        }

        /// <summary>
        ///   This is a testing method. 
        ///   
        ///   This method returns all registered users' email addresses.
        /// </summary>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerUsers>))]
        [Route("frontend/registeredUsers")]
        [HttpGet]
        public IHttpActionResult ServerAndDatabaseTest()
        {
            var db = new AirUDBCOE();

            List<User> allUsers = db.Users.Select(x => x).ToList<User>();

            if(allUsers != null)
            {
                List<SwaggerUsers> swaggerUsers = new List<SwaggerUsers>();

                foreach (var item in allUsers)
                {
                    swaggerUsers.Add(new SwaggerUsers(item.Email));
                }

                return Ok(swaggerUsers);
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        ///   Returns all datapoints for a Station given a DeviceID.
        /// 
        ///   Primary Use: Compare View and single AMS station Map View "data graph"
        /// </summary>
        /// <param name="deviceID"></param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerPollutantList>))]
        [Route("frontend/deviceDatapoints")]
        [HttpPost]
        public IHttpActionResult GetAllDataPointsForDevice([FromBody]string deviceID)
        {
            //var db = new AirUDBCOE();

            //Station existingStation = db.Stations.SingleOrDefault(x => x.Id == deviceID);

            //if (existingStation != null)
            //{
            //    List<SwaggerPollutantList> data = new List<SwaggerPollutantList>();

            //    foreach (Parameter parameter in existingStation.Parameters){
            //        if (parameter.Name.Equals("Altitude")){
            //            SwaggerPollutantList pl = new SwaggerPollutantList(parameter.Name);
            //            foreach (DataPoint datapoint in parameter.DataPoints){
                        
                            
            //                pl.values.Add(new object[2]);
            //                pl.values.Last()[0] = ConvertDateTimeToMilliseconds(datapoint.Time);
            //                pl.values.Last()[1] = (decimal)datapoint.Value;
            //            }
            //            data.Add(pl);
            //        }
            //    }

            //    return Ok(data);
            //}
            //else
            //{
            //    // Account register failed. Account with email address: '<user.Email>' already exists. Please try a different email address.
            //    return BadRequest("Station with ID: " + deviceID + " does not exist. Please try a different Station ID.");
            //}


            return Ok();
        }

        /// <summary>
        /// Registers an AMS station:
        /// - Validates request
        /// - Updates Database to represent new association between existing user and 
        ///    new station.
        /// </summary>
        /// <param name="newDeviceState">The current Station and its StationState</param>
        /// <returns></returns>
        [ResponseType(typeof(SwaggerDeviceState))]
        [Route("frontend/registerUserDevice")]
        [HttpPost]
        public IHttpActionResult RegisterUserDevice([FromBody]JObject jsonData)
        {
            var db = new AirUDBCOE();

            /*Register Station
            {
	            "station": {
		            "Name": "Draper",
		            "ID": "123",
		            "Agency": "EPA",
		            "Purpose": "Bad Stuff"
	            },
	            "user": {
		            "Email": "zacharyisaiahlobato@gmail.com"
	            }
            }
            */
            
            dynamic newDeviceState = jsonData;

            JObject userJObj = newDeviceState.user;
            JObject stationJObj = newDeviceState.station;

            User user = userJObj.ToObject<User>();
            Station station = stationJObj.ToObject<Station>();

            // IdentityUser existingUser = await _auth_repo.FindUser("lobato", "burritos");

            Station existingDevice = db.Stations.SingleOrDefault(x => x.Id == station.Id);
            User existingUser = db.Users.SingleOrDefault(x => x.Email == user.Email);

            if (existingDevice == null)
            {
                // Add station success.
                station.User = existingUser;
                db.Stations.Add(station);
                db.SaveChanges();

                return Ok(newDeviceState);
            }
            else
            {
                // Add station fail.
                return BadRequest("Existing Station");
            }
        }
        
        /// <summary>
        ///   Returns the AMS DeviceStates for all AMS devices within specified MapParameters.
        ///   
        ///   Primary Use: Populate the Map View with AMS station icons. 
        /// </summary>
        /// <param name="para">The NE and SW bounds of a map</param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerAMSList>))]
        [Route("frontend/map")]
        [HttpPost]
        public IHttpActionResult GetAllDevicesInMapRange([FromBody]GpsBounds para)
        {
            // SHOULD BE VARIABLE
            decimal latMin = para.latMin;
            decimal latMax = para.latMax;
            decimal lngMin = para.longMin;
            decimal lngMax = para.longMax;

            var db = new AirUDBCOE();


            var results = from point in db.DataPoints
                          where
                          point.Lat > latMin
                          && point.Lat < latMax
                          && point.Lng > lngMin
                          && point.Lng < lngMax
                          && point.Indoor == false
                          group point by point.Station.Id into stationPoints
                          select new
                          {
                              StationID = stationPoints.Key,
                              point = stationPoints.OrderByDescending(a => a.Time).FirstOrDefault()
                          };

            SwaggerAMSList amses = new SwaggerAMSList();

            foreach (var result in results)
            {
                amses.AddSwaggerDevice(result.StationID, result.point.Lat, result.point.Lng);
            }
            
            return Ok(amses);
        }
        
        /// <summary>
        ///   Returns the values of the pollutants within the specified map range.
        ///   
        ///   Primary Use: Populate the HeatMap View with values for a specified pollutant.
        /// </summary>
        /// <param name="para">The NE and SW bounds of a map and name of requested Pollutant</param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerHeatMapValueList>))]
        [Route("frontend/heatmap")]
        [HttpPost]
        public IHttpActionResult GetLatestValuesForSpecifiedPollutantInMapRange([FromBody]SwaggerHeatMapParameters para)
        {
            // DEFAULT VALUES
            DateTime measurementTimeMax = DateTime.Now;
            int inOrOut = 0; // 0 = Outside, 1 = Inside
            int statePrivacy = 0; // 0 = Not Private, 1 = Private

            // SHOULD BE VARIABLE
            decimal latMin = para.mapParameters.southWest.lat;
            decimal latMax = para.mapParameters.northEast.lat;
            decimal longMin = para.mapParameters.southWest.lng;
            decimal longMax = para.mapParameters.northEast.lng;
            string pollutantName = para.pollutantName;

            // CURRENTLY NOT USED
            /*
            String deviceID = "12-34-56-78-9A-BC";
            DateTime measurementTimeMin = measurementTimeMax.AddHours(-12);
            */
            
            // TODO: database connection should not be manual 
            SqlConnection conn = new SqlConnection(@"Data Source=mssql.eng.utah.edu;Initial Catalog=air;Persist Security Info=True;User ID=lobato;Password=eVHDpynh;MultipleActiveResultSets=True;Application Name=EntityFramework");
            SwaggerHeatMapValueList pollutantCoordinatesAndValues = new SwaggerHeatMapValueList(pollutantName);

            using (SqlConnection myConnection = conn)
            {
                string oString = @"select Devices_States_and_DataPoints.DeviceID,
		                            Devices_States_and_DataPoints.StateTime,
		                            Devices_States_and_DataPoints.MeasurementTime,
		                            Devices_States_and_DataPoints.Lat,
		                            Devices_States_and_DataPoints.Lng,
		                            Devices_States_and_DataPoints.InOrOut,
		                            Devices_States_and_DataPoints.Privacy,
		                            Devices_States_and_DataPoints.Value,
		                            Devices_States_and_DataPoints.PollutantName
                            from(select DeviceID, Max(MeasurementTime) as MaxMeasurementTime, PollutantName
	                            from (select MaxStates.DeviceID, MaxStates.MaxStateTime, MeasurementTime, PollutantName
			                            from (select DeviceID, Max(StateTime) as MaxStateTime
					                            from DeviceStates
												where Lat > @latMin
												and Lat < @latMax
												and Lng > @longMin
												and Lng < @longMax
												and Privacy=@statePrivacy
												and InOrOut=@inOrOut
					                            group by DeviceID) as MaxStates
			                            left join Devices_States_and_DataPoints
			                            on MaxStates.DeviceID = Devices_States_and_DataPoints.DeviceID
			                            and MaxStates.MaxStateTime = Devices_States_and_DataPoints.StateTime) as MaxStatesAndMeasurementTime
										where PollutantName=@pollutantName
	                            group by DeviceID, PollutantName) as MaxMeasurementTimeForPollutants
                            left join Devices_States_and_DataPoints
			                            on MaxMeasurementTimeForPollutants.DeviceID = Devices_States_and_DataPoints.DeviceID
			                            and MaxMeasurementTimeForPollutants.PollutantName = Devices_States_and_DataPoints.PollutantName
			                            and MaxMeasurementTimeForPollutants.MaxMeasurementTime = Devices_States_and_DataPoints.MeasurementTime
										order by DeviceID;";
                SqlCommand oCmd = new SqlCommand(oString, myConnection);

                oCmd.Parameters.AddWithValue("@latMin", latMin);
                oCmd.Parameters.AddWithValue("@latMax", latMax);
                oCmd.Parameters.AddWithValue("@longMin", longMin);
                oCmd.Parameters.AddWithValue("@longMax", longMax);
                oCmd.Parameters.AddWithValue("@inOrOut", inOrOut);
                oCmd.Parameters.AddWithValue("@statePrivacy", statePrivacy);
                oCmd.Parameters.AddWithValue("@pollutantName", pollutantName);

                myConnection.Open();
                using (SqlDataReader oReader = oCmd.ExecuteReader())
                {
                    while (oReader.Read())
                    {
                        pollutantCoordinatesAndValues.AddSwaggerCoordinateAndValue((decimal)oReader["Lat"], (decimal)oReader["Lng"], (double)oReader["Value"]);
                    }

                    myConnection.Close();
                }
            }

            return Ok(pollutantCoordinatesAndValues);
        }

        /// <summary>
        ///   Returns the latest datapoints for a single AMS station based on specified DeviceID. 
        ///   
        ///   Primary Use: "details" panel on Map View after selecting AMS station on map. 
        /// </summary>
        /// <param name="deviceID"></param>
        /// <returns></returns>
        [ResponseType(typeof(SwaggerLatestDataPoints))]
        [Route("frontend/singleLatest")]
        [HttpPost]
        public IHttpActionResult GetLatestDataFromSingleAMSDevice([FromBody]string deviceID)
        {
            var db = new AirUDBCOE();

            // Validate DeviceID represents an actual AMS station.
            Station registeredDevice = db.Stations.SingleOrDefault(x => x.Id == deviceID);
            if (registeredDevice != null)
            {
                // Performs database query to obtain the latest Datapoints for specific DeviceID.
                SqlConnection conn = new SqlConnection(@"Data Source=mssql.eng.utah.edu;Initial Catalog=lobato;Persist Security Info=True;User ID=lobato;Password=eVHDpynh;MultipleActiveResultSets=True;Application Name=EntityFramework");
                SwaggerLatestPollutantsList latestPollutants = new SwaggerLatestPollutantsList();
                SwaggerLatestDataPoints latest = new SwaggerLatestDataPoints();
                using (SqlConnection myConnection = conn)
                {
                    string oString =   @"select Devices_States_and_DataPoints.DeviceID,
		                                        Devices_States_and_DataPoints.StateTime,
		                                        Devices_States_and_DataPoints.MeasurementTime,
		                                        Devices_States_and_DataPoints.Lat,
		                                        Devices_States_and_DataPoints.Lng,
		                                        Devices_States_and_DataPoints.InOrOut,
		                                        Devices_States_and_DataPoints.Privacy,
		                                        Devices_States_and_DataPoints.Value,
		                                        Devices_States_and_DataPoints.PollutantName
                                        from(select DeviceID, Max(MeasurementTime) as MaxMeasurementTime, PollutantName
	                                        from (select MaxStates.DeviceID, MaxStates.MaxStateTime, MeasurementTime, PollutantName
			                                        from (select DeviceID, Max(StateTime) as MaxStateTime
					                                        from DeviceStates
					                                        where DeviceID=@deviceID
					                                        group by DeviceID) as MaxStates
			                                        left join Devices_States_and_DataPoints
			                                        on MaxStates.DeviceID = Devices_States_and_DataPoints.DeviceID
			                                        and MaxStates.MaxStateTime = Devices_States_and_DataPoints.StateTime) as MaxStatesAndMeasurementTime
	                                        group by DeviceID, PollutantName) as MaxMeasurementTimeForPollutants
                                        left join Devices_States_and_DataPoints
			                                        on MaxMeasurementTimeForPollutants.DeviceID = Devices_States_and_DataPoints.DeviceID
			                                        and MaxMeasurementTimeForPollutants.PollutantName = Devices_States_and_DataPoints.PollutantName
			                                        and MaxMeasurementTimeForPollutants.MaxMeasurementTime = Devices_States_and_DataPoints.MeasurementTime";
                    SqlCommand oCmd = new SqlCommand(oString, myConnection);
                    oCmd.Parameters.AddWithValue("@deviceID", deviceID);
                    
                    myConnection.Open();
                    using (SqlDataReader oReader = oCmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            latestPollutants.AddPollutantAndValue(oReader["PollutantName"].ToString(), (double)oReader["Value"]);
                        }     

                        foreach (var item in latestPollutants.latest)
                        {
                            switch (item.pollutantName)
                            {
                                case "Altitude":
                                    latest.altitude = item.value.ToString();
                                    break;

                                case "CO":
                                    latest.co = item.value.ToString();
                                    break;

                                case "CO2":
                                    latest.co2 = item.value.ToString();
                                    break;

                                case "Humidity":
                                    latest.humidity = item.value.ToString();
                                    break;

                                case "NO2":
                                    latest.no2 = item.value.ToString();
                                    break;

                                case "PM":
                                    latest.pm = item.value.ToString();
                                    break;

                                case "Pressure":
                                    latest.pressure = item.value.ToString();
                                    break;

                                case "Temperature":
                                    latest.temp = item.value.ToString();
                                    break;

                                case "O3":
                                    latest.o3 = item.value.ToString();
                                    break;
                            }
                        }
                        myConnection.Close();
                    }
                }
                return Ok(latest);
            }
            else
            {
                // Station with DeviceID: <deviceID> does not exist.
                return NotFound();
            }
        }

        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        [Route("ams")]
        [HttpPut]
        public void Put(int id, [FromBody]string value)
        {

        }
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        [Route("frontend/devices/{id}")]
        [HttpDelete]
        public IHttpActionResult Delete(string id)
        {
            AirUDBCOE db = new AirUDBCOE();

            // Validate Station from given DeviceId exists.
            Station registeredDevice = db.Stations.SingleOrDefault(x => x.Id == id);

            if (registeredDevice != null)
            {
                Station toDelete = (from dev in db.Stations
                                   where dev.Id == id
                                   select dev).Single();

                db.Stations.Remove(toDelete);
                db.SaveChanges();

                return Ok("Delete Successful");
            }
            else
            {
                // Station with DeviceID: <deviceID> does not exist.
                return NotFound();
            }
        }


        /// <summary>
        /// Converts DateTime to compatible JS time in Milliseconds
        /// </summary>
        /// <param name="date">the date to be converted</param>
        /// <returns>date in milliseconds since January 1st, 1970</returns>
        public static long ConvertDateTimeToMilliseconds(DateTime date)
        {
            return (long)(date - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
    }
}

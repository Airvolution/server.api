using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AirnowRetrieval
{
    class Program
    {
        static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        static void Main(string[] args)
        {
            string currentPath = Directory.GetCurrentDirectory();
            string logPath = currentPath + "\\" + "airNowApiLog.txt";
            string stationDictionaryPath = currentPath + "\\" + "station_dictionary.txt";

            if (!File.Exists(logPath))
            {
                File.Create(logPath);
            }

            if (!File.Exists(stationDictionaryPath))
            {
                File.Create(stationDictionaryPath);
            }

            // Log the time.
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(logPath, true))
            {
                file.WriteLine("Time: " + DateTime.Now.ToString());
            }

            // Get data from airnowapi.org.
            AirNowDataPoint[] data = GetAirNowApiDataPoints();

            foreach(var dataPoint in data)
            {
                if (!SetAirUDataPoint(dataPoint, logPath))
                {
                    Console.WriteLine("Sending datapoints to AirU Failed.");
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(logPath, true))
                    {
                        file.WriteLine("Sending datapoints to AirU Failed.");
                    }
                }
            }

            Console.Write("");
        }

        public static AirNowDataPoint[] GetAirNowApiDataPoints()
        {
            // DataPoints to be returned.
            AirNowDataPoint[] airNowApiData = null;

            // Getting time.
            DateTime now = DateTime.UtcNow;
            DateTime endDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, 0); // year, month, day, hour, minute, second, milisecond
            DateTime startDate = endDate.AddHours(-1);

            // Prepared string.
            string startDateString = startDate.ToString("yyyy-MM-dd") + "T" + startDate.ToString("HH");
            string endDateString = endDate.ToString("yyyy-MM-dd") + "T" + endDate.ToString("HH");
            string parameters = "O3,PM25,PM10,CO,NO2,SO2";
            string BBOX = "-131.591187,23.993461,-55.653687,51.472612";  // America
            string dataType = "B";
            string format = "application/json";
            string verbose = "1";
            string API_KEY = "1CD19983-D26A-46F2-8022-6A6E16A991F7";

            // WebClient performing the GET Request from AirNowApi.
            WebClient airNowApiWebClient = new WebClient();

            // GET Url.
            string getUrl = "http://www.airnowapi.org/aq/data";

            airNowApiWebClient.BaseAddress = getUrl;

            // Append GET URL parameters.
            airNowApiWebClient.QueryString.Add("startDate", startDateString);
            airNowApiWebClient.QueryString.Add("endDate", endDateString);
            airNowApiWebClient.QueryString.Add("parameters", parameters);
            airNowApiWebClient.QueryString.Add("BBOX", BBOX);
            airNowApiWebClient.QueryString.Add("dataType", dataType);
            airNowApiWebClient.QueryString.Add("format", format);
            airNowApiWebClient.QueryString.Add("verbose", verbose);
            airNowApiWebClient.QueryString.Add("API_KEY", API_KEY);

            string json = "";

            Console.WriteLine("Requesting data from AirNowApi.org");

            try
            {
                json = airNowApiWebClient.DownloadString(getUrl);

                airNowApiData = JsonConvert.DeserializeObject<AirNowDataPoint[]>(json);

                //airNowResponse = "200";
            }
            catch (WebException e)
            {
                //airNowResponse = e.Status.ToString();
            }

            return airNowApiData;
        }

        public static bool SetAirUDataPoint(AirNowDataPoint dataPoint, string logPath)
        {
            // WebClient performing the POST to airu.
            WebClient airuApiWebClient = new WebClient();

            // POST Url.
            string postUrl = "http://dev.air.eng.utah.edu/api/stations/data";

            string datePattern = "yyyy-MM-ddTHH:mm";

            var item = new DataPoint
            {
                Time = DateTime.ParseExact(dataPoint.UTC, datePattern, CultureInfo.InvariantCulture),
                Station = new Station
                {
                    Id = dataPoint.SiteName.GetHashCode().ToString()
                },
                Parameter = new Parameter
                {
                    Name = dataPoint.Parameter,
                    Unit = dataPoint.Unit
                },
                Indoor = false,
                Lat = dataPoint.Latitude,
                Lng = dataPoint.Longitude,
                Value = dataPoint.Value,
                Category = dataPoint.Category,
                AQI = dataPoint.AQI
            };

            DataPoint[] tempDataPoint = new DataPoint[1];

            tempDataPoint[0] = item;

            string dataPointJson = "";

            try
            {
                var http = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
                http.ContentType = "application/json";
                http.Method = "POST";

                dataPointJson = JsonConvert.SerializeObject(tempDataPoint, jsonSettings);
                UTF8Encoding encoding = new UTF8Encoding();
                Byte[] bytes = encoding.GetBytes(dataPointJson);

                http.ContentLength = bytes.Length;

                Stream newStream = http.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();

                using (HttpWebResponse response = (HttpWebResponse)http.GetResponse())
                {
                    Console.WriteLine("DataPoint add response code: " + response.StatusCode.ToString());
                }

                Console.WriteLine("Datapoint added.");
            }
            catch (WebException e)
            {
                Console.WriteLine("Failed to add datapoint. Exception status: " + e.Status.ToString());

                // Possible missing.
                // Attempt to add.
                if(!SetAirUStation(dataPoint, logPath))
                {
                    // Failed to add station.
                }
                else
                {
                    // After successfully adding missing station, attempt to add datapoint.
                    SetAirUDataPoint(dataPoint, logPath);
                }
            }

            return true;
        }

        public static bool SetAirUStation(AirNowDataPoint dataPoint, string logPath)
        {
            // Look up geo information from google.
            string lat = dataPoint.Latitude.ToString();
            string lng = dataPoint.Longitude.ToString();
            GoogleGeo geoInfo = GetReverseGeolocationLookUp(lat, lng, logPath);

            string url = "http://dev.air.eng.utah.edu/api/stations/register";

            Station newStation = new Station
            {
                Name = dataPoint.SiteName,
                Id = dataPoint.SiteName.GetHashCode().ToString(),
                Agency = dataPoint.AgencyName,
                Purpose = dataPoint.AgencyName + ": " + dataPoint.SiteName + " data retrieved from AirNow.gov.",
                Indoor = false,
                City = geoInfo.city,
                State = geoInfo.state,
                Postal = geoInfo.postal
            };

            User newUser = new User
            {
                Email = "epa_stations"
            };

            StationUser stationUser = new StationUser
            {
                station = newStation,
                user = newUser
            };

            string stationJson = "";

            try
            {
                var http = (HttpWebRequest)WebRequest.Create(new Uri(url));
                http.ContentType = "application/json";
                http.Method = "POST";

                stationJson = JsonConvert.SerializeObject(stationUser, jsonSettings);
                UTF8Encoding encoding = new UTF8Encoding();
                Byte[] bytes = encoding.GetBytes(stationJson);

                http.ContentLength = bytes.Length;

                Stream newStream = http.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();

                using (HttpWebResponse response = (HttpWebResponse)http.GetResponse())
                {
                    Console.WriteLine("Station Creation response code: " + response.StatusCode.ToString());
                }

                Console.WriteLine("New Station Added.");
            }
            catch (WebException e2)
            {
                Console.WriteLine("Failed to add station: " + e2.Status.ToString());
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(logPath, true))
                {
                    file.WriteLine("Failed to add station: \n" + stationJson);
                }
                return false;
            }

            return true;
        }

        public static GoogleGeo GetReverseGeolocationLookUp(string lat, string lng, string logPath)
        {
            // WebClient performing the GET Request from AirNowApi.
            WebClient googleApiWebClient = new WebClient();

            // GET Url.
            string getUrl = "https://maps.googleapis.com/maps/api/geocode/json";

            // Prepare strings.
            string latLng = lat + "," + lng;
            //string latLng = "40.758839,-111.855112"; // my house
            string googleApiKey = "AIzaSyBTZfapJs0edgaklFGhC3c9DcDt_Kg92VI";

            googleApiWebClient.BaseAddress = getUrl;

            // Append GET URL parameters.
            googleApiWebClient.QueryString.Add("latlng", latLng);
            googleApiWebClient.QueryString.Add("key", googleApiKey);

            string json = "";

            Console.WriteLine("Requesting google geo lookup.");

            GoogleGeo result = null;

            try
            {
                json = googleApiWebClient.DownloadString(getUrl);

                string tmpCity = "";
                string tmpState = "";
                string tmpPostalCode = "";

                dynamic dynamicJson = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                foreach(dynamic item in dynamicJson.results[0].address_components)
                {
                    string type = item.types[0];
                    switch (type)
                    {
                        case "locality":
                            tmpCity = item.long_name;
                            break;
                        case "administrative_area_level_1":
                            tmpState = item.short_name;
                            break;
                        case "postal_code":
                            tmpPostalCode = item.long_name;
                            break;
                    }
                }

                result = new GoogleGeo
                {
                    city = tmpCity,
                    state = tmpState,
                    postal = tmpPostalCode
                };

                Console.WriteLine("Obtained google geo lookup.");
            }
            catch (WebException e)
            {
                Console.WriteLine("Google lookup failed.");

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(logPath, true))
                {
                    file.WriteLine("Google lookup failed: " + "\n" + json);
                }
            }

            return result;
        }
    }
}

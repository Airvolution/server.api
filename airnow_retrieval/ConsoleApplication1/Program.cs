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
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace AirnowRetrieval
{
    class Blah
    {
        public void Example()
        {
            Console.Out.WriteLine("HI");
        }
    }

    class Program
    {
        static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        private static Object thisLock = new Object();
        static string logPath = null;
        static string hostUrl = "http://localhost:2307/";
        //static string hostURL = "http://dev.air.eng.utah/edu/api/";

        static void Main(string[] args)
        {
            string currentPath = Directory.GetCurrentDirectory();
            logPath = currentPath + "\\" + "airNowApiLog.txt";
            //string logPath = "C:\\dev\\airnow_retrieval\\ConsoleApplication1\\bin\\Debug\\" + "airNowApiLog.txt";
            //string stationDictionaryPath = currentPath + "\\" + "station_dictionary.txt";

            if (!File.Exists(logPath))
            {
                File.Create(logPath);
            }

            //if (!File.Exists(stationDictionaryPath))
            //{
            //    File.Create(stationDictionaryPath);
            //}

            // Log the time.
            Log("Start Time");

            // Get data from airnowapi.org.
            Dictionary<string, List<AirNowDataPoint>> dataDictionary = GetAirNowApiDataPoints();
            int tamper = 0;
            foreach(List<AirNowDataPoint> stationPoints in dataDictionary.Values)
            {
                
                Thread newThread = new Thread(() => SetAirUDataPoint(stationPoints));
                Console.WriteLine("Starting new thread...");
                newThread.Start();
                tamper += 1;
                //if (tamper > 15)
                //{
                    //newThread.Join();
                    //Console.WriteLine("Threads joined...");
                    //tamper = 0;
                //}               

                //if (!SetAirUDataPoint(stationPoints))
                //{
                //    Console.WriteLine("Sending datapoints to AirU Failed.");
                //    Log("Sending DataPoints to AirU - FAILED");
                //}
            }

            Log("End Time" );
        }

        private static void Log(String msg)
        {
            lock (thisLock)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(logPath, true))
                {
                    file.WriteLine(DateTime.UtcNow.ToString()+ ": "+msg);
                    file.Dispose();
                }
            }
        }

        public static Dictionary<string, List<AirNowDataPoint>> GetAirNowApiDataPoints()
        {
            Dictionary<string, List<AirNowDataPoint>> optimizedPoints = new Dictionary<string, List<AirNowDataPoint>>();

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


            // Slight Optimization
            foreach (AirNowDataPoint dataPoint in airNowApiData)
            {
                if (optimizedPoints.ContainsKey(dataPoint.SiteName)){
                    List<AirNowDataPoint> empty = null;
                    optimizedPoints.TryGetValue(dataPoint.SiteName, out empty);
                    empty.Add(dataPoint);
                }
                else{
                    List<AirNowDataPoint> newList = new List<AirNowDataPoint>();
                    newList.Add(dataPoint);
                    optimizedPoints.Add(dataPoint.SiteName, newList);
                }
            }

            return optimizedPoints;
        }

        public static bool SetAirUDataPoint(List<AirNowDataPoint> airNowDataPoints)
        {
            // WebClient performing the POST to airu.
            WebClient airuApiWebClient = new WebClient();


            string datePattern = "yyyy-MM-ddTHH:mm";

            List<DataPoint>tempDataPoints = new List<DataPoint>();

            foreach (AirNowDataPoint airNowDataPoint in airNowDataPoints){

                DataPoint item = new DataPoint
                {
                    Time = DateTime.ParseExact(airNowDataPoint.UTC, datePattern, CultureInfo.InvariantCulture),
                    Station = new Station
                    {
                        Id = airNowDataPoint.SiteName.GetHashCode().ToString()
                    },
                    Parameter = new Parameter
                    {
                        Name = airNowDataPoint.Parameter,
                        Unit = airNowDataPoint.Unit
                    },
                    Indoor = false,
                    Lat = airNowDataPoint.Latitude,
                    Lng = airNowDataPoint.Longitude,
                    Value = airNowDataPoint.Value,
                    Category = airNowDataPoint.Category,
                    AQI = airNowDataPoint.AQI
                };

                tempDataPoints.Add(item);

            };

            AirNowDataPoint tempNowDataPoint = airNowDataPoints.ElementAt(0);
            DataPoint tempDataPoint = tempDataPoints.ElementAt(0);

            string route = "stations/data";

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(hostUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    Task<HttpResponseMessage> responsePost = client.PostAsJsonAsync(route, tempDataPoints.ToArray());
                    if (responsePost.Result.IsSuccessStatusCode)
                    {
                        HttpResponseMessage httpMsg = responsePost.Result;
                        Task<string> content = httpMsg.Content.ReadAsStringAsync();
                        string jsonAsString = content.Result;

                        dynamic responseObject = JsonConvert.DeserializeObject(jsonAsString);
                        Console.WriteLine(httpMsg.StatusCode + ": DataPoints Added");
                        foreach (var d in responseObject)
                        {
                            Console.WriteLine("\tStationId: " + d.station.id + "\tParameterName:" + d.parameter.name + "\tParameterUnit:" + d.parameter.unit);
                        }   
                    }
                    else if (responsePost.Result.StatusCode == HttpStatusCode.BadRequest)
                    {
                        HttpResponseMessage httpMsg = responsePost.Result;
                        Task<string> content = httpMsg.Content.ReadAsStringAsync();
                        string jsonAsString = content.Result;

                        dynamic responseObject = JsonConvert.DeserializeObject(jsonAsString);

                        string errorMessage = responseObject.message;

                        switch (errorMessage)
                        {
                            case "Station does not exist.":
                                Console.Write("Attempting to Add Station at site " + tempNowDataPoint.SiteName + "...");
                                if (!SetAirUStation(tempNowDataPoint))
                                {
                                    Console.WriteLine("Failed.");
                                    Log("Failed to register station:");
                                    Log("\tStationId: " + tempDataPoint.Station.Id);
                                }
                                else
                                {
                                    Console.WriteLine("Success!");
                                    SetAirUDataPoint(airNowDataPoints);
                                }
                                break;
                            case "No DataPoints in sent array.":
                                Console.WriteLine("Empty array of DataPoints sent:");
                                Log("Empty array of DataPoints sent on StationId:" + tempDataPoint.Station.Id);
                                break;
                            default:
                                Console.WriteLine("An unexpected status code occurrred:");
                                Log("Unexpected: " + httpMsg.StatusCode + ": " + httpMsg.Content);
                                break;
                        }
                        if (errorMessage.Equals("Station does not exist."))
                        {
                            
                        }
                        else
                        {
                            Console.WriteLine("An unexpected status code occurrred:");
                            Log("Unexpected: " + httpMsg.StatusCode + ": " + httpMsg.ReasonPhrase);
                        }

                        
                    }
                    client.Dispose();
                }
            }
            catch (TaskCanceledException e)
            {
                Log("A task canceled exception occurred: " + e.Message);
            }
            catch (Exception e)
            {
                Log("An unknown exception occurred: " + e.Message);
            }
            
            return true;
        }

        public static bool SetAirUStation(AirNowDataPoint dataPoint)
        {
            // Look up geo information from google.
            string lat = dataPoint.Latitude.ToString();
            string lng = dataPoint.Longitude.ToString();
            GoogleGeo geoInfo = GetReverseGeolocationLookUp(lat, lng);

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


            string route = "stations/register";

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(hostUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    Task<HttpResponseMessage> responsePost = client.PostAsJsonAsync(route, stationUser);
                    if (responsePost.Result.IsSuccessStatusCode)
                    {
                        HttpResponseMessage httpMsg = responsePost.Result;
                        Task<string> content = httpMsg.Content.ReadAsStringAsync();
                        string jsonAsString = content.Result;

                        dynamic responseObject = JsonConvert.DeserializeObject(jsonAsString);
                        Console.WriteLine(httpMsg.StatusCode + ": Station Registered");
                        //Console.WriteLine("\tStationId: " + responseObject.id + "\tUserName:" + responseObject.user.id);

                    }
                    else if (responsePost.Result.StatusCode == HttpStatusCode.BadRequest)
                    {
                        HttpResponseMessage httpMsg = responsePost.Result;
                        Task<string> content = httpMsg.Content.ReadAsStringAsync();
                        string jsonAsString = content.Result;

                        dynamic responseObject = JsonConvert.DeserializeObject(jsonAsString);

                        string errorMessage = responseObject.message;

                        if (errorMessage.Equals("Station already exists."))
                        {
                            Console.Write("Station already exists: " + stationUser.station.Id);
                            Log("Station already exists: " + stationUser.station.Id);
                        }
                        else if (errorMessage.Equals("User does not exist."))
                        {
                            Console.Write("User does not exist: " + stationUser.user.Username);
                            Log("User does not exist: " + stationUser.user.Username);
                        }
                        else
                        {
                            Console.WriteLine("An unexpected status code occurrred:");
                            Log("Unexpected: " + httpMsg.StatusCode + ": " + httpMsg.Content);
                        }
                    }
                    client.Dispose();
                }
            }
            catch (Exception e)
            {
                Log("An unknown exception occurred: " + e.Message);
            }

            return true;
        }

        public static GoogleGeo GetReverseGeolocationLookUp(string lat, string lng)
        {
            // WebClient performing the GET Request from AirNowApi.
            WebClient googleApiWebClient = new WebClient();

            // GET Url.
            string getUrl = "https://maps.googleapis.com/maps/api/geocode/json";

            // Prepare strings.
            string latLng = lat + "," + lng;
            //string latLng = "40.758839,-111.855112"; // my house
            string googleApiKey = "AIzaSyBTZfapJs0edgaklFGhC3c9DcDt_Kg92VI";
            //string googleApiKey = "AIzaSyDeA2Hn7XRCY3dgq4y_upM4HW6rQkSZSUo";

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

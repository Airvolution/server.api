using AirStoreToDB.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using server_api.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AirStoreToDB
{
    class Program
    {
        private static Object thisLock = new Object();
        static string logDirectory = "C:\\dev\\airnow_retrieval\\log\\";
        static string logFileName = "AirStoreToDB.txt";
        static string invalidStationsFileName = "InvalidStations.txt";
        static string oAuthToken;


        static string hostUrl = "http://localhost:40321/";
        //static string hostUrl = "http://dev.air.eng.utah.edu/api/";

        static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        static void Main(string[] args)
        {
            LogInvalidStation("TEST");


            string backupFolder = "\\..\\..\\..\\AirNowSaveData\\AirNow Backup Directory";
            DirectoryInfo backupDirectory = GetBackupDirectoryIfExists(backupFolder);

            // Login

            string loginRoute = "users/login";
            oAuthToken = Login(loginRoute, "AirNow", "AirNowAdmin123");
            

            // Check that the directory exists
            if (backupDirectory != null)
            {
                HashSet<string> currentRegisteredStations = new HashSet<string>();
                HashSet<string> unregisteredStations = new HashSet<string>();
                HashSet<AirNowDataPoint> unregisteredStationsData = new HashSet<AirNowDataPoint>();

                // Get set of currentRegisteredStations in DB to check later
                string routeForStationLocations = "stations/locations?latMin=-91&latMax=91&lngMin=-181&lngMax=181";
                currentRegisteredStations = GetExistingStationsFromDB(routeForStationLocations);

                // read in invalid stations
                ExtractInvalidStations(logDirectory + invalidStationsFileName, currentRegisteredStations);
             
                if (currentRegisteredStations == null)
                {
                    Log("Could not locate existing stations.");
                    Log("Exiting AirStoreToDB.");
                    return;
                }

                // Go through each file in the directory and add datapoints into our dictionary
                Dictionary<string, List<AirNowDataPoint>> optimizedPoints = new Dictionary<string, List<AirNowDataPoint>>();
                foreach (FileInfo fi in backupDirectory.GetFiles())
                {
                    // If data has not already been extracted
                        // Extract Data
                        Log("Extracting: " + fi.FullName);
                        ExtractDataFromNewFilesInDictionary(fi.FullName, optimizedPoints, currentRegisteredStations, unregisteredStations, unregisteredStationsData);
                }

                string routeForPingingStations = "stations/ping";
                string routeForRegisteringStations = "stations/register";
                string routeForGettingGoogleGeo = "https://maps.googleapis.com/maps/api/geocode/json";

                // Go through each unregisteredStation
                foreach (AirNowDataPoint stationInfo in unregisteredStationsData)
                {
                    // Get Geo Information
                    GoogleGeo geoInfo = GetGoogleInfo(routeForGettingGoogleGeo, stationInfo.Latitude, stationInfo.Longitude, stationInfo.IntlAQSCode);

                    if (geoInfo != null)
                    {
                        // Ping the station to the UnregisterdStations table
                        if (PingStationIDToUnregisteredTable(routeForPingingStations, stationInfo.IntlAQSCode))
                        {                                               
                            // Register the station
                            RegisterStation(routeForRegisteringStations, stationInfo, geoInfo);
                        }                        
                    }                                        
                }
                    
                // Go through each station in the optimized points and add all of its points
                foreach (List<AirNowDataPoint> singleStationDataPoints in optimizedPoints.Values)
                {
                    // Add new datapoints
                }


                Console.ReadLine();

            }
            else
            {
                Log("Files not found.");
            }

            Console.ReadLine();
        }

        private static GoogleGeo GetGoogleInfo(string route, double lat, double lng, string stationId)
        {
            // WebClient performing the GET Request from AirNowApi.
            WebClient googleApiWebClient = new WebClient();

            // GET Url.
            string getUrl = route;

            // Prepare strings.
            string latLng = lat + "," + lng;
            string googleApiKey = "AIzaSyBTZfapJs0edgaklFGhC3c9DcDt_Kg92VI";

            googleApiWebClient.BaseAddress = getUrl;

            // Append GET URL parameters.
            googleApiWebClient.QueryString.Add("latlng", latLng);
            googleApiWebClient.QueryString.Add("key", googleApiKey);

            string json = "";

            Log("Requesting google geo lookup.");

            GoogleGeo result = null;

            try
            {
                json = googleApiWebClient.DownloadString(getUrl);

                string tmpCity = "";
                string tmpState = "";
                string tmpPostalCode = "";
                string tmpCountry = "";

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
                        case "country":
                            tmpCountry = item.long_name;
                            break;
                    }
                }

                if (!tmpCountry.Equals("United States"))
                {
                    Log("Google Geo returned country not in the United States: " + tmpCountry);
                    LogInvalidStation(stationId);
                    return null;
                }

                result = new GoogleGeo
                {
                    city = tmpCity,
                    state = tmpState,
                    postal = tmpPostalCode
                };

                if (result.city == null || result.postal == null || result.state == null ||
                    result.city.Equals("") || result.postal.Equals("") || result.state.Equals(""))
                {
                    Log("Google Geo returned null or empty values.");
                    LogInvalidStation(stationId);
                    return null;
                }
                    

                Log("Obtained google geo lookup.");
            }
            catch (WebException e)
            {
                Log("Google lookup failed.");
            }

            return result;        
        }

        private static string Login(string route, string email, string password)
        {
            // /api/users/login
            // 'grant_type=password&username=' + loginData.userName + '&password=' + loginData.password;

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(hostUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

                    //client.PostAsync(route,,,)

                    //var formatter = new JsonMediaTypeFormatter();
                    //formatter.SerializerSettings = new JsonSerializerSettings
                    //{
                    //    Formatting = Formatting.Indented,
                    //    ContractResolver = new CamelCasePropertyNamesContractResolver()
                    //};                    

                    ////string data = "grant_type=password&email=" + email + "&password=" + password;

                    //var formatter = new FormUrlEncodedMediaTypeFormatter();
                    var formContent = new FormUrlEncodedContent(new[]{
                        new KeyValuePair<string, string>("grant_type", "password"),
                        new KeyValuePair<string, string>("password", password),
                        new KeyValuePair<string, string>("username", email)
                    });

                    string hi = formContent.ToString();

                    Task<HttpResponseMessage> responsePost = client.PostAsync(route, formContent);

                    if (responsePost.Result.IsSuccessStatusCode)
                    {
                        HttpResponseMessage httpMsg = responsePost.Result;

                        Task<string> content = httpMsg.Content.ReadAsStringAsync();
                        string jsonAsString = content.Result;

                        dynamic responseObject = JsonConvert.DeserializeObject(jsonAsString, jsonSettings);
                        Console.WriteLine(httpMsg.StatusCode + ": Login successful");

                        return responseObject.access_token;
                    }
                    else
                    {
                        Log("Error: Unable to ping unregistered station!");
                        return null;
                    }
                }
            }
            catch (TaskCanceledException e)
            {
                Log("A task canceled exception occurred: " + e.Message);
                return null;
            }
            catch (Exception e)
            {
                Log("An unknown exception occurred in SetAirUDataPoint: " + e.Message);
                return null;
            }

        }

        private static void RegisterStation(string route, AirNowDataPoint stationInfo, GoogleGeo geoInfo)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(hostUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", oAuthToken);

                    //client.PostAsync(route,,,)

                    var formatter = new JsonMediaTypeFormatter();
                    formatter.SerializerSettings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };

                    Station unregisteredStation = new Station
                    {
                        Name = stationInfo.SiteName,
                        Id = stationInfo.IntlAQSCode,
                        Agency = stationInfo.AgencyName,
                        Purpose = stationInfo.AgencyName + ": " + stationInfo.SiteName + " data retrieved from AirNow.gov.",
                        Indoor = false,
                        City = geoInfo.city,
                        State = geoInfo.state,
                        Postal = geoInfo.postal,
                        Type = "AirNow"
                    };

                    Task<HttpResponseMessage> responsePost = client.PostAsync(route, unregisteredStation, formatter);

                    if (responsePost.Result.IsSuccessStatusCode)
                    {
                        HttpResponseMessage httpMsg = responsePost.Result;
                        Console.WriteLine(httpMsg.StatusCode + ": Unregisterd station pinged");
                        return;
                    }
                    else
                    {
                        Log("Error: Unable to ping unregistered station!");
                        return;
                    }
                }
            }
            catch (TaskCanceledException e)
            {
                Log("A task canceled exception occurred: " + e.Message);
                return;
            }
            catch (Exception e)
            {
                Log("An unknown exception occurred in SetAirUDataPoint: " + e.Message);
                return;
            }
        }

        private static bool PingStationIDToUnregisteredTable(string route, string stationId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(hostUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    //client.PostAsync(route,,,)

                    var formatter = new JsonMediaTypeFormatter();
                    formatter.SerializerSettings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };

                    UnregisteredStation unregisteredStation = new UnregisteredStation();
                    unregisteredStation.Id = stationId;

                    Task<HttpResponseMessage> responsePost = client.PostAsync(route, unregisteredStation, formatter);

                    if (responsePost.Result.IsSuccessStatusCode)
                    {
                        HttpResponseMessage httpMsg = responsePost.Result;
                        Console.WriteLine(httpMsg.StatusCode + ": Unregisterd station pinged");
                        return true;
                    }
                    else
                    {
                        Log("Error: Unable to ping unregistered station!");
                        return false;
                    }
                }
            }
            catch (TaskCanceledException e)
            {
                Log("A task canceled exception occurred: " + e.Message);
                return false;
            }
            catch (Exception e)
            {
                Log("An unknown exception occurred in SetAirUDataPoint: " + e.Message);
                return false;
            }
        }

        private static HashSet<string> GetExistingStationsFromDB(string route)
        {
            try
            {
                using (var client = new HttpClient())
                {                    
                    client.BaseAddress = new Uri(hostUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    //client.PostAsync(route,,,)

                    var formatter = new JsonMediaTypeFormatter();
                    formatter.SerializerSettings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };

                    Task<HttpResponseMessage> responseGet = client.GetAsync(route);

                    if (responseGet.Result.IsSuccessStatusCode)
                    {                        
                        HttpResponseMessage httpMsg = responseGet.Result;
                        Task<string> content = httpMsg.Content.ReadAsStringAsync();
                        string jsonAsString = content.Result;

                        dynamic responseObject = JsonConvert.DeserializeObject(jsonAsString, jsonSettings);
                        Console.WriteLine(httpMsg.StatusCode + ": DataPoints Added");

                        HashSet<string> existingStations = new HashSet<string>();
                        
                        foreach (var station in responseObject)
                        {
                            Console.WriteLine("\tStationId: " + station.id.Value);
                            existingStations.Add(station.id.Value);
                        }
                        return existingStations;
                    }
                    else
                    {
                        Log("Error: Unable to download existing stations.");
                        return null;
                    }
                }
            }
            catch (TaskCanceledException e)
            {
                Log("A task canceled exception occurred: " + e.Message);
                return null;
            }
            catch (Exception e)
            {
                Log("An unknown exception occurred in SetAirUDataPoint: " + e.Message);
                return null;
            }
        }

        private static void ExtractInvalidStations(string fileName, HashSet<string> currentRegisteredStations)
        {
            // Read in file
            string fileContents = ReadInFile(fileName);

            if (fileContents != null)
            {
                // Parse the file
                string[] stations = fileContents.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

                foreach (string station in stations)
                {
                    currentRegisteredStations.Add(station);
                }

            }
            else
            {
                Log("Failed to read in file: " + fileName);
            }
        }

        private static void ExtractDataFromNewFilesInDictionary(string fileName, 
                                                                Dictionary<string, List<AirNowDataPoint>> dataPointsByStation,
                                                                HashSet<string> currentRegisteredStations,
                                                                HashSet<string> unregisteredStations,
                                                                HashSet<AirNowDataPoint> unregisteredStationsData)
        {
            // Read in file
            string json = ReadInFile(fileName);

            if (json != null)
            {
                // Parse the file
                AirNowDataPoint[] dataPoints = JsonConvert.DeserializeObject<AirNowDataPoint[]>(json);                                                
                
                // Combining calls from the same stations into a single call
                foreach (AirNowDataPoint dataPoint in dataPoints)
                {
                    if (!currentRegisteredStations.Contains(dataPoint.IntlAQSCode))
                    {
                        if (!unregisteredStations.Contains(dataPoint.IntlAQSCode))
                        {
                            unregisteredStations.Add(dataPoint.IntlAQSCode);
                            unregisteredStationsData.Add(dataPoint);
                        }                        
                    }
                    
                    if (dataPointsByStation.ContainsKey(dataPoint.IntlAQSCode))
                    {
                        List<AirNowDataPoint> empty = null;
                        dataPointsByStation.TryGetValue(dataPoint.IntlAQSCode, out empty);
                        empty.Add(dataPoint);
                    }
                    else
                    {
                        List<AirNowDataPoint> newList = new List<AirNowDataPoint>();
                        newList.Add(dataPoint);
                        dataPointsByStation.Add(dataPoint.IntlAQSCode, newList);
                    }
                }

            }
            else
            {
                Log("Failed to Upload: " + fileName);
            }            
        }

        private static string ReadInFile(string fileName)
        {
            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(fileName))
                {
                    // Read the stream to a string, and write the string to the console.
                    String json = sr.ReadToEnd();
                    return json;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                return null;
            }
        }


        /* HELPER STATIC METHODS */

        private static DirectoryInfo GetBackupDirectoryIfExists(string backupFolder)
        {
            DirectoryInfo projectDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

            // Look up directory containing backup files
            Console.WriteLine(projectDirectory.FullName + backupFolder);

            if (Directory.Exists(projectDirectory.FullName + backupFolder))
            {
                Log("Backup directory (" + projectDirectory.FullName + ") found.");
                return new DirectoryInfo(projectDirectory.FullName + backupFolder);
            }
            else
            {
                Log("Backup directory (" + projectDirectory.FullName + ") does not exist.");
                return null;
            }
        }

        /// <summary>
        /// This function writes the string (msg) that is passed into the argument into a file at the 
        /// location of the logDirectory.
        /// </summary>
        /// <param name="msg">A message to log</param>
        private static void Log(String msg)
        {
            Console.WriteLine(msg);

            lock (thisLock)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(logDirectory + logFileName, true))
                {
                    file.WriteLine(DateTime.UtcNow.ToString() + ": " + msg);
                    file.Dispose();
                }
            }
        }


        /// <summary>
        /// This function writes the string (msg) that is passed into the argument into a file at the 
        /// location of the logDirectory.
        /// </summary>
        /// <param name="stationId">A message to log</param>
        private static void LogInvalidStation(String stationId)
        {
            Console.WriteLine(stationId);

            lock (thisLock)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(logDirectory + invalidStationsFileName, true))
                {
                    file.WriteLine(stationId);
                    file.Dispose();
                }
            }
        }
    }
}

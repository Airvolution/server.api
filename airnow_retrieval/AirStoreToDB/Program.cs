using AirStoreToDB.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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


        static string hostUrlLocal = "http://localhost:2307/";
        static string hostUrl = "http://dev.air.eng.utah.edu/api/";

        static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        static void Main(string[] args)
        {
            string backupFolder = "\\..\\..\\..\\AirNowSaveData\\AirNow Backup Directory";

            DirectoryInfo backupDirectory = GetBackupDirectoryIfExists(backupFolder);
            

            // Check that the directory exists
            if (backupDirectory != null)
            {
                HashSet<string> currentRegisteredStations = new HashSet<string>();
                HashSet<string> unregisteredStations = new HashSet<string>();
                HashSet<AirNowDataPoint> unregisteredStationsData = new HashSet<AirNowDataPoint>();

                // Get set of currentRegisteredStations in DB to check later
                string route = "stations/locations?latMin=-91&latMax=91&lngMin=-181&lngMax=181";
                currentRegisteredStations = GetExistingStationsFromDB(route);
             
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

                // Go through each unregisteredStation
                foreach (AirNowDataPoint stationInfo in unregisteredStationsData)
                {
                    string routes = "hi";
                    PingStationIDToUnregisteredTable(routes, stationInfo.FullAQSCode);
                    // Ping the station to the UnregisterdStations table
                    // Register the station
                }
                    

                Console.ReadLine();

            }
            else
            {
                Log("Files not found.");
            }

            Console.ReadLine();
        }

        private static void PingStationIDToUnregisteredTable(string route, string stationId)
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
                        return;
                    }
                    else
                    {
                        Log("Error: Unable to download existing stations.");
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
                    if (!currentRegisteredStations.Contains(dataPoint.FullAQSCode))
                    {
                        if (!unregisteredStations.Contains(dataPoint.FullAQSCode))
                        {
                            unregisteredStations.Add(dataPoint.FullAQSCode);
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
    }
}

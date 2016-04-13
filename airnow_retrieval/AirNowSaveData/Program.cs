using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace AirNowSaveData
{
    class Options
    {
        public DateTime StartDate;
        public DateTime EndDate;

        public Options(DateTime startDate, DateTime endDate)
        {
            this.StartDate = startDate;
            this.EndDate = endDate;
        }

        public static Options ParseArguments(string[] args)
        {
            if (args.Length == 0)
            {
                return new Options(DateTime.UtcNow.AddHours(-3), DateTime.UtcNow);
            }
                

            String pattern = @"(\d\d\d\d)[-:]?(\d\d)?[-:]?(\d\d)?[-:]?(\d\d)?[-:]?(\d\d)?[-:]?(\d\d)?";
            Regex rgx = new Regex(pattern);

            DateTime startDate = DateTime.UtcNow;
            DateTime endDate = DateTime.UtcNow;

            if (args.Length == 1)
            {
                // Parse Start Date
                Match matchStart = rgx.Match(args[0]);
                startDate = ParseMatchToDateTime(matchStart);
            }
            else if (args.Length == 2)
            {
                Match matchStart = rgx.Match(args[0]);
                Match matchEnd = rgx.Match(args[1]);
                startDate = ParseMatchToDateTime(matchStart);
                endDate = ParseMatchToDateTime(matchEnd);                
            }

            return new Options(startDate, endDate);
        }

        /// <exception cref="ArgumentOutOfRangeException">Invalid datetime input.</exception>
        private static DateTime ParseMatchToDateTime(Match match) 
        {
            int second = 0;
            int minute = 0;
            int hour = 0;
            int day = 1;
            int month = 1;
            int year = -1;

            switch (CountNonEmptyGroups(match))
            {
                default:
                case 7:
                    Int32.TryParse(match.Groups[6].Value, out second);
                    goto case 6;
                case 6:
                    Int32.TryParse(match.Groups[5].Value, out minute);
                    goto case 5;
                case 5:
                    Int32.TryParse(match.Groups[4].Value, out hour);
                    goto case 4;
                case 4:
                    Int32.TryParse(match.Groups[3].Value, out day);
                    goto case 3;
                case 3:
                    Int32.TryParse(match.Groups[2].Value, out month);
                    goto case 2;
                case 2:
                    Int32.TryParse(match.Groups[1].Value, out year);
                    break;
                case 1:
                case 0:
                    break;
            }
            return new DateTime(year, month, day, second, hour, minute);
        }

        private static int CountNonEmptyGroups(Match match)
        {
            int count = 0;
            foreach (Group g in match.Groups)
            {
                if (g.Length > 0)
                {
                    count++;
                }
            }
            return count;
        }

    }
    class Program
    {
        private static Object thisLock = new Object();
        static string logFileName = "\\AirNowSaveData.txt";

        static DirectoryInfo dirLog;
        static DirectoryInfo dirBackup;


        static void Main(string[] args)
        {
            DirectoryInfo dirAirNowRetrieval = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent;

            dirLog = Directory.CreateDirectory(dirAirNowRetrieval.FullName + "\\AirNowSaveData\\log");
            dirBackup = Directory.CreateDirectory(dirAirNowRetrieval.FullName + "\\AirNowSaveData\\AirNow Backup Directory"); 
            
            Log("Current Date: " + DateTime.UtcNow.ToString());
           
            Options options = null;

            try
            {
                options = Options.ParseArguments(args);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.Out.WriteLine("Invalid Arguments: Should be [YYYY[-MM][-DD][-HH][-MM][-SS] [YYYY[-MM][-DD][-HH][-MM][-SS]]]\n\n" +
                    "No Arguments (Collects past two hours)\n\n" + 

                    "One Argument (Start Date to Current Time):\n" +
                    "(i.e 2000) or \n" +
                    "(i.e 2000-12) or \n" +
                    "(i.e 2000-12-31) or \n" +
                    "(i.e 2000-12-31-17) or \n" +
                    "(i.e 2000-12-31-17-24) or \n" +
                    "(i.e 2000-12-31-17-24-36)\n\n" + 

                    "Two Arguments (Start Date to End Date):\n" +
                    "(i.e 2000-12-31 2016)");
                Console.ReadLine();
                return;
            }      

            DateTime startDateAndTime = options.StartDate;
            DateTime endDateAndTime = options.EndDate;


            int years = endDateAndTime.Year - startDateAndTime.Year;
            int days = endDateAndTime.DayOfYear - startDateAndTime.DayOfYear;
            int hours = endDateAndTime.Hour - startDateAndTime.Hour;

            int totalTime = (years * 365 * 24) + (days * 24) + hours;
            int offset = 3;
                        
            if (!File.Exists(dirLog.FullName + logFileName))
            {
                File.Create(dirLog.FullName + logFileName);
            }

            // For each 3 hour segment between the start date and end date
            for (int hourlyOffset = offset; hourlyOffset <= totalTime; hourlyOffset += offset)
            {
                // Save a file for output
                SaveAirNowApiDataPoints(startDateAndTime, offset, hourlyOffset);
            }            
        }        

        /// <summary>
        /// This function creates an output file with the given string msg as its contents at the
        /// location of the logDirectory
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <param name="contents">The string contents of the file</param>
        private static void CreateJsonOutputFile(String name, String contents, DateTime startDate, DateTime endDate)
        {
            string jsonFilePath = dirBackup.FullName + "\\" + startDate.Year + "-" + startDate.Month + "-" + startDate.Day + " " +
                                                        startDate.Hour + "-" + startDate.Minute + "-" + startDate.Second +
                                                        " to " +
                                                        endDate.Year + "-" + endDate.Month + "-" + endDate.Day + " " +
                                                        endDate.Hour + "-" + endDate.Minute + "-" + endDate.Second + " - "+ name +".json";
            
            lock (thisLock)
            {
                if (!File.Exists(jsonFilePath))
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(jsonFilePath, true))
                    {
                        file.Write(contents);
                        file.Dispose();
                    }
                }
                else
                {
                    // Call to method with Re-Entrant Lock (Locks within the same thread can lock the same lock as much as they want)
                    Log(jsonFilePath + " already created.");
                }

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

            // Re-Entrant Lock (Locks within the same thread can lock the same lock as much as they want)
            lock (thisLock)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(dirLog.FullName + logFileName, true))
                {
                    file.WriteLine(DateTime.UtcNow.ToString() + ": " + msg);
                    file.Dispose();
                }
            }
        }

        
        /// <summary>
        /// Pulls the data from the AirNow API and saves it to a json file
        /// </summary>
        /// <param name="startDateAndTime">The starting date and time for data collection</param>
        /// <param name="offset">The chunk size (in hours) for data to be saved</param>
        /// <param name="hourlyOffset">The offset of the starting date in time (in hours)</param>
        public static void SaveAirNowApiDataPoints(DateTime startDateAndTime, int offset, int hourlyOffset)
        {

            // Getting time.
            DateTime initial = startDateAndTime.AddHours(hourlyOffset);
            DateTime endDate = new DateTime(initial.Year, initial.Month, initial.Day, initial.Hour, 0, 0, 0); // year, month, day, hour, minute, second, milisecond
            DateTime startDate = endDate.AddHours(0 - offset);

            Log("Collecting time from: " + startDate.ToString() + " to: " + endDate.ToString());

            // Prepared string.
            string startDateString = startDate.ToString("yyyy-MM-dd") + "T" + startDate.ToString("HH");
            string endDateString = endDate.ToString("yyyy-MM-dd") + "T" + endDate.ToString("HH");
            string parameters = "O3,PM25,PM10,CO,NO2,SO2";
            string BBOX = "-131.591187,23.993461,-55.653687,51.472612";  // America
            string dataType = "B";
            string format = "application/json";
            string verbose = "1";
            //string API_KEY = "1CD19983-D26A-46F2-8022-6A6E16A991F7";
            string API_KEY = "3A7B6343-7943-4C2E-B1D6-2C3D77858D7A";

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
                CreateJsonOutputFile("48", json, startDate, endDate);
                
            }
            catch (WebException e)
            {
                Log("Could not pull data for FortyEight States");
            }

            airNowApiWebClient.QueryString.Remove("BBOX");
            BBOX = "-179.394524,12.497917,-140.722649,71.617702";  // Alaska and Hawaii
            airNowApiWebClient.QueryString.Add("BBOX", BBOX);
            json = "";

            try
            {
                json = airNowApiWebClient.DownloadString(getUrl);
                CreateJsonOutputFile("AK-HI", json, startDate, endDate);
            }
            catch (WebException e)
            {
                Log("Could not pull data for Alaska and Hawaii");
            }

            return;
        }
    }
}

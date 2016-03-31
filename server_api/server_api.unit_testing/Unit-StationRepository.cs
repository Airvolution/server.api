using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using server_api;
using System.IO;
using System.Data.Entity;
using System.Collections.Generic;
using System.Threading;
using server_api.Models;

namespace server_api.unit_testing
{
    [TestClass]
    public class UnitTestingStationRepository
    {
        private static AirUDBCOE _context;
        private static StationsRepository _repo;
        private static string connectionString;

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            var filePath = @"C:\database\temp.mdf";
            var logPath = @"C:\database\temp_log.ldf";

            if (File.Exists(filePath))
                File.Delete(filePath);
            if (File.Exists(logPath))
                File.Delete(logPath);

            /* Local Database */
            connectionString = @"Server=(LocalDB)\MSSQLLocalDB; Integrated Security=true ;AttachDbFileName=C:\database\temp.mdf";
            using (var context = new AirUDBCOE(connectionString))
            {
                context.Database.Create();
            }
            /* SQL Express Database */
            
            /* Live Database */
            //connectionString = "data source=mssql.eng.utah.edu;initial catalog=lobato;persist security info=True;user id=lobato;password=eVHDpynh;multipleactiveresultsets=True;application name=EntityFramework\" providerName=\"System.Data.SqlClient";

            _context = new AirUDBCOE(connectionString);
            _repo = new StationsRepository(_context);
            SetupDatabase();
        }

        private static void SetupDatabase()
        {
            User newUser = new User();
            newUser.ConfirmPassword = "admin";
            newUser.Password = "admin";
            newUser.FirstName = "Admin";
            newUser.Email = "admin-email";
            newUser.LastName = "master";
            newUser.UserName = "Fake-Admin";


            _context.Users.Add(newUser);
            _context.SaveChanges();

            AddStations(10, newUser); // Number of Stations
            //AddParameters(); // Parameters (currently 8)
            var task = AddDataPoints(100); // Number of Datapoints for each station
            task.Wait();
        }

        private static async System.Threading.Tasks.Task AddDataPoints(int count)
        {
            Random rand = new Random();

            List<Station> stations = await _context.Stations.ToListAsync();
            List<Parameter> parameters = await _context.Parameters.ToListAsync();


            foreach (Station station in stations)
            {
                List<DataPoint> points = new List<DataPoint>();

                decimal lat = (decimal)(rand.NextDouble() * 13 + 35.8);
                decimal lng = (decimal)(rand.NextDouble() * 44.1 - 121);

                for (int i = 0; i < count; i++)
                {
                    foreach (Parameter parameter in parameters)
                    {
                        DataPoint datapoint = new DataPoint();

                        //datapoint.Lat = lat;
                        //datapoint.Lng = lng;


                        datapoint.AQI = rand.Next(400);
                        datapoint.Category = GetHealthRiskCategory(datapoint.AQI);
                        datapoint.Indoor = (datapoint.AQI % 2 == 0) ? true : false;

                        datapoint.Parameter = parameter;
                        datapoint.Station = station;
                        datapoint.Value = DateTime.Now.Millisecond;
                        datapoint.Time = DateTime.Now.AddMinutes(i * 15);

                        points.Add(datapoint);
                    }
                }

                _repo.SetDataPointsFromStation(points.ToArray());
            }
        }

        private static void AddParameters()
        {
            List<Tuple<string, string>> parameters = new List<Tuple<string, string>>() 
            {
                /*
                Name	Unit
                CO	PPM
                NO2	PPB
                OZONE	PPB
                PM10	UG/M3
                PM2.5	UG/M3
                SO2	PPB
                 */

                new Tuple<string,string>("PM2.5", "UG/M3"),
                new Tuple<string,string>("PM10", "UG/M3"),
                new Tuple<string,string>("CO", "PPM"),
                new Tuple<string,string>("NO2", "PPB"),
                new Tuple<string,string>("OZONE", "PPB"),
                new Tuple<string,string>("SO2", "PPB"),
            };

            foreach (Tuple<string, string> t in parameters)
            {
                Parameter parameter = new Parameter();
                parameter.Name = t.Item1;
                parameter.Unit = t.Item2;

                _context.Parameters.Add(parameter);
            }

            _context.SaveChanges();
        }

        private static void AddStations(int count, User newUser)
        {
            /*
            Station oneStation = new Station();
            oneStation.Agency = "One";
            oneStation.Id = "ONE" + 0.ToString("D6");
            oneStation.Name = "Name" + 0.ToString("D6");
            oneStation.Purpose = "Testing";
            _context.Stations.Add(oneStation);
            _context.SaveChanges();
            */
            for (int i = 0; i < count; i++)
            {
                Station existingStation = new Station();
                existingStation.Id = i.ToString("D6") + "AAA";
                existingStation.Purpose = null;
                existingStation.Name = "Name" + i.ToString("D6");
                existingStation.Purpose = "Testing";
                existingStation.Indoor = false;
                //existingStation.Lat = 77m;
                //existingStation.Lng = 77m;
                existingStation.User = newUser;

                _context.Stations.Add(existingStation);
            }
            _context.SaveChanges();
        }

        private static int GetHealthRiskCategory(int AQI)
        {
            if (AQI < 40)
                return 1;
            else if (AQI < 80)
                return 2;
            else if (AQI < 120)
                return 3;
            else if (AQI < 160)
                return 4;
            else if (AQI < 200)
                return 5;
            else if (AQI < 240)
                return 6;
            else if (AQI < 280)
                return 7;
            else if (AQI < 320)
                return 8;
            else if (AQI < 360)
                return 9;
            else
                return 10;
        }


        [ClassCleanup]
        public static void ClassClean()
        {
            _context.Dispose();
        }

        [TestMethod]
        public void DatabaseExists()
        {       
            Assert.IsTrue(File.Exists(@"C:\database\temp.mdf"));
        }

        [TestMethod]
        public void StationDoesNotExist()
        {
            Assert.IsFalse(_repo.StationExists("I-Do-Not-Exist"));
        }

        [TestMethod]
        public void StationDoesExist()
        {
            Assert.IsTrue(_repo.StationExists("MAC000000"));
        }

        [TestMethod]
        public void DataPointDoesExist()
        {
            Assert.IsTrue(_context.DataPoints.Find(1)!=null);
        }
        
        [TestMethod]
        public void SetDataPoint()
        {
            
            DataPoint validDataPoint = new DataPoint();
            validDataPoint.Indoor = true;
            //validDataPoint.Lat = 123;
            //validDataPoint.Lng = 123;

            Parameter newParameter = new Parameter();
            newParameter.Name = "CO";
            newParameter.Unit = "   ";
            validDataPoint.Parameter = newParameter;

            Station newStation = new Station();
            newStation.Id = "MAC000000";
            validDataPoint.Station = newStation;

            validDataPoint.Time = DateTime.Now.AddYears(5);
            validDataPoint.Value = 25;
            validDataPoint.AQI = 234;
            validDataPoint.Category = 8;

            List<DataPoint> points = new List<DataPoint>();
            points.Add(validDataPoint);

            _repo.SetDataPointsFromStation(points.ToArray());

            IEnumerable<DataPoint> latestPoints = _repo.GetLatestDataPointsFromStation("MAC000000");

            bool isNotThere = true;

            foreach (DataPoint point in latestPoints)
            {
                if (point.Parameter.Name.Equals("CO"))
                    if (point.Value == validDataPoint.Value)
                        if (point.AQI == validDataPoint.AQI)
                            isNotThere = false;
            }

            Assert.IsFalse(isNotThere);


            Station mac000000 = _context.Stations.Find("MAC000000");

            //Assert.IsTrue(mac000000.Lat == validDataPoint.Lat);
            //Assert.IsTrue(mac000000.Lng == validDataPoint.Lng);
            Assert.IsTrue(mac000000.Indoor == validDataPoint.Indoor);
        }
    }
}

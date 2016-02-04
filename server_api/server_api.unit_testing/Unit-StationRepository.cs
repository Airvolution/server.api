using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using server_api;
using System.IO;
using System.Data.Entity;

namespace server_api.unit_testing
{
    [TestClass]
    public class UnitTestingStationRepository
    {
        private static AirUDBCOE _context;
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

            connectionString = @"Server=(LocalDB)\MSSQLLocalDB; Integrated Security=true ;AttachDbFileName=C:\database\temp.mdf";
            using (var context = new AirUDBCOE(connectionString))
            {
                context.Database.Create();
            }

            _context = new AirUDBCOE(connectionString);

            Station existingStation = new Station();
            existingStation.Agency = "Exist";
            existingStation.Id = "MAC1234";
            existingStation.Name = "Zach's Station";
            existingStation.Purpose = "To win at life";

            _context.Stations.Add(existingStation);
            _context.SaveChanges();
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
            var stationRepository = new StationsRepository(connectionString);
            Assert.IsFalse(stationRepository.StationExists("I-Do-Not-Exist"));
        }

        [TestMethod]
        public void StationDoesExist()
        {
            var stationRepository = new StationsRepository(connectionString);
            Assert.IsTrue(stationRepository.StationExists("MAC1234"));
        }
    }
}

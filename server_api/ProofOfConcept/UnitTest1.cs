/**
 * Site: http://www.codeproject.com/Articles/460175/Two-strategies-for-testing-Entity-Framework-Effort
 * 
 */

using System;
using System.Data.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using server_api;
using System.IO;
using System.Data.Entity.Infrastructure;

namespace ProofOfConcept
{
    [TestClass]
    public class UnitTest1
    {
        private static AirUDBCOE _context;


        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            // file path of the database to create
            var filePath = @"C:\database\TempDb.sdf";

            // delete it if it already exists
            if (File.Exists(filePath))
                File.Delete(filePath);

            // create the SQL CE connection string - this just points to the file path
            string connectionString = "Datasource = " + filePath;

            // NEED TO SET THIS TO MAKE DATABASE CREATION WORK WITH SQL CE!!!
            Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");

            using (var context = new AirUDBCOE(connectionString))
            {
                // this will create the database with the schema from the Entity Model
                context.Database.Create();
                //context.Database.CreateIfNotExists();
            }

            // initialize our DbContext class with the SQL CE connection string, 
            // ready for our tests to use it.
            _context = new AirUDBCOE(connectionString);
        }

        [TestMethod]
        public void RegisterNewUser()
        {
            var userRepository = new UserRepository(_context);
            Assert.IsTrue(userRepository.RegisterUser("new-user-1@user.com", "123"));
        }

        [TestMethod]
        public void RegisterExistingUser()
        {
            var userRepository = new UserRepository(_context);
            Assert.IsFalse(userRepository.RegisterUser("new-user-1@user.com", "123"));
        }
    }
}

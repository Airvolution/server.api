using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlServerCe;
using System.Linq;
using System.Web;

namespace server_api
{
    public class DBContextFactory : IDbContextFactory<AirUDBCOE>
    {
        public static Boolean DefaultDbSource = true;

        public AirUDBCOE Create()
        {
            return GetContext();
        }

        /// <summary>
        /// This is added for my convenience.
        /// </summary>
        public static AirUDBCOE GetContext()
        {
            return DefaultDbSource ? GetClientContext() : GetServerContext();
        }

        #region Private Methods

        private static AirUDBCOE GetClientContext()
        {
            /*
            // Get path
            var path = @"C:\database\TempDb.sdf";

            // Create SqlCe connection
            var sb = new SqlCeConnectionStringBuilder { DataSource = path };
            var con = new SqlCeConnection(sb.ToString());
            */
            return new AirUDBCOE("name=Local");
        }

        private static AirUDBCOE GetServerContext()
        {
            // Get connectionstring
            return new AirUDBCOE();
        }

        #endregion
    }
}
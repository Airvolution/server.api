using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using server_api.Models;

namespace server_api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class StationsController: ApiController
    {
        /// <summary>
        ///   Returns the set of DeviceStates associated with the given user email.
        /// </summary>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerDeviceState>))]
        [Route("stations/state/{email}")]
        [HttpGet]
        public IHttpActionResult StationStates([FromUri]string email)
        {
            var db = new AirUDBCOE();

            email = "jaredpotter1@gmail.com";

            // Validate given email has associated User.
            User registeredUser = db.Users.SingleOrDefault(x => x.Email == email);

            if (registeredUser != null)
            {
                SqlConnection conn = new SqlConnection(@"Data Source=mssql.eng.utah.edu;Initial Catalog=lobato;Persist Security Info=True;User ID=lobato;Password=eVHDpynh;MultipleActiveResultSets=True;Application Name=EntityFramework");
                List<SwaggerDeviceState> swaggerDeviceStates = new List<SwaggerDeviceState>();
                using (SqlConnection myConnection = conn)
                {
                    string oString = @"select MaxCompleteStates.DeviceID, Devices.Name, Devices.Purpose, MaxCompleteStates.StateTime, MaxCompleteStates.Lat, MaxCompleteStates.Long, MaxCompleteStates.InOrOut, MaxCompleteStates.StatePrivacy from
                                        (select MaxStates.DeviceID, MaxStates.StateTime, DeviceStates.Lat, DeviceStates.Long, DeviceStates.InOrOut, DeviceStates.StatePrivacy from
	                                        (select DeviceID, Max(StateTime) as StateTime
				                                        from DeviceStates
				                                        group by DeviceID) as MaxStates
		                                        left join DeviceStates
		                                        on MaxStates.DeviceID=DeviceStates.DeviceID
		                                        and MaxStates.StateTime = DeviceStates.StateTime) as MaxCompleteStates
		                                        left join Devices
		                                        on Devices.DeviceID=MaxCompleteStates.DeviceID
		                                        where Devices.Email = @owner;";
                    SqlCommand oCmd = new SqlCommand(oString, myConnection);

                    oCmd.Parameters.AddWithValue("@owner", email);

                    myConnection.Open();
                    using (SqlDataReader oReader = oCmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            swaggerDeviceStates.Add(new SwaggerDeviceState(
                                                                    (string)oReader["Name"],
                                                                    (string)oReader["DeviceID"],
                                                                    (bool)oReader["StatePrivacy"],
                                                                    (string)oReader["Purpose"],
                                                                    (bool)oReader["InOrOut"],
                                                                    (decimal)oReader["Lat"],
                                                                    (decimal)oReader["Long"],
                                                                    email));
                        }

                        myConnection.Close();
                    }
                }
                return Ok(swaggerDeviceStates);
            }
            else
            {
                // User with email address: <email> does not exist.
                return NotFound();
            }
        }

        /// <summary>
        /// Registers an AMS device:
        /// - Validates request
        /// - Updates Database to represent new association between existing user and 
        ///    new device.
        /// </summary>
        /// <param name="newDeviceState">The current Device and its DeviceState</param>
        /// <returns></returns>
        [ResponseType(typeof(SwaggerDeviceState))]
        [Route("stations/register")]
        [HttpPost]
        public IHttpActionResult RegisterStation([FromBody]SwaggerDeviceState newDeviceState) // TODO: why can't methods share the same name if they are different endpoints?
        {
            var db = new AirUDBCOE();

            Device existingDevice = db.Devices.SingleOrDefault(x => x.DeviceID == newDeviceState.Id);
            if (existingDevice == null)
            {
                // Add device success.
                Device device = new Device();
                device.Name = newDeviceState.Name;
                device.DeviceID = newDeviceState.Id;
                device.Email = "jaredpotter1@gmail.com"; // newDeviceAndState.Email;
                device.DevicePrivacy = newDeviceState.Privacy;
                device.Purpose = newDeviceState.Purpose;
                db.Devices.Add(device);
                db.SaveChanges();

                DeviceState state = new DeviceState();
                state.Device = device;
                state.DeviceID = newDeviceState.Id;
                state.InOrOut = newDeviceState.Indoor;
                state.StatePrivacy = newDeviceState.Privacy;
                state.StateTime = new DateTime(1900, 1, 1);
                state.Long = 0.0m;
                state.Lat = 90.0m;
                db.DeviceStates.Add(state);
                db.SaveChanges();

                return Ok(newDeviceState);
            }
            else
            {
                // Add device fail.
                return BadRequest("Existing Device");
            }
        }

        /// <summary>
        ///   Adds one or many DeviceStates (from a station)
        /// </summary>
        /// <param name="state">*xml comment*</param>
        /// <returns></returns>
        [Route("stations/states")]
        [HttpPost]
        public IHttpActionResult AddAMSDeviceStates([FromBody]DeviceState[] states)
        {
            var db = new AirUDBCOE();
            string deviceID = states[0].DeviceID;
            Device device = db.Devices.SingleOrDefault(x => x.DeviceID == deviceID);

            if (device == null)
            {
                // Failed to add DeviceState.
                return Ok("Failed to add device state with Device with ID = " + states[0].DeviceID + " not found.");
            }

            db.DeviceStates.AddRange(states);

            db.SaveChanges();

            // Success.
            return Ok(states);
        }

        /// <summary>
        ///   Updates a single AMS DeviceState from the "my devices" settings web page.
        /// </summary>
        /// <param name="state">The state of the device</param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerDeviceState>))]
        [Route("stations/state/{deviceId}")]
        [HttpPut]
        public IHttpActionResult Station([FromBody]SwaggerDeviceState state)
        {
            var db = new AirUDBCOE();

            // Validate Device from given DeviceId exists.
            Device registeredDevice = db.Devices.SingleOrDefault(x => x.DeviceID == state.Id);

            if (registeredDevice != null)
            {
                // Request previous state from database based on state.DeviceID
                DeviceState previousState = (
                                    from device in db.DeviceStates
                                    where device.DeviceID == state.Id
                                    && device.StateTime <= DateTime.Now // **May be a future source of contention - REVIEW**
                                    group device by device.DeviceID into deviceIDGroup
                                    select new
                                    {
                                        DeviceID = deviceIDGroup.Key,
                                        MaxMeasurementTime = deviceIDGroup.Max(device => device.StateTime)
                                    } into MaxStates
                                    join coordinates in db.DeviceStates
                                                            on MaxStates.MaxMeasurementTime equals coordinates.StateTime into latestStateGroup
                                    select latestStateGroup.FirstOrDefault()).Single();

                // Inherit lat and long from previous state

                DeviceState newDeviceState = new DeviceState();
                newDeviceState.Device = previousState.Device;
                newDeviceState.DeviceID = state.Id;
                newDeviceState.InOrOut = state.Indoor;
                newDeviceState.StatePrivacy = state.Privacy;
                newDeviceState.Lat = previousState.Lat;
                newDeviceState.Long = previousState.Long;
                newDeviceState.StateTime = DateTime.Now;
                db.DeviceStates.Add(newDeviceState);
                db.SaveChanges();

                registeredDevice.Name = state.Name;
                registeredDevice.Purpose = state.Purpose;

                //db.Devices.Add(registeredDevice);
                db.SaveChanges();

                // Send user newly updated state back to user
                return Ok(state);
            }
            else
            {
                // Device with DeviceID: <deviceID> does not exist.
                return NotFound();
            }
        }


        /// <summary>
        ///   Adds one or many DevicePoints (from a station)
        /// </summary>
        /// <param name="dataSet">AMSDataSet Model</param>
        /// <returns></returns>
        [Route("stations/data")]
        [HttpPost]
        public IHttpActionResult AddAMSDataSet([FromBody]DataPoint[] dataSet)
        {
            var db = new AirUDBCOE();

            db.DataPoints.AddRange(dataSet);

            db.SaveChanges();

            return Ok(dataSet);
        }


        /// <summary>
        ///   Returns the station locators based on the given coordinates.
        ///   
        ///   Primary Use: Populate the Map View with AMS device icons. 
        /// </summary>
        /// <param name="latMin"></param>
        /// <param name="latMax"></param>
        /// <param name="longMin"></param>
        /// <param name="longMax"></param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerAMSList>))]
        [Route("stations/locators")] // TODO: properly configure the URL to specify lat/long min/max
        [HttpGet]
        public IHttpActionResult StationLocators([FromUri]GpsBounds bounds)
        {
            // SHOULD BE VARIABLE
            decimal latMin = bounds.latMin;
            decimal latMax = bounds.latMax;
            decimal longMin = bounds.longMin;
            decimal longMax = bounds.longMax;

            var db = new AirUDBCOE();

            var results = from state in db.DeviceStates
                          where
                          state.Lat > latMin
                          && state.Lat < latMax
                          && state.Long > longMin
                          && state.Long < longMax
                          && state.StatePrivacy == false // Can create add in Spring
                          && state.InOrOut == false // Can create add in Spring
                          group state by state.DeviceID into deviceIDGroup
                          select new
                          {
                              MaxStateTime = deviceIDGroup.Max(device => device.StateTime)
                          } into MaxStates
                          join coordinates in db.DeviceStates
                          on MaxStates.MaxStateTime equals coordinates.StateTime into latestStateGroup
                          select latestStateGroup.FirstOrDefault();

            SwaggerAMSList amses = new SwaggerAMSList();

            foreach (DeviceState d in results)
            {
                amses.AddSwaggerDevice(d.DeviceID, d.Lat, d.Long);
            }

            return Ok(amses);
        }

        /// <summary>
        ///   Returns all datapoints for a Device given a DeviceID.
        /// 
        ///   Primary Use: Compare View and single AMS device Map View "data graph"
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerPollutantList>))]
        [Route("stations/datapoints/{deviceID}")]
        [HttpGet]
        public IHttpActionResult DataPoints([FromUri]int deviceId)
        {
            var db = new AirUDBCOE();

            Device existingDevice = db.Devices.SingleOrDefault(x => x.DeviceID == deviceId.ToString());

            if (existingDevice != null)
            {

                List<Pollutant> pollutants = db.Pollutants.Select(x => x).ToList<Pollutant>();

                List<SwaggerPollutantList> data = new List<SwaggerPollutantList>();

                StringBuilder msg = new StringBuilder();

                foreach (Pollutant p in pollutants)
                {
                    var amsDataForPollutant = from a in db.Devices_States_and_Datapoints
                                              where a.DeviceID == deviceId.ToString()
                                              && a.PollutantName == p.PollutantName
                                              orderby a.MeasurementTime
                                              select a;

                    /* MOVE ALTITUDE TO STATE */
                    if (amsDataForPollutant.Count() != 0 && !p.PollutantName.Equals("Altitude"))
                    {
                        SwaggerPollutantList pl = new SwaggerPollutantList(p.PollutantName);

                        foreach (var item in amsDataForPollutant)
                        {
                            pl.values.Add(new object[2]);
                            pl.values.Last()[0] = ConvertDateTimeToMilliseconds(item.MeasurementTime);
                            pl.values.Last()[1] = (decimal)item.Value;
                        }
                        data.Add(pl);
                    }
                }

                return Ok(data);
            }
            else
            {
                // Account register failed. Account with email address: '<user.Email>' already exists. Please try a different email address.
                return BadRequest("Device with ID: " + deviceId + " does not exist. Please try a different Device ID.");
            }
        }

        /// <summary>
        ///   Returns the latest datapoints for a single AMS device based on specified DeviceId. 
        ///   
        ///   Primary Use: "details" panel on Map View after selecting AMS device on map. 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [ResponseType(typeof(SwaggerLatestDataPoints))]
        [Route("stations/latestDataPoint/{deviceId}")]
        [HttpGet]
        public IHttpActionResult LatestDataPoint([FromUri]int deviceId)
        {
            var db = new AirUDBCOE();

            // Validate DeviceID represents an actual AMS device.
            Device registeredDevice = db.Devices.SingleOrDefault(x => x.DeviceID == deviceId.ToString());
            if (registeredDevice != null)
            {
                // Performs database query to obtain the latest Datapoints for specific DeviceID.
                SqlConnection conn = new SqlConnection(@"Data Source=mssql.eng.utah.edu;Initial Catalog=lobato;Persist Security Info=True;User ID=lobato;Password=eVHDpynh;MultipleActiveResultSets=True;Application Name=EntityFramework");
                SwaggerLatestPollutantsList latestPollutants = new SwaggerLatestPollutantsList();
                SwaggerLatestDataPoints latest = new SwaggerLatestDataPoints();
                using (SqlConnection myConnection = conn)
                {
                    string oString = @"select Devices_States_and_DataPoints.DeviceID,
		                                        Devices_States_and_DataPoints.StateTime,
		                                        Devices_States_and_DataPoints.MeasurementTime,
		                                        Devices_States_and_DataPoints.Lat,
		                                        Devices_States_and_DataPoints.Long,
		                                        Devices_States_and_DataPoints.InOrOut,
		                                        Devices_States_and_DataPoints.StatePrivacy,
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
                    oCmd.Parameters.AddWithValue("@deviceID", deviceId);

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
                // Device with DeviceID: <deviceID> does not exist.
                return NotFound();
            }
        }

        /// <summary>
        ///   Deletes the selected station
        /// </summary>
        /// <param name="id"></param>
        [Route("stations/{id}")]
        [HttpDelete]
        public IHttpActionResult Station(string id)
        {
            AirUDBCOE db = new AirUDBCOE();

            // Validate Device from given DeviceId exists.
            Device registeredDevice = db.Devices.SingleOrDefault(x => x.DeviceID == id);

            if (registeredDevice != null)
            {
                Device toDelete = (from dev in db.Devices
                                   where dev.DeviceID == id
                                   select dev).Single();

                db.Devices.Remove(toDelete);
                db.SaveChanges();

                return Ok("Delete Successful");
            }
            else
            {
                // Device with DeviceID: <deviceID> does not exist.
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
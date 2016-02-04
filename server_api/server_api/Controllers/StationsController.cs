using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using server_api.Models;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace server_api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class StationsController : ApiController
    {
        private StationsRepository _repo = null;

        public StationsController()
        {
            _repo = new StationsRepository();
        }

        /*
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
                    string oString = @"select MaxCompleteStates.DeviceID, Devices.Name, Devices.Purpose, MaxCompleteStates.StateTime, MaxCompleteStates.Lat, MaxCompleteStates.Lng, MaxCompleteStates.InOrOut, MaxCompleteStates.Privacy from
                                        (select MaxStates.DeviceID, MaxStates.StateTime, DeviceStates.Lat, DeviceStates.Lng, DeviceStates.InOrOut, DeviceStates.Privacy from
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
                                                                    (bool)oReader["Privacy"],
                                                                    (string)oReader["Purpose"],
                                                                    (bool)oReader["InOrOut"],
                                                                    (decimal)oReader["Lat"],
                                                                    (decimal)oReader["Lng"],
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
       */
        /*
        /// <summary>
        /// Registers an AMS station:
        /// - Validates request
        /// - Updates Database to represent new association between existing user and 
        ///    new station.
        /// </summary>
        /// <param name="newDeviceState">The current Station and its StationState</param>
        /// <returns></returns>
        [ResponseType(typeof(SwaggerDeviceState))]
        [Route("stations/register")]
        [HttpPost]
        public IHttpActionResult RegisterStation([FromBody]SwaggerDeviceState newDeviceState) // TODO: why can't methods share the same name if they are different endpoints?
        {
            var db = new AirUDBCOE();

            Station existingDevice = db.Stations.SingleOrDefault(x => x.ID == newDeviceState.Id);
            if (existingDevice == null)
            {
                // Add station success.
                Station station = new Station();
                station.Name = newDeviceState.Name;
                station.ID = newDeviceState.Id;
                station.Email = "jaredpotter1@gmail.com"; // newDeviceAndState.Email;
                station.Privacy = newDeviceState.Privacy;
                station.Purpose = newDeviceState.Purpose;
                db.Stations.Add(station);
                db.SaveChanges();

                StationState state = new StationState();
                state.Station = station;
                state.InOrOut = newDeviceState.Indoor;
                state.Privacy = newDeviceState.Privacy;
                state.StateTime = new DateTime(1900, 1, 1);
                state.Lng = 0.0m;
                state.Lat = 90.0m;
                db.DeviceStates.Add(state);
                db.SaveChanges();

                return Ok(newDeviceState);
            }
            else
            {
                // Add station fail.
                return BadRequest("Existing Station");
            }
        }
        */

        /*
        /// <summary>
        ///   Adds one or many DeviceStates (from a station)
        /// </summary>
        /// <param name="state">*xml comment*</param>
        /// <returns></returns>
        [Route("stations/states")]
        [HttpPost]
        public IHttpActionResult AddAMSDeviceStates([FromBody]StationState[] states)
        {
            var db = new AirUDBCOE();
            Station station = states[0].Station;

            if (station == null)
            {
                // Failed to add StationState.
                return Ok("Failed to add station state with Station with ID = " + states[0].Station.ID + " not found.");
            }

            db.DeviceStates.AddRange(states);

            db.SaveChanges();

            // Success.
            return Ok(states);
        }
        */
        /*
        /// <summary>
        ///   Updates a single AMS StationState from the "my devices" settings web page.
        /// </summary>
        /// <param name="state">The state of the station</param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerDeviceState>))]
        [Route("stations/state/{deviceId}")]
        [HttpPut]
        public IHttpActionResult Station([FromBody]SwaggerDeviceState state)
        {
            var db = new AirUDBCOE();

            // Validate Station from given DeviceId exists.
            Station registeredDevice = db.Stations.SingleOrDefault(x => x.ID == state.Id);

            if (registeredDevice != null)
            {
                // Request previous state from database based on state.DeviceID
                StationState previousState = (
                                    from station in db.DeviceStates
                                    where station.Station.ID == state.Id
                                    && station.StateTime <= DateTime.Now // **May be a future source of contention - REVIEW**
                                    group station by station.Station.ID into deviceIDGroup
                                    select new
                                    {
                                        DeviceID = deviceIDGroup.Key,
                                        MaxMeasurementTime = deviceIDGroup.Max(station => station.StateTime)
                                    } into MaxStates
                                    join coordinates in db.DeviceStates
                                                            on MaxStates.MaxMeasurementTime equals coordinates.StateTime into latestStateGroup
                                    select latestStateGroup.FirstOrDefault()).Single();

                // Inherit lat and long from previous state

                StationState newDeviceState = new StationState();
                newDeviceState.Station = previousState.Station;
                newDeviceState.InOrOut = state.Indoor;
                newDeviceState.Privacy = state.Privacy;
                newDeviceState.Lat = previousState.Lat;
                newDeviceState.Lng = previousState.Lng;
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
                // Station with DeviceID: <deviceID> does not exist.
                return NotFound();
            }
        }
        */

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


        [ResponseType(typeof(IEnumerable<SwaggerAMSList>))]
        [Route("stations/locations/{latMin}/{latMax}/{lngMin}/{lngMax}/{withDataPoint}")] // TODO: properly configure the URL to specify lat/long min/max
        [HttpGet]
        public IHttpActionResult StationLocators(decimal latMin, decimal latMax, decimal lngMin, decimal lngMax)
        {
            return Ok(_repo.StationLocations(latMin, latMax, lngMin, lngMax));
        }

        /// <summary>
        ///   Returns all datapoints for a Station given a StationID.
        /// 
        ///   Primary Use: Compare View and single AMS station Map View "data graph"
        /// </summary>
        /// <param name="stationID"></param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<DataPoint>))]
        [Route("stations/datapoints/{stationID}")]
        [HttpGet]
        public IHttpActionResult DataPoints([FromUri]string stationID)
        {
            if (!_repo.StationExists(stationID))
            {
                return BadRequest("Station ID: " + stationID + " does not exist. Please verify the station has been registered.");
            }
            return Ok(_repo.GetDataPointsFromStation(stationID));
        }

        /// <summary>
        ///   Returns the latest datapoints for a single AMS station based on specified DeviceId. 
        ///   
        ///   Primary Use: "details" panel on Map View after selecting AMS station on map. 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [ResponseType(typeof(SwaggerLatestDataPoints))]
        [Route("stations/latestDataPoint/{deviceId}")]
        [HttpGet]
        public IHttpActionResult LatestDataPoint([FromUri]string deviceId)
        {
            var db = new AirUDBCOE();

            // Validate DeviceID represents an actual AMS station.
            Station registeredDevice = db.Stations.SingleOrDefault(x => x.Id == deviceId.ToString());
            /*
            if (registeredDevice != null)
            {
                // TODO: Should not be using hard coded db link
                // Performs database query to obtain the latest Datapoints for specific DeviceID.
                SqlConnection conn = new SqlConnection(@"Data Source=mssql.eng.utah.edu;Initial Catalog=air;Persist Security Info=True;User ID=lobato;Password=eVHDpynh;MultipleActiveResultSets=True;Application Name=EntityFramework");
                SwaggerLatestPollutantsList latestPollutants = new SwaggerLatestPollutantsList();
                SwaggerLatestDataPoints latest = new SwaggerLatestDataPoints();
                using (SqlConnection myConnection = conn)
                {
                    string oString = @"select Devices_States_and_DataPoints.DeviceID,
		                                        Devices_States_and_DataPoints.StateTime,
		                                        Devices_States_and_DataPoints.MeasurementTime,
		                                        Devices_States_and_DataPoints.Lat,
		                                        Devices_States_and_DataPoints.Lng,
		                                        Devices_States_and_DataPoints.InOrOut,
		                                        Devices_States_and_DataPoints.Privacy,
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
                // Station with DeviceID: <deviceID> does not exist.
                return NotFound();
            }
             * */
            return Ok();
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

            // Validate Station from given DeviceId exists.
            Station registeredDevice = db.Stations.SingleOrDefault(x => x.Id == id);

            if (registeredDevice != null)
            {
                Station toDelete = (from dev in db.Stations
                                    where dev.Id == id
                                    select dev).Single();

                db.Stations.Remove(toDelete);
                db.SaveChanges();

                return Ok("Delete Successful");
            }
            else
            {
                // Station with DeviceID: <deviceID> does not exist.
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
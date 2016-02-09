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



         /// <summary>
        ///   Returns all datapoints for a Station given a DeviceID.
        /// 
        ///   Primary Use: Compare View and single AMS station Map View "data graph"
        /// </summary>
        /// <param name="deviceID"></param>
        /// <returns></returns>
        //[ResponseType(typeof(IEnumerable<SwaggerPollutantList>))]
        [Route("stations/parameterValues")]
        [HttpGet]
        public IHttpActionResult GetAllDataPointsForParameters(string stationID, string parameter)
        {
            IEnumerable<DataPoint> points = _repo.GetDataPointsFromStation(stationID, parameter);

            Station existingStation = _repo.GetStation(stationID);

            List<SwaggerPollutantList> data = new List<SwaggerPollutantList>();

            SwaggerPollutantList pl = new SwaggerPollutantList(parameter);
            foreach (DataPoint datapoint in points)
            {
                pl.values.Add(new object[2]);
                pl.values.Last()[0] = ConvertDateTimeToMilliseconds(datapoint.Time);
                pl.values.Last()[1] = (decimal)datapoint.Value;
            }

            return Ok(pl);
        }



        [ResponseType(typeof(Station))]
        [Route("stations/register")]
        [HttpPost]
        public IHttpActionResult RegisterUserDevice([FromBody]JObject jsonData)
        {
            var db = new AirUDBCOE();

            /*Register Station
            {
                "station": {
                    "Name": "Draper",
                    "ID": "123",
                    "Agency": "EPA",
                    "Purpose": "Bad Stuff",
                    "Indoor" : false
                },
                "user": {
                    "Email": "zacharyisaiahlobato@gmail.com"
                }
            }
            */

            dynamic userPostData = jsonData;

            JObject userJObj = userPostData.user;
            JObject stationJObj = userPostData.station;

            User user = userJObj.ToObject<User>();
            Station station = stationJObj.ToObject<Station>();

            // IdentityUser existingUser = await _auth_repo.FindUser("lobato", "burritos");

            Station existingDevice = db.Stations.SingleOrDefault(x => x.Id == station.Id);
            User existingUser = db.Users.SingleOrDefault(x => x.Email == user.Email);

            if (existingDevice == null)
            {
                // Add station success.
                station.User = existingUser;
                db.Stations.Add(station);
                db.SaveChanges();

                return Ok(station);
            }
            else
            {
                // Add station fail.
                return BadRequest("Existing Station");
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
            if (!_repo.SetDataPointsFromStation(dataSet))
            {
                return NotFound();
            }
            else 
                return Ok();
        }


        [ResponseType(typeof(IEnumerable<Station>))]
        [Route("stations/locations")]
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
                return NotFound();
            }
            return Ok(_repo.GetDataPointsFromStation(stationID));
        }

        /// <summary>
        ///   Returns all datapoints for a Station given a StationID after a specified time.
        ///   On Javascript Side, use encodeURIComponent when sending DateTime
        ///   Primary Use: Compare View and single AMS station Map View "data graph"
        /// </summary>
        /// <param name="stationID"></param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<DataPoint>))]
        [Route("stations/datapoints/{stationID}/{after}")]
        [HttpGet]
        public IHttpActionResult DataPoints([FromUri]string stationID, [FromUri]DateTime after)
        {
            if (!_repo.StationExists(stationID))
            {
                return NotFound();
            }
            return Ok(_repo.GetDataPointsFromStationAfterTime(stationID, after));
        }


        /// <summary>
        ///   Returns all datapoints for a Station given a StationID between two times.
        ///   On Javascript Side, use encodeURIComponent when sending DateTime
        ///   Primary Use: Compare View and single AMS station Map View "data graph"
        /// </summary>
        /// <param name="stationID"></param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<DataPoint>))]
        [Route("stations/datapoints/{stationID}/{after}/{before}")]
        [HttpGet]
        public IHttpActionResult DataPoints([FromUri]string stationID, [FromUri]DateTime after, [FromUri]DateTime before)
        {
            if (!_repo.StationExists(stationID))
            {
                return NotFound();
            }
            return Ok(_repo.GetDataPointsFromStationBetweenTimes(stationID, after, before));
        }



        /// <summary>
        ///   Returns the latest datapoints for a single AMS station based on specified DeviceId. 
        ///   
        ///   Primary Use: "details" panel on Map View after selecting AMS station on map. 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<DataPoint>))]
        [Route("stations/latestDataPoint/{stationID}")]
        [HttpGet]
        public IHttpActionResult LatestDataPoint([FromUri]string stationID)
        {
            var db = new AirUDBCOE();

            if (!_repo.StationExists(stationID))
            {
                return BadRequest("Station ID: " + stationID + " does not exist. Please verify the station has been registered.");
            }

            return Ok(_repo.GetLatestDataPointsFromStation(stationID));
        }

        /// <summary>
        ///   Deletes the selected station
        /// </summary>
        /// <param name="id"></param>
        [Route("stations/{stationID}")]
        [HttpDelete]
        public IHttpActionResult Station(string stationID)
        {
            if (_repo.DeleteStation(stationID))
            {
                return Ok();
            }

            return NotFound();
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
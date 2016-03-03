using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Threading;
using System.Threading.Tasks;
using server_api.Models;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.Entity.Spatial;
using System.Globalization;

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
        /// Returns an array of objects specific for our NVD3 plots. Each object is keyed by the 
        ///   station name and parameter type. Each value is an array of timestamps and measurements of the
        ///   given parameter. This endpoint is temporary, proof of concept... I'd like to add time range
        ///   to this functionality.
        /// </summary>
        /// <param name="stationID">A list of station ids</param>
        /// <param name="parameter">A list of parameter types</param>
        /// <returns></returns>
        [Route("stations/parameterValues")]
        [HttpGet]
        public IHttpActionResult GetAllDataPointsForParameters([FromUri] string[] stationID, [FromUri] string[] parameter)
        {
            // get all datapoints matching the station ids and parameter types
            IEnumerable<DataPoint> points = _repo.GetDataPointsFromStation(stationID, parameter);

            Dictionary<string, SwaggerPollutantList> data = new Dictionary<string, SwaggerPollutantList>();

            // builds an object specific for the NVD3 plots where the key is eg:
            //   Hawthorne - PM2.5
            foreach (DataPoint d in points)
            {
                string key = (d.Station.Name + " - " + d.Parameter.Name);

                SwaggerPollutantList list;
                if (!data.TryGetValue(key, out list))
                {
                    data.Add(key, new SwaggerPollutantList(key));
                    if (!data.TryGetValue(key, out list))
                    {
                        return BadRequest();
                    }
                }

                list.values.Add(new object[2]);
                list.values.Last()[0] = ConvertDateTimeToMilliseconds(d.Time);
                list.values.Last()[1] = (decimal)d.Value;
            }

            normalizeDataSwaggerPollutantList(ref data);
            
            return Ok(data.Values);
        }

        private void normalizeDataSwaggerPollutantList(ref Dictionary<string, SwaggerPollutantList> dict) {
            // find longest length array in each object
            int max = 0;
            foreach (KeyValuePair<string, SwaggerPollutantList> pair in dict) {
                if (pair.Value.values.Count > max)
                {
                    max = pair.Value.values.Count;
                }
            }

            // normalize all other arrays to be the same length
            foreach (KeyValuePair<string, SwaggerPollutantList> pair in dict)
            {
                List<object[]> current = pair.Value.values;
                while (current.Count < max)
                {
                    current.Add(new object[0]);
                }
            }
        }

        [ResponseType(typeof(Station))]
        [Route("stations/register")]
        [HttpPost]
        public IHttpActionResult RegisterUserStation([FromBody]JObject jsonData)
        {
            var db = new AirUDBCOE();

            /*Register Station exmaple json.
            {
                "station": {
                    "Name": "Draper",
                    "ID": "MAC000001",
                    "Agency": "AirU",
                    "Purpose": "Testing",
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

            Station existingDevice = db.Stations.SingleOrDefault(x => x.Id == station.Id);
            User existingUser = db.Users.SingleOrDefault(x => x.Email == user.Email);

            if (existingUser != null)
            {
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
                    return BadRequest("Station already exists.");
                }
            }
            else
            {
                return BadRequest("User does not exist.");
            }
        }

        /// <summary>
        ///   Adds one or many DevicePoints (from a station).
        ///   Used by stations to post data to the database.
        /// </summary>
        /// <param name="dataSet">AMSDataSet Model</param>
        /// <returns></returns>
        [Route("stations/data")]
        [HttpPost]
        public IHttpActionResult AddStationDataPointSet([FromBody]DataPoint[] dataSet)
        {
            DateTime start = DateTime.Now;

            if (dataSet.Length == 0)
            {
                return BadRequest("No DataPoints in sent array.");
            }

            IEnumerable<DataPoint>response = _repo.SetDataPointsFromStation(dataSet);
            DateTime end = DateTime.Now;

            if (response==null)
            {
                return BadRequest("Station does not exist.");
            }
            else 
                return Ok(response);
        }

        [Route("stations/download")]
        [HttpGet]
        public IHttpActionResult DownloadStationData([FromUri] string[] stationID, [FromUri] string[] parameter)
        {
            // get all datapoints matching the station ids and parameter types
            IEnumerable<DataPoint> points = _repo.GetDataPointsFromStation(stationID, parameter);

            string delimiter = "\"";
            string tick = "\'";
            string separator = ",";
            string linefeed = "\r\n";
            
            StringBuilder sb = new StringBuilder();

            // write header
            sb.Append("Station,Id,Agency,City,State,Postal,Parameter,Value,Unit,AQI,Category,Date,Time\r\n");
            foreach (DataPoint d in points) {
                // write station information for datapoint
                sb.Append(delimiter + d.Station.Name + delimiter + separator);
                sb.Append(tick + d.Station.Id + separator);
                sb.Append(delimiter + d.Station.Agency + delimiter + separator);
                sb.Append(d.Station.City + separator);
                sb.Append(d.Station.State + separator);
                sb.Append(d.Station.Postal + separator);

                // write datapoint information
                sb.Append(d.Parameter.Name + separator);
                sb.Append(d.Value + separator);
                sb.Append(d.Parameter.Unit + separator);
                sb.Append(d.AQI + separator);
                sb.Append(d.Category + separator);

                // write the date and time
                sb.Append(d.Time.Year + "/" + d.Time.Month + "/" + d.Time.Day + separator);
                sb.Append(d.Time.Hour + ":" + d.Time.Minute + separator);

                sb.Append(linefeed);
            }

            // this tells the client browser to download the content as an attachment
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(sb.ToString());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = "attachment.csv";
            return ResponseMessage(response);
        }
        
        [ResponseType(typeof(IEnumerable<Station>))]
        [Route("stations/locations")]
        [HttpGet]
        public IHttpActionResult StationLocators(double latMin, double latMax, double lngMin, double lngMax)
        {
            return Ok(_repo.StationLocations(latMin, latMax, lngMin, lngMax));
        }

        [ResponseType(typeof(IEnumerable<Station>))]
        [Route("stations/nearest")]
        [HttpGet]
        public IHttpActionResult NearestStation(double lat, double lng)
        {
            return Ok(_repo.GetNearestStation(lat, lng));
        }

        [ResponseType(typeof(IEnumerable<Station>))]
        [Route("stations/within")]
        [HttpGet]
        public IHttpActionResult StationsInRadiusMiles(double lat, double lng, double miles)
        {
            return Ok(_repo.GetStationsWithinRadiusMiles(lat, lng, miles));
            //return Ok(_repo.GetStationsWithinRadiusMiles(lat, lng, miles));
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
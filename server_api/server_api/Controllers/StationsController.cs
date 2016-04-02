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
using Swashbuckle.Swagger.Annotations;
using Microsoft.AspNet.Identity;

namespace server_api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class StationsController : ApiController
    {
        private StationsRepository _stationRepo = null;
        private UserRepository _userRepo = null;

        public StationsController()
        {
            _stationRepo = new StationsRepository();
            _userRepo = new UserRepository();
        }


        /// <summary>
        /// Returns an array of objects specific for our NVD3 plots. Each object is keyed by the 
        ///   station name and parameter type. Each value is an array of timestamps and measurements of the
        ///   given parameter. This endpoint is temporary, proof of concept... I'd like to add time range
        ///   to this functionality.
        /// </summary>
        /// <param name="id">A list of station ids</param>
        /// <param name="parameter">A list of parameter types</param>
        /// <returns></returns>
        [Route("stations/parameterValues")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(SwaggerPollutantList))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public IHttpActionResult GetAllDataPointsForParameters([FromUri] string[] stationID, [FromUri] string[] parameter)
        {
            // get all datapoints matching the station ids and parameter types
            IEnumerable<DataPoint> points = _stationRepo.GetDataPointsFromStation(stationID, parameter);

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

        [Authorize]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Station))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(string))]
        [Route("stations/register")]
        [HttpPost]
        public IHttpActionResult RegisterUserStation([FromBody]Station newStation)
        {
            var db = new ApplicationContext();

            newStation.User_Id = RequestContext.Principal.Identity.GetUserId() as string;
            Station result = _stationRepo.CreateStation(newStation);
            if (result != null)
            {
                return Ok(result);
            }else{
                return BadRequest("Station already exists.");
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<DataPoint>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(string))]
        public IHttpActionResult AddStationDataPointSet([FromBody]DataPoint[] dataSet)
        {
            DateTime start = DateTime.Now;

            if (dataSet.Length == 0)
            {
                return BadRequest("No DataPoints in sent array.");
            }

            IEnumerable<DataPoint>response = _stationRepo.SetDataPointsFromStation(dataSet);
            DateTime end = DateTime.Now;

            if (response==null)
            {
                return BadRequest("Station does not exist.");
            }
            else 
                return Ok(response);
        }

        /// <summary>
        /// Downloads the datapoints for specific parameters given a list of station ids and parameter names
        /// </summary>
        /// <param name="id">Station IDs</param>
        /// <param name="parameter">Parameter names</param>
        /// <returns></returns>
        [Route("stations/download")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK)]
        public IHttpActionResult DownloadStationData([FromUri] string[] stationID, [FromUri] string[] parameter)
        {
            // get all datapoints matching the station ids and parameter types
            IEnumerable<DataPoint> points = _stationRepo.GetDataPointsFromStation(stationID, parameter);

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
            return Ok(_stationRepo.StationLocations(latMin, latMax, lngMin, lngMax));
        }

        [ResponseType(typeof(IEnumerable<Station>))]
        [Route("stations/nearest")]
        [HttpGet]
        public IHttpActionResult NearestStation(double lat, double lng)
        {
            return Ok(_stationRepo.GetNearestStation(lat, lng));
        }

        [ResponseType(typeof(IEnumerable<Station>))]
        [Route("stations/within")]
        [HttpGet]
        public IHttpActionResult StationsInRadiusMiles(double lat, double lng, double miles)
        {
            return Ok(_stationRepo.GetStationsWithinRadiusMiles(lat, lng, miles));
            //return Ok(_stationRepo.GetStationsWithinRadiusMiles(lat, lng, miles));
        }

        /// <summary>
        ///   Returns all datapoints for a Station given a StationID.
        /// 
        ///   Primary Use: Compare View and single AMS station Map View "data graph"
        /// </summary>
        /// <param name="id">Station ID</param>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<DataPoint>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("stations/datapoints/{id}")]
        [HttpGet]
        public IHttpActionResult DataPoints([FromUri]string id)
        {
            if (!_stationRepo.StationExists(id))
            {
                return NotFound();
            }
            return Ok(_stationRepo.GetDataPointsFromStation(id));
        }

        /// <summary>
        ///   Returns all datapoints for a Station given a StationID after a specified time.
        ///   On Javascript Side, use encodeURIComponent when sending DateTime
        ///   Primary Use: Compare View and single AMS station Map View "data graph"
        /// </summary>
        /// <param name="id">Station Id</param>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<DataPoint>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("stations/datapoints/{id}/{after}")]
        [HttpGet]
        public IHttpActionResult DataPoints([FromUri]string id, [FromUri]DateTime after)
        {
            if (!_stationRepo.StationExists(id))
            {
                return NotFound();
            }
            return Ok(_stationRepo.GetDataPointsFromStationAfterTime(id, after));
        }

        /// <summary>
        ///   Returns all datapoints for a Station given a StationID between two times.
        ///   On Javascript Side, use encodeURIComponent when sending DateTime
        ///   Primary Use: Compare View and single AMS station Map View "data graph"
        /// </summary>
        /// <param name="id">Station ID</param>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<DataPoint>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("stations/datapoints/{id}/{after}/{before}")]
        [HttpGet]
        public IHttpActionResult DataPoints([FromUri]string id,[FromUri]DateTime after, [FromUri]DateTime before)
        {
            if (!_stationRepo.StationExists(id))
            {
                return NotFound();
            }
            return Ok(_stationRepo.GetDataPointsFromStationBetweenTimes(id, after, before));
        }

        /// <summary>
        ///   Returns the latest datapoints for a single AMS station based on specified DeviceId. 
        ///   
        ///   Primary Use: "details" panel on Map View after selecting AMS station on map. 
        /// </summary>
        /// <param name="id">Station ID</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DataPoint))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("stations/latestDataPoint/{id}")]
        [HttpGet]
        public IHttpActionResult LatestDataPoint([FromUri]string id)
        {
            var db = new ApplicationContext();

            if (!_stationRepo.StationExists(id))
            {
                return NotFound();
            }

            return Ok(_stationRepo.GetLatestDataPointsFromStation(id));
        }

        /// <summary>
        ///   Deletes the selected station
        /// </summary>
        /// <param name="id">Station ID</param>
        [Authorize]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [Route("stations/{id}")]
        [HttpDelete]
        public IHttpActionResult Station(string id)
        {
            Station station = _stationRepo.GetStation(id);
            if (station == null)
            {
                return NotFound();
            }
            if (station.User_Id == RequestContext.Principal.Identity.GetUserId())
            {
                if (_stationRepo.DeleteStation(id))
                {
                    return Ok();
                }
                return InternalServerError();
            }
            else
            {
                return Unauthorized();
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
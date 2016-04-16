using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Description;
using server_api.Models;
using Newtonsoft.Json.Linq;
using System.Data.Entity.Spatial;
using System.Globalization;
using Swashbuckle.Swagger.Annotations;
using Microsoft.AspNet.Identity;
using server_api.Utilities;
using System.Threading.Tasks;

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

        [Route("stations/{id}")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Station))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetStation(string id)
        {
            Station station = _stationRepo.GetStation(id);
            if (station == null)
            {
                return NotFound();
            }
            return Ok(station);
        }

        [Authorize]
        [Route("stations/{id}")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Station))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public IHttpActionResult UpdateStation(string id, [FromBody]Station stationUpdate)
        {

            if (stationUpdate.Id != id)
            {
                stationUpdate.Id = id;
            }
            Station station = _stationRepo.GetStation(id);
            if (station == null)
            {
                return NotFound();
            }
            
            if (station.User_Id != RequestContext.Principal.Identity.GetUserId())
            {
                return Unauthorized();
            }
            var result = _stationRepo.UpdateStation(station, stationUpdate);
            return Ok(result);
        }

        /// <summary>
        /// Returns an array of objects specific for our NVD3 plots. Each object is keyed by the 
        ///   station name and parameter type. Each value is an array of timestamps and measurements of the
        ///   given parameter.
        /// </summary>
        [Route("stations/parameterValues")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Nvd3Data))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public IHttpActionResult GetAllDataPointsForParameters([FromUri] DownloadOptions options)
        {
            // get all datapoints matching the station ids and parameter types
            IEnumerable<DataPoint> points = _stationRepo.GetDataPointsFromStation(options);

            Dictionary<string, Nvd3Data> data = new Dictionary<string, Nvd3Data>();

            // builds an object specific for the NVD3 plots where the key is eg:
            //   Hawthorne - PM2.5
            foreach (DataPoint d in points)
            {
                string key = (d.Station.Name + " - " + d.Parameter.Name);

                Nvd3Data list;
                if (!data.TryGetValue(key, out list))
                {
                    data.Add(key, new Nvd3Data(key));
                    if (!data.TryGetValue(key, out list))
                    {
                        return BadRequest();
                    }
                }

                list.Values.Add(new object[2]);
                list.Values.Last()[0] = ConvertDateTimeToMilliseconds(d.Time);
                if (options.UseRawValues)
                {
                    list.Values.Last()[1] = (decimal) d.Value;
                }
                else
                {
                    list.Values.Last()[1] = (int) d.AQI;
                }
                
            }

            normalizeDataSwaggerPollutantList(ref data);
            
            return Ok(data.Values);
        }

        private void normalizeDataSwaggerPollutantList(ref Dictionary<string, Nvd3Data> dict)
        {
            // find longest length array in each object
            int max = 0;
            foreach (KeyValuePair<string, Nvd3Data> pair in dict)
            {
                if (pair.Value.Values.Count > max)
                {
                    max = pair.Value.Values.Count;
                }
            }

            // normalize all other arrays to be the same length
            foreach (KeyValuePair<string, Nvd3Data> pair in dict)
            {
                List<object[]> current = pair.Value.Values;
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
        public async Task<IHttpActionResult> RegisterUserStation([FromBody]Station newStation)
        {
            User user = await _userRepo.FindUserById(RequestContext.Principal.Identity.GetUserId());
            if (user == null)
            {
                return Unauthorized();
            }

            newStation.User_Id = user.Id;
            Object result = _stationRepo.CreateStation(newStation);
            if (result is Station)
            {
                return Ok(result);
            }

            return BadRequest(result as string);
        }

        /// <summary>
        ///   Adds one or many DevicePoints (from a station).
        ///   Used by stations to post data to the database.
        /// 
        ///   AQI formula and breakpoints.
        ///   https://www3.epa.gov/ttn/caaa/t1/memoranda/rg701.pdf
        /// 
        ///   UPDATED breakpoints for PM 2.5
        ///   https://www3.epa.gov/airquality/particlepollution/2012/decfsstandards.pdf
        /// 
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
                return NotFound();
            }

            foreach(DataPoint dataPoint in dataSet)
            {
                if(dataPoint.Category == 0 && dataPoint.AQI == 0 || dataPoint.AQI < 0)
                {
                    Tuple<int, int> result = null;

                    switch(dataPoint.Parameter.Name)
                    {
                        case "PM2.5":
                            result = Pm25Aqi.CalculateAQIAndCategory(dataPoint.Value);
                            dataPoint.AQI = result.Item1;
                            dataPoint.Category = result.Item2;
                            break;

                        case "PM10":
                            result = Pm10Aqi.CalculateAQIAndCategory(dataPoint.Value);
                            dataPoint.AQI = result.Item1;
                            dataPoint.Category = result.Item2;
                            break;

                        case "CO":
                            result = CoAqi.CalculateAQIAndCategory(dataPoint.Value);
                            dataPoint.AQI = result.Item1;
                            dataPoint.Category = result.Item2;
                            break;

                        case "NO2":
                            result = No2Aqi.CalculateAQIAndCategory(dataPoint.Value);
                            dataPoint.AQI = result.Item1;
                            dataPoint.Category = result.Item2;
                            break;

                        case "OZONE":
                            result = OzoneAqi.CalculateAQIAndCategory(dataPoint.Value);
                            dataPoint.AQI = result.Item1;
                            dataPoint.Category = result.Item2;
                            break;

                        case "SO2":
                            result = So2Aqi.CalculateAQIAndCategory(dataPoint.Value);
                            dataPoint.AQI = result.Item1;
                            dataPoint.Category = result.Item2;
                            break;
                    }
                }
            }

            IEnumerable<DataPoint>response = _stationRepo.SetDataPointsFromStation(dataSet);
            DateTime end = DateTime.Now;

            if (response==null)
            {
                return NotFound();
            }

            return Ok(response);
        }

        /// <summary>
        /// Downloads the datapoints for specific parameters given a list of station ids and parameter names
        /// </summary>
        [Route("stations/download")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK)]
        public IHttpActionResult DownloadStationData([FromUri] DownloadOptions options)
        {
            // get all datapoints matching the station ids and parameter types
            IEnumerable<DataPoint> points = _stationRepo.GetDataPointsFromStation(options);

            string delimiter = "\"";
            string tick = "\'";
            string separator = ",";
            string linefeed = "\r\n";

            StringBuilder sb = new StringBuilder();

            // write header
            sb.Append("Station,Id,Agency,City,State,Postal,Parameter,Value,Unit,AQI,Category,Date,Time\r\n");
            foreach (DataPoint d in points)
            {
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
        [Route("stations")]
        [HttpGet]
        public IHttpActionResult GetAllStations()
        {
            return Ok(_stationRepo.GetAllStations());
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
        ///   Adds the MAC address of a custom device (non-AirNow) to the un-registered table.
        ///   
        ///   If the MAC address is already added, do nothing and return 200.
        /// </summary>
        /// <returns></returns>
        [Route("stations/ping")]
        [HttpPost]
        public IHttpActionResult Ping([FromBody]UnregisteredStation station)
        {
            _stationRepo.AddThirdPartyDevice(station);

            return Ok();
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
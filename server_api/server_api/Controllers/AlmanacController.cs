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
    public class AlmanacController : ApiController
    {
        private AlmanacRepository _repo = null;

        public AlmanacController()
        {
            _repo = new AlmanacRepository();
        }

        [ResponseType(typeof(IEnumerable<Daily>))]
        [Route("almanac/dailies")]
        [HttpGet]
        public IHttpActionResult NearestStation(string stationId, int daysBack)
        {
            return Ok(_repo.GetNDailiesByStationID(stationId, daysBack));
        }

    }
}
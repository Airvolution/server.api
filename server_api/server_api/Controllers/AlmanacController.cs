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
        private StationsRepository _repo = null;

        public AlmanacController()
        {
            _repo = new StationsRepository();
        }
    }
}
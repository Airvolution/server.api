﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace server_api.Controllers
{
    public class StationsController: ApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("stations")]
        [HttpGet]
        public IHttpActionResult StationLocations()
        {

            return Ok();
        }
    }
}
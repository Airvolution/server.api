using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace server_api.Controllers
{
    public class ContactUsController : ApiController
    {
        /// <summary>
        ///   Sends the post contents to airu group email address.
        /// </summary>
        [Route("contactUs")]
        [HttpPost]
        public IHttpActionResult SendEmail([FromBody]Content content)
        {

            return Ok("yeah bitches");
        }
    }

    public class Content
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}

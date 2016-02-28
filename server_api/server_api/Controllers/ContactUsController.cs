using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;
using server_api.Models;

namespace server_api.Controllers
{
    public class ContactUsController : ApiController
    {
        /// <summary>
        ///   Sends the post contents to airu group email address.
        /// </summary>
        [Route("contactUs")]
        [HttpPost]
        public IHttpActionResult SendEmail([FromBody]EmailContent content)
        {
            string body = "Name: " + content.Name + '\n' +
                          "Subject: " + content.Subject + '\n' +
                          "Email: " + content.Email + '\n' +
                          "Message: " + content.Message;

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("air.utah.cs@gmail.com", "Burri0s123*"),
                EnableSsl = true,

            };
            client.Send(content.Email, "air.utah.cs@gmail.com", content.Subject, body);
            Console.WriteLine("Sent");

            return Ok();
        }
    }
}

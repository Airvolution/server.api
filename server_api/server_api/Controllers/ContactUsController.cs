using System;
using System.Collections.Generic;
using System.IO;
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

            string username = "";
            string password = "";

            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(@"c:\contactUsEmailCredentials.txt"))
                {
                    // Read the stream to a string, and write the string to the console.
                    username = sr.ReadLine();
                    password = sr.ReadLine();
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true,

            };
            client.Send(content.Email, "air.utah.cs@gmail.com", content.Subject, body);
            Console.WriteLine("Sent");

            return Ok();
        }
    }
}

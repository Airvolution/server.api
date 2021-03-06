﻿using System.IO;
using System.Net;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;
using server_api.Models;
using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger.Annotations;

namespace server_api.Controllers
{
    public class WeatherController : ApiController
    {
        private static string baseUrl = "http://api.openweathermap.org/data/2.5/weather";
        private static string AccessToken = "&APPID=9f5c853ae111945a65873eae72898a19";

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("weather/current")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(WeatherResponse))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(string))]
        public IHttpActionResult CurrentWeather(decimal lat = 360, decimal lng = 360, int id = -1, int zip = -1)
        {
            StringBuilder parameters = new StringBuilder("?");

            if (id != -1)
            {
                parameters.Append("id=" + id);
            }
            else if (zip != -1)
            {
                parameters.Append("zip=" + zip + ",us");
            }
            else if (lat != 360 || lng != 360)
            {
                parameters.Append("lat=" + lat + "&lon=" + lng);
            }
            else
            {
                return BadRequest("Invalid parameters");
            }

            string url = baseUrl + parameters.ToString() + AccessToken;

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                string json = reader.ReadToEnd();
                return Ok(json);
            }
            return BadRequest("Call to OpenWeather failed with response code: " + response.StatusCode);
        }
    }
}
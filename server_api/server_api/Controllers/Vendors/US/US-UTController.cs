using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml.Serialization;
using server_api.Models;

namespace server_api.Controllers.Vendors
{
    public class UTController : ApiController
    {
        static SwaggerDAQData[] dataArray = new SwaggerDAQData[11];

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<SwaggerDAQData>))]
        [Route("US/UT")]
        [HttpGet]
        public IHttpActionResult MostRecentStationData()
        {
            #region ToBeReplacedByDBStuff

            string[] apiUrls = new string[] 
                { "http://air.utah.gov/xmlFeed.php?id=boxelder",   // "Box Elder County"
                  "http://air.utah.gov/xmlFeed.php?id=cache",      // "Cache County"
                  "http://air.utah.gov/xmlFeed.php?id=p2",         // Carbon/"Price"
                  "http://air.utah.gov/xmlFeed.php?id=bv",         // "Davis County"
                  "http://air.utah.gov/xmlFeed.php?id=rs",         // "Duchesne County"
                  "http://air.utah.gov/xmlFeed.php?id=slc",        // "Salt Lake County"
                  "http://air.utah.gov/xmlFeed.php?id=tooele",     // "Tooele County"
                  "http://air.utah.gov/xmlFeed.php?id=v4",         // "Uintah County"
                  "http://air.utah.gov/xmlFeed.php?id=utah",       // "Utah County"
                  "http://air.utah.gov/xmlFeed.php?id=washington", // "Washington County"
                  "http://air.utah.gov/xmlFeed.php?id=weber"       // "Weber County"
                };

            Tuple<double, double>[] gpsLocations = new Tuple<double, double>[] 
            { new Tuple<double, double>(41.510544, -112.014640), 
              new Tuple<double, double>(41.737159, -111.836706),
              new Tuple<double, double>(39.598401, -110.811250),
              new Tuple<double, double>(40.979952, -111.887608),
              new Tuple<double, double>(40.163389, -110.402936),
              new Tuple<double, double>(40.734280, -111.871593), 
              new Tuple<double, double>(40.530786, -112.298464),
              new Tuple<double, double>(40.455679, -109.528717),
              new Tuple<double, double>(40.296847, -111.695003),
              new Tuple<double, double>(37.096288, -113.568486),
              new Tuple<double, double>(41.222803, -111.973789) };

            

            for (int i = 0; i < apiUrls.Length; i++)
            {
                HttpWebRequest request = WebRequest.Create(apiUrls[i]) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream stream = response.GetResponseStream();
                XmlSerializer serializer = new XmlSerializer(typeof(SwaggerDAQData));
                StreamReader reader = new StreamReader(stream);
                SwaggerDAQData data = (SwaggerDAQData)serializer.Deserialize(reader);

                data.site.latitude = gpsLocations[i].Item1;
                data.site.longitude = gpsLocations[i].Item2;

                for (int j = 0; j < data.site.data.Length; j++)
                {
                    DateTime wrongDateTime = DateTime.ParseExact(data.site.data[j].date, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    DateTime correctDateTime = wrongDateTime.AddHours(1);
                    data.site.data[j].date = correctDateTime.ToString("MM/dd/yyyy HH:mm:ss");
                }



                dataArray[i] = data;
            }
            #endregion

            //TODO: here is where we database calls

            return Ok(dataArray);
        }

        static Dictionary<string, string> apiUrlDict = new Dictionary<string, string>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [Route("US/UT/{id}")]
        [HttpGet]
        public IHttpActionResult GetDAQChartData([FromUri]int id)
        {
                apiUrlDict.Add("Box Elder County", "http://air.utah.gov/xmlFeed.php?id=boxelder");
                apiUrlDict.Add("Cache County", "http://air.utah.gov/xmlFeed.php?id=cache");
                apiUrlDict.Add("Price", "http://air.utah.gov/xmlFeed.php?id=p2");
                apiUrlDict.Add("Davis County", "http://air.utah.gov/xmlFeed.php?id=bv");
                apiUrlDict.Add("Duchesne County", "http://air.utah.gov/xmlFeed.php?id=rs");
                apiUrlDict.Add("Salt Lake County", "http://air.utah.gov/xmlFeed.php?id=slc");
                apiUrlDict.Add("Tooele County", "http://air.utah.gov/xmlFeed.php?id=tooele");
                apiUrlDict.Add("Uintah County", "http://air.utah.gov/xmlFeed.php?id=v4");
                apiUrlDict.Add("Utah County", "http://air.utah.gov/xmlFeed.php?id=utah");
                apiUrlDict.Add("Washington County", "http://air.utah.gov/xmlFeed.php?id=washington");
                apiUrlDict.Add("Weber County", "http://air.utah.gov/xmlFeed.php?id=weber");


                //string url = apiUrlDict[name];
            string url = "asd";

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream stream = response.GetResponseStream();
                XmlSerializer serializer = new XmlSerializer(typeof(SwaggerDAQData));
                StreamReader reader = new StreamReader(stream);
                SwaggerDAQData data = (SwaggerDAQData)serializer.Deserialize(reader);

                List<SwaggerPollutantList> pollutantDataList = new List<SwaggerPollutantList>();

                List<string> dates = new List<string>();
                SwaggerPollutantList ozone = new SwaggerPollutantList("Ozone ppm");
                SwaggerPollutantList pm25 = new SwaggerPollutantList("PM 2.5 ug/m^3");
                SwaggerPollutantList no2 = new SwaggerPollutantList("NO2 ppm");
                SwaggerPollutantList temperature = new SwaggerPollutantList("Temperature F");
                SwaggerPollutantList co = new SwaggerPollutantList("CO ppm");

                foreach (var dataSet in data.site.data)
                {
                    DateTime wrongDateTime = DateTime.ParseExact(dataSet.date, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    DateTime correctDateTime = wrongDateTime.AddHours(1);

                    long dateMilliseconds = ConvertDateTimeToMilliseconds(correctDateTime);

                    if (dataSet.ozone != "")
                    {
                        ozone.values.Add(new object[2]);
                        ozone.values.Last()[0] = dateMilliseconds;
                        ozone.values.Last()[1] = Decimal.Parse(dataSet.ozone);
                    }
                    else
                    {
                        ozone.values.Add(new object[2]);
                        ozone.values.Last()[0] = dateMilliseconds;
                        ozone.values.Last()[1] = 0.0;
                    }

                    if (dataSet.pm25 != "")
                    {
                        pm25.values.Add(new object[2]);
                        pm25.values.Last()[0] = dateMilliseconds;
                        pm25.values.Last()[1] = Decimal.Parse(dataSet.pm25);
                    }
                    else
                    {
                        pm25.values.Add(new object[2]);
                        pm25.values.Last()[0] = dateMilliseconds;
                        pm25.values.Last()[1] = 0.0;
                    }

                    if (dataSet.no2 != "")
                    {
                        no2.values.Add(new object[2]);
                        no2.values.Last()[0] = dateMilliseconds;
                        no2.values.Last()[1] = Decimal.Parse(dataSet.no2);
                    }
                    else
                    {
                        no2.values.Add(new object[2]);
                        no2.values.Last()[0] = dateMilliseconds;
                        no2.values.Last()[1] = 0.0;
                    }

                    if (dataSet.temperature != "")
                    {
                        temperature.values.Add(new object[2]);
                        temperature.values.Last()[0] = dateMilliseconds;
                        temperature.values.Last()[1] = Decimal.Parse(dataSet.temperature);
                    }
                    else
                    {
                        temperature.values.Add(new object[2]);
                        temperature.values.Last()[0] = dateMilliseconds;
                        temperature.values.Last()[1] = 0.0;
                    }

                    if (dataSet.co != "")
                    {
                        co.values.Add(new object[2]);
                        co.values.Last()[0] = dateMilliseconds;
                        co.values.Last()[1] = Decimal.Parse(dataSet.co);
                    }
                    else
                    {
                        co.values.Add(new object[2]);
                        co.values.Last()[0] = dateMilliseconds;
                        co.values.Last()[1] = 0.0;
                    }
                }

                if (ozone.values.Count != 0)
                {
                    pollutantDataList.Add(ozone);
                }

                if (pm25.values.Count != 0)
                {
                    pollutantDataList.Add(pm25);
                }

                if (no2.values.Count != 0)
                {
                    pollutantDataList.Add(no2);
                }

                if (temperature.values.Count != 0)
                {
                    pollutantDataList.Add(temperature);
                }

                if (co.values.Count != 0)
                {
                    pollutantDataList.Add(co);
                }

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

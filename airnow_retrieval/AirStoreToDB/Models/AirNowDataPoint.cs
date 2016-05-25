using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirStoreToDB.Models
{
    class AirNowDataPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string UTC { get; set; }
        public string Parameter { get; set; }
        public string Unit { get; set; }
        public double Value { get; set; }
        public int AQI { get; set; }
        public int Category { get; set; }
        public string SiteName { get; set; }
        public string AgencyName { get; set; }
        public string FullAQSCode { get; set; }
        public string IntlAQSCode { get; set; }
    }
}

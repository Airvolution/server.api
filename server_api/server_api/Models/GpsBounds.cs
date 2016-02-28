using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace server_api.Models
{
    public class GpsBounds
    {
        public double latMin { get; set; }
        public double longMin { get; set; }
        public double latMax { get; set; }
        public double longMax { get; set; }
    }
}
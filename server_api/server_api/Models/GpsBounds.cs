using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace server_api.Models
{
    public class GpsBounds
    {
        public decimal latMin { get; set; }
        public decimal longMin { get; set; }
        public decimal latMax { get; set; }
        public decimal longMax { get; set; }
    }
}
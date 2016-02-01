using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace server_api.Models
{
    public class WeatherResponse
    {
        coordindate coord;
        weather[] weather;
        //string base;
        measurements main;
        wind wind;
        clouds clouds;
        int dt;
        sys sys;
        int id;
        string name;
        int cod;
    }

    public class coordindate
    {
        public decimal lon {get; set;}
        public decimal lat {get; set;}
    }

    public class weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class measurements
    {
        public decimal temp { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
        public decimal temp_min { get; set; }
        public decimal temp_max { get; set; }
    }

    public class wind
    {
        public decimal speed { get; set; }
        public int deg { get; set; }
    }

    public class clouds
    {
        public int all { get; set; }
    }

    public class sys
    {
        public int type { get; set; }
        public int id { get; set; }
        public decimal message { get; set; }
        public string country { get; set; }
        public long sunrise { get; set; }
        public long sunset { get; set; }
    }
}
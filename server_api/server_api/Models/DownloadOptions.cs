using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace server_api.Models
{
    public class DownloadOptions
    {
        public string[] StationIds { get; set; }
        public string[] Parameters { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool UseRawValues { get; set; }
    }
}

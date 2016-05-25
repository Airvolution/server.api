using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace server_api.Models
{
    public class UpdateGroup
    {
        public string[] stationsToAdd { get; set; }
        public string[] stationsToRemove { get; set; }
    }
}

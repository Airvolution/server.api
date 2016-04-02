using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace server_api.Models
{
    public class BaseEntity
    {
        [JsonIgnore]
        public DateTime? DateCreated { get; set; }
        [JsonIgnore]
        public DateTime? DateModified { get; set; }
    }
}
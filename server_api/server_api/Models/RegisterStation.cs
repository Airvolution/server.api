using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace server_api.Models
{
    public class RegisterStation
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Agency { get; set; }
        public string Purpose { get; set; }
        public Boolean Indoor { get; set; }
    }
}
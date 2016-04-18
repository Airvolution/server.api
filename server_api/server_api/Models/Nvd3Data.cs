using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace server_api.Models
{
    public class Nvd3Data
    {
        public string Key { get; set; }
        public List<object[]> Values { get; set; }
        public Nvd3Data(string pollutantName)
        {
            Key = pollutantName;
            Values = new List<object[]>();
        }
    }
}

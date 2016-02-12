using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirnowRetrieval
{
    class Models
    {
    }

    class GoogleGeo
    {
        public string city { get; set; }
        public string state { get; set; }
        public string postal { get; set; }
    }

    class StationUser
    {
        public Station station { get; set; }
        public User user { get; set; }
    }

    class AirNowDataPoint
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
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

    class DataPoint
    {
        [JsonIgnore]
        public int Id { get; set; }

        public DateTime Time { get; set; }

        public Station Station { get; set; }

        public Parameter Parameter { get; set; }

        public bool Indoor { get; set; }

        public decimal Lat { get; set; }
        public decimal Lng { get; set; }

        public double Value { get; set; }
        public int Category { get; set; }
        public int AQI { get; set; }
    }

    class Station
    {
        public string Id { get; set; } // site name

        [JsonIgnore]
        public decimal Lat { get; set; }
        [JsonIgnore]
        public decimal Lng { get; set; }

        public bool Indoor { get; set; }

        public string Agency { get; set; }

        public string Name { get; set; }

        public string Purpose { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }

        public string State { get; set; }

        public string City { get; set; }

        public string Postal { get; set; }
    }

    class Parameter
    {
        public string Name { get; set; }

        public string Unit { get; set; }
    }

    class User
    {

        public User()
        {
            DeviceGroups = new HashSet<StationGroup>();
            Devices = new HashSet<Station>();
        }

        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public string FirstName { get; set; }

        [JsonIgnore]
        public string LastName { get; set; }

        [JsonIgnore]
        public string Username { get; set; }

        public string Email { get; set; }

        [JsonIgnore]
        public string Password { get; set; }

        [JsonIgnore]
        public string ConfirmPassword { get; set; }

        [JsonIgnore]
        public virtual ICollection<StationGroup> DeviceGroups { get; set; }

        [JsonIgnore]
        public virtual ICollection<Station> Devices { get; set; }
    }

    class StationGroup
    {
        public StationGroup()
        {
            Stations = new HashSet<Station>();
        }

        public string Name { get; set; }

        public string Email { get; set; }

        public virtual User User { get; set; }

        public virtual ICollection<Station> Stations { get; set; }
    }
}

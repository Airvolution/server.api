namespace server_api
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Station
    {
        public Station()
        {            
            DataPoints = new HashSet<DataPoint>();
        }

        [StringLength(32)]
        public string Id { get; set; }

        [Required]
        public int User_Id { get; set; }

        public decimal Lat { get; set; }

        public decimal Lng { get; set; }

        public bool Indoor { get; set; }

        [StringLength(100)]
        public string Agency { get; set; }

        [StringLength(320)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Purpose { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        [StringLength(10)]
        public string State { get; set; }

        [StringLength(10)]
        public string Postal { get; set; }

        [JsonIgnore]
        public virtual ICollection<DataPoint> DataPoints { get; set; }

        public virtual User User { get; set; }

    }
}

namespace server_api.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Station : BaseEntity
    {
        public Station()
        {            
            DataPoints = new HashSet<DataPoint>();
        }

        [StringLength(32)]
        public string Id { get; set; }

        [Required]
        [JsonIgnore]
        [ForeignKey("User")]
        public string User_Id { get; set; }
        [JsonIgnore]
        public virtual User User { get; set; }

        public int AQI { get; set; }

        [ForeignKey("Parameter")]
        [Column(Order = 0)]
        [StringLength(30)]
        [JsonIgnore]
        public string Parameter_Name { get; set; }

        [ForeignKey("Parameter")]
        [Column(Order = 1)]
        [StringLength(30)]
        [JsonIgnore]
        public string Parameter_Unit { get; set; }

        public virtual Parameter Parameter { get; set; }

        [JsonConverter(typeof(DbGeographyConverter))]
        public DbGeography Location { get; set; }
        

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

        [JsonIgnore]
        public virtual ICollection<Daily> Dailies { get; set; }

        

    }
}

namespace server_api.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DataPoint : BaseEntity
    {
        [Key]
        [Column(Order = 0)]
        public DateTime Time { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(32)]
        [JsonIgnore]
        public string Station_Id { get; set; }
        public virtual Station Station { get; set; }

        [Key]
        [ForeignKey("Parameter")]
        [Column(Order = 2)]
        [StringLength(30)]
        [JsonIgnore]
        public string Parameter_Name { get; set; }

        [Key]
        [ForeignKey("Parameter")]
        [Column(Order = 3)]
        [StringLength(30)]
        [JsonIgnore]
        public string Parameter_Unit { get; set; }
        public virtual Parameter Parameter { get; set; }

        [JsonConverter(typeof(DbGeographyConverter))]
        public DbGeography Location { get; set; }

        public double Value { get; set; }

        public int Category { get; set; }

        public int AQI { get; set; }        

        public bool Indoor { get; set; }
    }
}

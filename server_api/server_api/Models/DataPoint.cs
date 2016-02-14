namespace server_api
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DataPoint
    {
        [Key]
        [Column(Order = 0)]
        public DateTime Time { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(32)]
        [JsonIgnore]
        public string Station_Id { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(30)]
        [JsonIgnore]
        public string Parameter_Name { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(30)]
        [JsonIgnore]
        public string Parameter_Unit { get; set; }

        public decimal Lat { get; set; }

        public decimal Lng { get; set; }

        public double Value { get; set; }

        public int Category { get; set; }

        public int AQI { get; set; }

        public virtual Station Station { get; set; }

        public virtual Parameter Parameter { get; set; }

        public bool Indoor { get; set; }
    }
}

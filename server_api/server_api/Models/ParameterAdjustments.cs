namespace server_api.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ParameterAdjustment
    {

        [Key]
        [ForeignKey("Station")]
        [Column(Order = 0)]
        [StringLength(32)]
        [JsonIgnore]
        public string Station_Id { get; set; }

        [Key]
        [ForeignKey("Parameter")]
        [Column(Order = 1)]
        [StringLength(30)]
        [JsonIgnore]
        public string Parameter_Name { get; set; }

        [Key]
        [ForeignKey("Parameter")]
        [Column(Order = 2)]
        [StringLength(30)]
        [JsonIgnore]
        public string Parameter_Unit { get; set; }

        public double ScaleFactor { get; set; }
        public double ShiftFactor { get; set; }

        public virtual Station Station { get; set; }
        public virtual Parameter Parameter { get; set; }

    }
}
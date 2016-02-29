namespace server_api
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Parameter
    {
        public Parameter()
        {
            DataPoints = new HashSet<DataPoint>();
        }

        [Key]
        [Column(Order = 0)]
        [StringLength(30)]
        public string Name { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(30)]
        public string Unit { get; set; }

        [JsonIgnore]
        public virtual ICollection<DataPoint> DataPoints { get; set; }
    }
}

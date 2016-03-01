namespace server_api.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Daily : BaseEntity
    {
        [Key]
        [Column(Order = 0)]
        public DateTime Date { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(32)]
        [JsonIgnore]
        public string Station_Id { get; set; }

        public int MaxCategory { get; set; }

        public int MinCategory { get; set; }

        public int MaxAQI { get; set; }

        public double AvgAQI { get; set; }

        public int MinAQI { get; set; }

        public virtual Station Station { get; set; }

        public virtual Parameter MaxParameter { get; set; }

        public virtual Parameter MinParameter { get; set; }

    }
}

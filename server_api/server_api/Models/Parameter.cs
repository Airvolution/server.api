namespace server_api.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Parameter : BaseEntity
    {
        public Parameter()
        {
            DataPoints = new HashSet<DataPoint>();
            UserPreferences = new HashSet<UserPreferences>();
        }

        [Key]
        [Column(Order = 0)]
        [StringLength(30)]
        public string Name { get; set; }

        [StringLength(30)]
        public string Unit { get; set; }

        [JsonIgnore]
        public virtual ICollection<DataPoint> DataPoints { get; set; }

        [JsonIgnore]
        public virtual ICollection<UserPreferences> UserPreferences { get; set; }
    }
}

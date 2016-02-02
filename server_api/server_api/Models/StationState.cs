namespace server_api
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class StationState
    {
        [Key]
        [Column(Order = 1)]
        public DateTime StateTime { get; set; }

        public decimal Lat { get; set; }

        public decimal Long { get; set; }

        public bool InOrOut { get; set; }

        public bool StatePrivacy { get; set; }

        [Key]
        public virtual Station Station { get; set; }
    }
}

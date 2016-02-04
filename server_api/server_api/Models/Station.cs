namespace server_api
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Station
    {
        [StringLength(32)]
        public string Id { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }

        [StringLength(100)]
        public string Agency { get; set; }

        [Required]
        [StringLength(320)]
        public string Name { get; set; }

        [Required]
        [StringLength(1000)]
        public string Purpose { get; set; }

        public virtual User User { get; set; }

    }
}

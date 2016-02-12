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
        public bool Indoor { get; set; }

        [StringLength(100)]
        public string Agency { get; set; }

        [Required]
        [StringLength(320)]
        public string Name { get; set; }

        [Required]
        [StringLength(1000)]
        public string Purpose { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        [StringLength(2)]
        public string State { get; set; }

        [StringLength(10)]
        public string Postal { get; set; }

        public virtual User User { get; set; }

    }
}

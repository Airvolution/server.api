namespace server_api
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DataPoint
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime Time { get; set; }

        public virtual Station Station { get; set; }

        public virtual Parameter Parameter { get; set; }

        public bool Indoor { get; set; }

        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        
        public double Value { get; set; }
        public int Category { get; set; }
        public int AQI { get; set; }
    }
}

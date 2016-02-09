namespace server_api
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Parameter
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(30)]
        public string Name { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(30)]
        public string Unit { get; set; }

    }
}

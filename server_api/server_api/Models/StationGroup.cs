namespace server_api.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class StationGroup : BaseEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public StationGroup()
        {
            Stations = new HashSet<Station>();
        }

        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string Name { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(100)]
        public string Email { get; set; }

        public virtual User User { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Station> Stations { get; set; }
    }
}

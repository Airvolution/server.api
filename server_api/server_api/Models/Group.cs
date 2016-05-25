namespace server_api.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Group : BaseEntity
    {
        public Group()
        {
            Stations = new HashSet<Station>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        [ForeignKey("Owner")]
        [Required]
        public string Owner_Id { get; set; }

        [JsonIgnore]
        public User Owner { get; set; }

        public ICollection<Station> Stations { get; set; }
    }
}

namespace server_api.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using Microsoft.AspNet.Identity.EntityFramework;

    [JsonObject(MemberSerialization.OptIn)]
    public partial class User : IdentityUser
    {
        public User()
        {
            Stations = new HashSet<Station>();
        }

        [JsonProperty]
        public override string Id
        {
            get
            {
                return base.Id;
            }
            set
            {
                base.Id = value;
            }
        }
        [JsonProperty]
        public override string UserName
        {
            get
            {
                return base.UserName;
            }
            set
            {
                base.UserName = value;
            }
        }

        [JsonProperty]
        [StringLength(20)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [JsonProperty]
        [StringLength(20)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [JsonProperty]
        [StringLength(100)]
        public string Email { get; set; }

        public virtual ICollection<Station> Stations { get; set; }
      }
}


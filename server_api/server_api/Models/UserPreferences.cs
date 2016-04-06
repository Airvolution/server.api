namespace server_api.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class UserPreferences
    {
        public UserPreferences()
        {
            DefaultParameters = new HashSet<Parameter>();
        }

        [Key]
        [Required]
        [ForeignKey("User")]
        [JsonIgnore]
        public string User_Id { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }

        public String DefaultMapMode { get; set; }

        public String DefaultDownloadFormat { get; set; }

        public String DefaultStationId { get; set; }

        public virtual ICollection<Parameter> DefaultParameters { get; set; }
    }
}

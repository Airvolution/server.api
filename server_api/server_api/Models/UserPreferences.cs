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

        // TODO: have this Fkey to Station table so I don't have to check on update
        public String DefaultStationId { get; set; }

        // This is where the problem is. How does a user get more than 1 parameter?
        // This adds a user_id column to Parameter table, but then once a parameter is
        // assigned to a user, no one else can use the parameter.
        public virtual ICollection<Parameter> DefaultParameters { get; set; }
    }
}

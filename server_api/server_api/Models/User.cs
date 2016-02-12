namespace server_api
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class User
    {
        public User()
        {
            Stations = new HashSet<Station>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(20)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [StringLength(20)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required]
        [StringLength(20)]
        public string Username { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [JsonIgnore]
        [StringLength(100, MinimumLength = 1)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [StringLength(100)]
        [JsonIgnore]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [JsonIgnore]
        public virtual ICollection<Station> Stations { get; set; }
    }
}

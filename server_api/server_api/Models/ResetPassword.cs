using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace server_api.Models
{
    public class ResetPassword
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace server_api.Models
{
    public class ResetPasswordCode
    {
        public ResetPasswordCode() { }
        public ResetPasswordCode(User user, string resetCode)
        {
            this.Id = Guid.NewGuid().ToString();
            this.ResetCode = resetCode;
            this.User = user;

        }   

        [Key]
        public string Id { get; set; }
        public string ResetCode { get; set; }
        [ForeignKey("User")]
        public string User_Id { get; set; }
        public virtual User User { get; set; }
    }
}
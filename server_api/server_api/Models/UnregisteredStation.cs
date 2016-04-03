namespace server_api.Models
{
    using Newtonsoft.Json;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class UnregisteredStation
    {
        [Key]
        [StringLength(32)]
        public string Id { get; set; }
    }
}
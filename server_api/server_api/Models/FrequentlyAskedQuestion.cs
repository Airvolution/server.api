using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace server_api.Models
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FrequentlyAskedQuestion
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        [Column(Order = 0)]
        public string Question { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Column(Order = 1)]
        public string Answer { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Column(Order = 3)]
        public string Section { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Column(Order = 4)]
        public string[] Keywords { get; set; }
    }
}
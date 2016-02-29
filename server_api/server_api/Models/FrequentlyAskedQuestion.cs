using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Question { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Answer { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual Section Section { get; set; }

        /// <summary>
        /// 
        /// </summary
        public virtual ICollection<Keyword> Keywords { get; set; }
    }
}
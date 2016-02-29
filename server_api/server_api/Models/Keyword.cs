using System.ComponentModel.DataAnnotations;

namespace server_api.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Keyword
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public string keyword { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace server_api.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Section
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public string Name { get; set; }
    }
}
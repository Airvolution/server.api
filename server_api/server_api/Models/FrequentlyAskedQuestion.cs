﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server_api.Models
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FrequentlyAskedQuestion : BaseEntity
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Question { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AnswerPlainText { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AnswerRichText { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual Section Section { get; set; }

        /// <summary>
        /// 
        /// </summary
        public virtual ICollection<Keyword> Keywords { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual ICollection<QuestionAnswerUserReview> UserReviews { get; set; } 
        
        /// <summary>
        /// 
        /// </summary>
        public int ViewCount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int TotalUserReviewScore { get; set; }
    }
}
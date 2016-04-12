using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace server_api.Models
{
    public class QuestionAnswerUserReview
    {
        public QuestionAnswerUserReview(string userId, int questionId, int score)
        {
            User_Id = userId;
            FrequentlyAskedQuestion_Id = questionId;
            UserReviewScore = score;
        }

        [ForeignKey("User")]
        [Key]
        [Column(Order = 0)]
        public string User_Id
        { get; set; }

        public virtual User User { get; set; }

        [ForeignKey("FrequentlyAskedQuestion")]
        [Key]
        [Column(Order = 1)]
        [Required]
        public int FrequentlyAskedQuestion_Id
        { get; set; }

        public virtual FrequentlyAskedQuestion FrequentlyAskedQuestion { get; set; }

        [Required]
        public int UserReviewScore
        { get; set; }
    }
}
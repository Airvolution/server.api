using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace server_api.Models
{
    public class QuestionAnswerUsefulness
    {
        public QuestionAnswerUsefulness(string userId, int questionId, int score)
        {
            User_Id = userId;
            FrequentlyAskedQuestion_Id = questionId;
            UserUsefulnessScore = score;
        }

        [JsonProperty]
        [ForeignKey("User")]
        [Key]
        [Column(Order = 0)]
        [Required]
        public string User_Id
        { get; set; }

        public virtual User User { get; set; }

        [JsonProperty]
        [ForeignKey("FrequentlyAskedQuestion")]
        [Key]
        [Column(Order = 1)]
        [Required]
        public int FrequentlyAskedQuestion_Id
        { get; set; }

        public virtual FrequentlyAskedQuestion FrequentlyAskedQuestion { get; set; }

        [JsonProperty]
        [Required]
        public int UserUsefulnessScore
        { get; set; }
    }
}
﻿using Newtonsoft.Json;
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
        public QuestionAnswerUsefulness(string usrId, int quesId, int score)
        {
            User_Id = usrId;
            FrequentlyAskedQuestion_Id = quesId;
            UserUsefulnessScore = score;
        }

        [JsonProperty]
        [ForeignKey("User")]
        [Key]
        [Column(Order = 0)]
        public string User_Id
        { get; set; }

        public virtual User User { get; set; }

        [JsonProperty]
        [ForeignKey("FrequentlyAskedQuestion")]
        [Key]
        [Column(Order = 1)]
        public int FrequentlyAskedQuestion_Id
        { get; set; }

        public virtual FrequentlyAskedQuestion FrequentlyAskedQuestion { get; set; }

        [JsonProperty]
        public int UserUsefulnessScore
        { get; set; }
    }
}
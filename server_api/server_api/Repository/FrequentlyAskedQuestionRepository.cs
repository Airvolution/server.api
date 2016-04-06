using server_api.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace server_api.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class FrequentlyAskedQuestionRepository : IDisposable
    {
        private ApplicationContext db;

        /// <summary>
        /// 
        /// </summary>
        public FrequentlyAskedQuestionRepository()
        {
            db = new ApplicationContext();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="existingContext"></param>
        public FrequentlyAskedQuestionRepository(ApplicationContext existingContext)
        {
            db = existingContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FrequentlyAskedQuestion> GetAllQuestionsAnswers()
        {
            var faqs = db.FrequentlyAskedQuestions;

            foreach(var faq in faqs)
            {
                faq.AnswerMarkDown = MarkdownToHtml(faq.AnswerMarkDown);
            }

            return faqs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="markdown"></param>
        /// <returns></returns>
        private string MarkdownToHtml(string markdown)
        {
            string result = CommonMark.CommonMarkConverter.Convert(markdown);
            result = result.Replace("\r\n", "");

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="questionId"></param>
        /// <returns></returns>
        public bool QuestionExists(string questionId)
        {
            if (db.FrequentlyAskedQuestions.Find(questionId) == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="questionId"></param>
        /// <returns></returns>
        public bool IncrementViewCount(string questionId)
        {
            if (QuestionExists(questionId))
            {
                var faq = db.FrequentlyAskedQuestions.Find(questionId);
                faq.ViewCount++;
                db.SaveChanges();

                return true;
            }
            else
            {
                return false;
            } 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool UsefulnessScoreExists(int questionId, string userId)
        {
            if (db.QuestionAnswerUsefulness.Find(questionId, userId) == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="userId"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public void AddUsefulnessScore(QuestionAnswerUsefulness review)
        {
            // Update existing user usefulness review.
            if (UsefulnessScoreExists(review.FrequentlyAskedQuestion_Id, review.User_Id))
            {
                var usefulnessReview = db.QuestionAnswerUsefulness.Find(review.FrequentlyAskedQuestion_Id);
                usefulnessReview = review;
                db.SaveChanges();
            }
            else
            {
                // Add user usefulness review.
                db.QuestionAnswerUsefulness.Add(review);
                db.SaveChanges();
            } 
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
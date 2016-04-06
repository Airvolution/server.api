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
        private ApplicationContext _ctx;

        /// <summary>
        /// 
        /// </summary>
        public FrequentlyAskedQuestionRepository()
        {
            _ctx = new ApplicationContext();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        public FrequentlyAskedQuestionRepository(ApplicationContext ctx)
        {
            _ctx = ctx;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FrequentlyAskedQuestion> GetAllQuestionsAnswers()
        {
            var faqs = _ctx.FrequentlyAskedQuestions;

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
        public bool QuestionExists(int questionId)
        {
            if (_ctx.FrequentlyAskedQuestions.Find(questionId) == null)
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
        public bool IncrementViewCount(int questionId)
        {
            if (QuestionExists(questionId))
            {
                var faq = _ctx.FrequentlyAskedQuestions.Find(questionId);
                faq.ViewCount++;
                _ctx.SaveChanges();

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
            if (_ctx.QuestionAnswerUsefulness.Find(questionId, userId) == null)
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
                var usefulnessReview = _ctx.QuestionAnswerUsefulness.Find(review.FrequentlyAskedQuestion_Id);
                usefulnessReview = review;
                _ctx.SaveChanges();
            }
            else
            {
                // Add user usefulness review.
                _ctx.QuestionAnswerUsefulness.Add(review);
                _ctx.SaveChanges();
            } 
        }

        public void Dispose()
        {
            _ctx.Dispose();
        }
    }
}
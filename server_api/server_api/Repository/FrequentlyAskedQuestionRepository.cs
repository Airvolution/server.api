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
                faq.AnswerRichText = MarkdownToHtml(faq.AnswerRichText);
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
        public bool IncrementViewCount(int questionId)
        {
            FrequentlyAskedQuestion faq =  _ctx.FrequentlyAskedQuestions.Find(questionId);

            if (faq != null)
            {
                faq.ViewCount++;
                _ctx.SaveChanges();

                return true;
            }
   
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="userId"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public void AddUsefulnessScore(QuestionAnswerUserReview review)
        {
            FrequentlyAskedQuestion faq = _ctx.FrequentlyAskedQuestions.Find(review.FrequentlyAskedQuestion_Id);

            // Update existing user usefulness review.
            if (faq != null)
            {
                faq.UserReviews.Add(review);
                _ctx.SaveChanges();
            }
        }

        public void Dispose()
        {
            _ctx.Dispose();
        }
    }
}
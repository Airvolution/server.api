using server_api.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Web.Mvc;
using System.Linq;

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

                foreach(var review in faq.UserReviews)
                {
                    faq.TotalUserReviewScore += review.UserReviewScore;
                }
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
        public void IncrementViewCount(int questionId)
        {
            FrequentlyAskedQuestion faq =  _ctx.FrequentlyAskedQuestions.Find(questionId);

            if (faq != null)
            {
                faq.ViewCount++;
                _ctx.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="userId"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        [Authorize]
        public void AddUsefulnessScore(QuestionAnswerUserReview review)
        {
            FrequentlyAskedQuestion faq = _ctx.FrequentlyAskedQuestions.Find(review.FrequentlyAskedQuestion_Id);

            // Update existing user usefulness review.
            if (faq != null)
            {
                ICollection<QuestionAnswerUserReview> reviews = faq.UserReviews;

                var existingReviews = reviews.Where(e => e.User_Id == review.User_Id && e.FrequentlyAskedQuestion_Id == review.FrequentlyAskedQuestion_Id);
                QuestionAnswerUserReview existingReview = existingReviews.FirstOrDefault();

                if (existingReview == null)
                {
                    // No existing Review. Add new review.
                    faq.UserReviews.Add(review);
                    faq.TotalUserReviewScore += review.UserReviewScore;
                }
                else
                {
                    // Existing review. Update review.
                    faq.UserReviews.Remove(existingReview);
                    faq.TotalUserReviewScore += -existingReview.UserReviewScore;
                    faq.UserReviews.Add(review);
                    faq.TotalUserReviewScore += review.UserReviewScore;
                }
                _ctx.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FrequentlyAskedQuestion> getTop5Viewed()
        {
            var faqs = _ctx.FrequentlyAskedQuestions.OrderByDescending(x => x.ViewCount).Take(5);

            return faqs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FrequentlyAskedQuestion> getTop5Rated()
        {
            var faqs = _ctx.FrequentlyAskedQuestions.OrderByDescending(x => x.TotalUserReviewScore).Take(5);

            return faqs;
        }

        public void Dispose()
        {
            _ctx.Dispose();
        }
    }
}
﻿using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using server_api.Models;
using server_api.Repository;
using System.Web.Http.Description;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace server_api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class FrequentlyAskedQuestionsController : ApiController
    {
        private FrequentlyAskedQuestionRepository _faqRepo = null;
        private UserRepository _userRepo = null;

        /// <summary>
        /// 
        /// </summary>
        public FrequentlyAskedQuestionsController()
        {
            _faqRepo = new FrequentlyAskedQuestionRepository();
            _userRepo = new UserRepository();
        }

        /// <summary>
        ///   Returns an array of frequently asked questions.
        /// </summary>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<FrequentlyAskedQuestion>))]
        [Route("faq")]
        [HttpGet]
        public IHttpActionResult FrequentlyAskedQuestionsList()
        {
            // get all datapoints matching the station ids and parameter types
            IEnumerable<FrequentlyAskedQuestion> questionsAnswers = _faqRepo.GetAllQuestionsAnswers();

            return Ok(questionsAnswers);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="questionId"></param>
        /// <returns></returns>
        [Route("faq/view")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        public IHttpActionResult IncrementViewCount([FromBody]int questionId)
        {
            // increment the total view count of a question.
            _faqRepo.IncrementViewCount(questionId);

            return Ok();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("faq/usefulnessReview")]
        [Authorize]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        public async Task<IHttpActionResult> AddUserUsefulnessReview([FromBody]int questionId, int score)
        {
            User user = await _userRepo.FindUserById(RequestContext.Principal.Identity.GetUserId());
            if (user == null)
            {
                return Unauthorized();
            }

            var review = new QuestionAnswerUsefulness(user.Id, questionId, score);
            _faqRepo.AddUsefulnessScore(review);

            return Ok();
        }
    }
}
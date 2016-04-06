using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Http;
using server_api.Models;
using server_api.Repository;
using System.Web.Http.Description;
using Swashbuckle.Swagger.Annotations;

namespace server_api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class FrequentlyAskedQuestionsController : ApiController
    {
        private FrequentlyAskedQuestionRepository _repo = null;

        /// <summary>
        /// 
        /// </summary>
        public FrequentlyAskedQuestionsController()
        {
            _repo = new FrequentlyAskedQuestionRepository();
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
            IEnumerable<FrequentlyAskedQuestion> questionsAnswers = _repo.GetAllQuestionsAnswers();

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
            _repo.IncrementViewCount(questionId);

            return Ok();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("faq/usefulnessReview")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        public IHttpActionResult AddUserUsefulnessReview([FromBody]QuestionAnswerUsefulness review)
        {
            _repo.AddUsefulnessScore(review);

            return Ok();
        }
    }
}
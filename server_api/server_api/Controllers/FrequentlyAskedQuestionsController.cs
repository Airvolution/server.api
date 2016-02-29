using System.Collections.Generic;
using System.Web.Http;
using server_api.Models;
using server_api.Repository;

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
        [Route("faq")]
        [HttpGet]
        public IHttpActionResult FrequentlyAskedQuestions()
        {
            // get all datapoints matching the station ids and parameter types
            IEnumerable<FrequentlyAskedQuestion> questionsAnswers = _repo.GetAllQuestionsAnswers();

            return Ok(questionsAnswers);
        }
    }
}
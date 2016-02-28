using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using server_api.Models;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using server_api.Repository;

namespace server_api.Controllers
{
    public class FrequentlyAskedQuestionsController : ApiController
    {
        private FrequentlyAskedQuestionRepository _repo = null;

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
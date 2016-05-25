using System.Collections.Generic;
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
        private FrequentlyAskedQuestionRepository _faqRepo;
        private UserRepository _userRepo;
         
        /// <summary>
        /// 
        /// </summary>
        public FrequentlyAskedQuestionsController()
        {
            ApplicationContext ctx = new ApplicationContext();

            _faqRepo = new FrequentlyAskedQuestionRepository(ctx);
            _userRepo = new UserRepository(ctx);
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
        [Route("faq/{questionId}/view")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        public IHttpActionResult IncrementViewCount([FromUri]int questionId)
        {
            // increment the total view count of a question.
            _faqRepo.IncrementViewCount(questionId);

            return Ok();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("faq/{questionId}/userReview/{score}")]
        [Authorize]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        public async Task<IHttpActionResult> AddUserUsefulnessReview([FromUri]int questionId, [FromUri]int score)
        {
            User user = await _userRepo.FindUserById(RequestContext.Principal.Identity.GetUserId());
            if (user == null)
            {
                return Unauthorized();
            }

            var review = new QuestionAnswerUserReview(user.Id, questionId, score);
            _faqRepo.AddUsefulnessScore(review);

            return Ok();
        }

        /// <summary>
        ///   Returns an array of the top 5 most viewed frequently asked questions.
        /// </summary>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<FrequentlyAskedQuestion>))]
        [Route("faq/top5Viewed")]
        [HttpGet]
        public IHttpActionResult Top5MostViewedList()
        {
            // get all datapoints matching the station ids and parameter types
            IEnumerable<FrequentlyAskedQuestion> questionsAnswers = _faqRepo.getTop5Viewed();

            return Ok(questionsAnswers);
        }

        /// <summary>
        ///   Returns an array of the top 5 best user rated frequently asked questions.
        /// </summary>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<FrequentlyAskedQuestion>))]
        [Route("faq/top5Rated")]
        [HttpGet]
        public IHttpActionResult Top5HighestRatedList()
        {
            // get all datapoints matching the station ids and parameter types
            IEnumerable<FrequentlyAskedQuestion> questionsAnswers = _faqRepo.getTop5Rated();

            return Ok(questionsAnswers);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using server_api.Models;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System.Web.Http.ModelBinding;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.IO;
using System.Web.Hosting;
using server_api.Providers;

namespace server_api.Controllers
{
    /// <summary>
    ///   User API end points.
    /// </summary>
    [RoutePrefix("users")]
    public class UsersController : ApiController
    {
        private UserRepository _repo;
        private StationsRepository _stationsRepo;

        public UsersController()
        {
            ApplicationContext ctx = new ApplicationContext();
            _repo = new UserRepository(ctx);
            _stationsRepo = new StationsRepository(ctx);
        }


        /// <summary>
        ///   This is a testing method. 
        ///   
        ///   This method simply returns successful.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("authtest")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult ServerTest()
        {
            return Ok("Success");
        }

        /// <summary>
        /// This method returns the current user, if authorized
        /// </summary>
        /// <returns>User</returns>
        [Authorize]
        [Route("current")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(string))]
        public async Task<IHttpActionResult> GetCurrentUser()
        {
            User user = await _repo.FindUserById(RequestContext.Principal.Identity.GetUserId());
            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return BadRequest("Unable to find current user");
            }
        }

        [Authorize]
        [Route("current")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> UpdateUserProfile([FromBody]UserProfile user)
        {
            
            User result = _repo.UpdateUser(RequestContext.Principal.Identity.GetUserId(), user);
            if (result != null)
            {
                if (user.Email != RequestContext.Principal.Identity.Name)
                {
                    User userAccount = await _repo.FindUserById(RequestContext.Principal.Identity.GetUserId());
                    string confirmationLink = "http://" + Request.RequestUri.Host + ":" + Request.RequestUri.Port + Url.Route("ConfirmEmail", new { code = userAccount.Id});
                    SendEmailConfirmationEmail(userAccount, confirmationLink);
                }
                return Ok(result);
            }
            return InternalServerError();
            
        }


        [Authorize]
        [Route("current/password")]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> ResetUserPassword([FromBody]ResetPassword reset)
        {
            User user = await _repo.FindUser(RequestContext.Principal.Identity.GetUserName(), reset.CurrentPassword);
            bool authorized = user != null && user.Id.Equals(RequestContext.Principal.Identity.GetUserId());
            if (!authorized)
            {
                return Unauthorized();
            }
            if (reset.Password == null)
            {
                return BadRequest();
            }
            bool succeeded  = await _repo.UpdateUserPassword(RequestContext.Principal.Identity.GetUserId(), reset.Password);
            if (succeeded)
            {
                return Ok();
            }
            else
            {
                return InternalServerError();
            }
        }

        [Route("password/reset")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK)]
        public async Task<IHttpActionResult> SendResetEmail([FromUri]string email)
        {
            User user = await _repo.FindUserByEmail(email);
            if (user == null )
            {
                return BadRequest();
            }
            string code = await _repo.GeneratePasswordResetCode(user);
            if (code == "")
            {
                return InternalServerError();
            }
            string callbackUrl = "http://"+Request.RequestUri.Host +":"+Request.RequestUri.Port + Url.Route("ResetPassword", new {code = code });
            bool succeeded = await SendPasswordResetEmail(user,callbackUrl);
            if (succeeded)
            {
                return Ok();
            }
            return InternalServerError();
        }

        [AllowAnonymous]
        [Route("reset", Name = "ResetPassword")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.Moved)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public async Task<IHttpActionResult> ResetPassword(string code)
        {
            if (await _repo.ResetPasswordWithCode(code))
            {
                IHttpActionResult result;
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Moved);
                //TODO Fix this so it works when deployed
                response.Headers.Location = new Uri("http://localhost:8084/#/modal/password/reset/complete");
                result = ResponseMessage(response);
                return result;
            }
            else
            {
                return BadRequest();
            }
            
        }

        [AllowAnonymous]
        [Route("confirm/email",Name= "ConfirmEmail")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.Moved)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public async Task<IHttpActionResult> ConfirmPassword(string code)
        {
            User user = await _repo.FindUserById(code);
            if (user == null)
            {
                return BadRequest();
            }

           bool succeeded = await _repo.SetEmailConfirmed(user,true);
           if (succeeded)
           {
               IHttpActionResult result;
               HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Moved);
               //TODO Fix this so it works when deployed
               response.Headers.Location = new Uri("http://localhost:8084/#/modal/email/confirmed");
               result = ResponseMessage(response);
               return result;
           }
           return BadRequest();
        }

        [Authorize]
        [Route("preferences")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(UserPreferences))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public async Task<IHttpActionResult> GetUserPreferences()
        {
            User user = await _repo.FindUserById(RequestContext.Principal.Identity.GetUserId());
            if (user == null)
            {
                return Unauthorized();
            }
            UserPreferences prefs = _repo.GetUserPreferences(user.Id);
            return Ok(prefs);
        }

        [Authorize]
        [Route("preferences")]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(UserPreferences))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public async Task<IHttpActionResult> UpdateUserPreferences([FromBody]UserPreferences prefs)
        {
            // Expects the Parameter List to be SPACE separated string "NAME UNIT" eg. "PM2.5 UG/M3"
            prefs.User_Id = RequestContext.Principal.Identity.GetUserId();


            if (!_repo.IsValidPreferences(prefs.DefaultMapMode, prefs.DefaultDownloadFormat))
            {
                return BadRequest();
            }

            var updatedPreferences = _repo.UpdateUserPreferences(prefs);
            return Ok(updatedPreferences);
        }

        [AllowAnonymous]
        [Route("register")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> Register(RegisterUser userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            IdentityResult result = await _repo.RegisterUser(userModel);
            IHttpActionResult error = GetErrorResult(result);
            if (error != null)
            {
                return error;
            }
            User user = await _repo.FindUserByEmail(userModel.Email);
            if(user == null){
                return InternalServerError();
            }
            string confirmationLink = "http://"+Request.RequestUri.Host+":"+Request.RequestUri.Port+ Url.Route("ConfirmEmail", new {code = user.Id });
            SendEmailConfirmationEmail(user, confirmationLink);
            return Ok(user);
        }

        [Authorize]
        [Route("stations")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<Station>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetUserStations()
        {
            var stations = _stationsRepo.GetUserStations(RequestContext.Principal.Identity.GetUserId());
            return Ok(stations);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repo.Dispose();
            }
            base.Dispose(disposing);
        }

        private async Task<bool> SendPasswordResetEmail(User user, string resetLink)
        {
            string emailPath = HostingEnvironment.MapPath("~/EmailTemplates/PasswordResetEmail.html");
            string email = File.ReadAllText(emailPath);
            string linkPattern = @"(<a\s+id=""passwordResetButton""[\s\S]+?href="")(.*?)(""[\s\S]+?>)";
            email = Regex.Replace(email, linkPattern, "$1" + resetLink + "$3");
            return await MessageProvider.SendEmailToUserAsync(user, "Reset your password", email);
        }
        private async Task<bool> SendEmailConfirmationEmail(User user,string confirmationLink)
        {
            string emailPath = HostingEnvironment.MapPath("~/EmailTemplates/ConfirmEmailEmail.html");
            string email = File.ReadAllText(emailPath);
            string linkPattern = @"(<a\s+id=""confirmEmailBtn""[\s\S]+?href="")(.*?)(""[\s\S]+?>)";
            email = Regex.Replace(email, linkPattern, "$1" + confirmationLink + "$3");
            return await MessageProvider.SendEmailToUserAsync(user, "Please confirm your email address", email);
        }
        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }
    }
}

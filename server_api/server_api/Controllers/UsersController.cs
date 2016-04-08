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

namespace server_api.Controllers
{
    /// <summary>
    ///   User API end points.
    /// </summary>
    [RoutePrefix("users")]
    public class UsersController : ApiController
    {
        private UserRepository _repo = null;

        public UsersController()
        {
            _repo = new UserRepository();
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
        public IHttpActionResult UpdateUserProfile([FromBody]UserProfile user)
        {

            User result = _repo.UpdateUser(RequestContext.Principal.Identity.GetUserId(), user);
            if (result != null)
            {
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

        [Authorize]
        [Route("current/email")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK)]
        public async Task<IHttpActionResult> SendResetEmail()
        {
            User user = await _repo.FindUserById(RequestContext.Principal.Identity.GetUserId());
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
            string emailPath = HostingEnvironment.MapPath("~/EmailTemplates/PasswordResetEmail.html");
            string email = File.ReadAllText(emailPath);
            email = SetEmailResetLink(email, callbackUrl);
            bool succeeded = await _repo.SendPasswordResetEmail(user, email);
            if (succeeded)
            {
                return Ok();
            }
            return InternalServerError();
        }

        [AllowAnonymous]
        [Route("password/reset", Name = "ResetPassword")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.Moved)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public async Task<IHttpActionResult> ResetPassword(string code)
        {
            if (await _repo.ResetPasswordWithCode(code))
            {
                IHttpActionResult result;
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Moved);
                response.Headers.Location = new Uri("http://localhost:8084");
                result = ResponseMessage(response);
                return result;
            }
            else
            {
                return BadRequest();
            }
            
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
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repo.Dispose();
            }
            base.Dispose(disposing);
        }
        private string SetEmailLogo(String email, string imageUrl)
        {
            string srcPattern = @"(<img\s+id=""airlogo""[\s\S]+?src="")(.*?)(""[\s\S]+?>)";
            return Regex.Replace(email,srcPattern,"$1"+imageUrl+"$3");
        }
        private string SetEmailResetLink(string email,string callbackUrl)
        {
            string linkPattern = @"(<a\s+id=""passwordResetButton""[\s\S]+?href="")(.*?)(""[\s\S]+?>)";
            return Regex.Replace(email, linkPattern, "$1" + callbackUrl + "$3");
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

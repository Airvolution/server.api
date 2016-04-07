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

            return Ok(_repo.GetUserPreferences(user.Id));
        }

        [Authorize]
        [Route("preferences")]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(UserPreferences))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public async Task<IHttpActionResult> UpdateUserPreferences([FromUri]String mapMode, [FromUri]String downloadFormat, [FromUri]String stationId, [FromUri]String[] parameters)
        {
            // Expects the Parameter List to be SPACE separated string "NAME UNIT" eg. "PM2.5 UG/M3"

            User user = await _repo.FindUserById(RequestContext.Principal.Identity.GetUserId());
            if (user == null)
            {
                return Unauthorized();
            }

            if (!_repo.IsValidPreferences(mapMode, downloadFormat))
            {
                return BadRequest();
            }

            var updatedPreferences = _repo.UpdateUserPreferences(user.Id, mapMode, downloadFormat, stationId, parameters);
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

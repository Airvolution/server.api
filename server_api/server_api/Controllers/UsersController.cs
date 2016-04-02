using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using server_api.Models;

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


        [AllowAnonymous]
        [Route("register")]
        public async Task<IHttpActionResult> Register(UserRegistration userModel)
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using server_api.Models;

namespace server_api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class UsersController : ApiController
    {
        /// <summary>
        ///   Validates user is not already in database and if not, creates new User in database.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Route("users/register")]
        [HttpPost]
        public IHttpActionResult Register([FromBody]SwaggerUser user)
        {
            var db = new AirUDBCOE();

            User existingUser = db.Users.SingleOrDefault(x => x.Email == user.email);

            if (existingUser == null)
            {
                // Perform queries to insert new user into database.
                User newUser = new User();
                newUser.Email = user.email;
                newUser.Pass = user.pass;

                db.Users.Add(newUser);
                db.SaveChanges();

                // Account register success.
                return Ok("Account registration successful! Welcome, " + user.email);
            }
            else
            {
                // Account register failed. Account with email address: '<user.Email>' already exists. Please try a different email address.
                return BadRequest("Account registration failed! Account with email address: " +
                                                                             user.email +
                                                                             " already exists. Please try a different email address.");
            }
        }

        /// <summary>
        ///   Validates user based on Email and Pass.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Route("users/login")]
        [HttpPost]
        public IHttpActionResult Login([FromBody]SwaggerUser user)
        {
            var db = new AirUDBCOE();

            User validUserAndPass = db.Users.SingleOrDefault(x => x.Email == user.email && x.Pass == user.pass);

            if (validUserAndPass != null)
            {
                // Login success.
                return Ok("Login Successful! Welcome, " + user.email);
            }
            else
            {
                // Login fail.
                return BadRequest("Login failed! Please check email and password.");
            }
        }
    }
}
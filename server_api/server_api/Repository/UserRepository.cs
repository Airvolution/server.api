using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using server_api;
using server_api.Models;

namespace server_api
{
    public class UserRepository : IDisposable
    {
        private ApplicationContext _ctx;

        private UserManager<User> _userManager;

        public UserRepository()
        {
            _ctx = new ApplicationContext();
            _userManager = new UserManager<User>(new UserStore<User>(_ctx));
        }

        public async Task<IdentityResult> RegisterUser(UserRegistration registration)
        {
            User user = new User
            {
                UserName = registration.Email,
                Email = registration.Email,
                
            };
            var result = await _userManager.CreateAsync(user, registration.Password);

            return result;
        }

        public async Task<User> FindUserById(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            return user;
        }

        public async Task<User> FindUser(string userName, string password)
        {
            User user = await _userManager.FindAsync(userName, password);

            return user;
        }

        public void Dispose()
        {
            _ctx.Dispose();
            _userManager.Dispose();

        }
    }
}
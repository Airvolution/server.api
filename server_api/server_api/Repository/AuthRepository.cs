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
    public class AuthRepository : IDisposable
    {
        private AirUDBCOE _ctx;

        private UserManager<User> _userManager;

        public AuthRepository()
        {
            _ctx = new AirUDBCOE();
            _userManager = new UserManager<User>(new UserStore<User>(_ctx));
        }

        public async Task<IdentityResult> RegisterUser(User user)
        {
            var result = await _userManager.CreateAsync(user, user.Password);

            return result;
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
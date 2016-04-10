using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using server_api;
using server_api.Models;
using server_api.Providers;
using System.Runtime.Remoting.Contexts;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.ObjectModel;

namespace server_api
{
    public class UserRepository : IDisposable
    {
        private ApplicationContext _ctx;

        private UserManager<User> _userManager;
        private UserStore<User> _userStore;

        public UserRepository()
        {
            _ctx = new ApplicationContext();
            var provider = new DpapiDataProtectionProvider("Airvolution");
            _userStore = new UserStore<User>(_ctx);
            _userManager = new UserManager<User>(_userStore);
            _userManager.UserTokenProvider = new DataProtectorTokenProvider<User>(provider.Create("EmailCode"));
        }

        public UserRepository(ApplicationContext ctx)
        {
            _ctx = ctx;
            _userStore = new UserStore<User>(ctx);
            _userManager = new UserManager<User>(_userStore);
        }

        public async Task<IdentityResult> RegisterUser(RegisterUser registration)
        {
            User user = new User
            {
                UserName = registration.Email,
                Email = registration.Email,

            };
            var result = await _userManager.CreateAsync(user, registration.Password);
            return result;
        }

        public async Task<string> GeneratePasswordResetCode(User user)
        {
            if (user == null)
            {
                return "";
            }
            string orginialCode = await _userManager.GeneratePasswordResetTokenAsync(user.Id);
            ResetPasswordCode shortCode = new ResetPasswordCode(user, orginialCode);
            _ctx.ResetPasswordCodes.Add(shortCode);
            _ctx.SaveChanges();
            return shortCode.Id;
        }

        public async Task<bool> ResetPasswordWithCode(string resetCode)
        {
            ResetPasswordCode code = _ctx.ResetPasswordCodes.Find(resetCode);
            if (code == null)
            {
                return false;
            }
            var result = await _userManager.ResetPasswordAsync(code.User_Id, code.ResetCode, "CleanAir");
            if (result.Succeeded)
            {
                _ctx.ResetPasswordCodes.Remove(code);
                _ctx.SaveChanges();
                return true;
            }
            return false;
        }

        public async Task<bool> SetEmailConfirmed(User user,bool confirmed)
        {
            _userStore.SetEmailConfirmedAsync(user, confirmed);
            await _userStore.UpdateAsync(user);
            return true;
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

        public async Task<User> FindUserByEmail(string email)
        {
            User user = await _userManager.FindByEmailAsync(email);
            return user;
        }

        public User UpdateUser(string id, UserProfile user)
        {
            User existing =  _userManager.FindById(id);
            if (existing == null)
            {
                return null;
            }

            existing.FirstName = user.FirstName;
            existing.LastName = user.LastName;
            existing.Email = user.Email;
            existing.UserName = user.Email;
            IdentityResult result = _userManager.Update(existing);
            if (result.Succeeded)
            {
                return existing;
            }
            return null;

        }

        public async Task<bool> UpdateUserPassword(string id, string password)
        {
            User existing = await _userManager.FindByIdAsync(id);
            if (existing == null)
            {
                return false;
            }
            string hash = _userManager.PasswordHasher.HashPassword(password);
            await _userStore.SetPasswordHashAsync(existing,hash);
            IdentityResult result = await _userManager.UpdateAsync(existing);
            if (result.Succeeded)
            {
                return true;
            }
            return false;
            

        }

        public Boolean IsValidPreferences(String mapMode, String downloadFormat)
        {
            switch (mapMode.ToUpper())
            {
                case "LIGHT":
                    break;
                case "DARK":
                    break;
                case "SATELLITE":
                    break;
                default:
                    return false;
            }

            switch (downloadFormat.ToUpper())
            {
                case "CSV":
                    break;
                case "JSON":
                    break;
                default:
                    return false;
            }

            return true;
        }

        public UserPreferences GetUserPreferences(string id)
        {
            UserPreferences data = _ctx.UserPreferences
                                        .Where(u => id.Equals(u.User_Id))
                                        .FirstOrDefault();

            if (data == null) { return CreateDefaultUserPreferences(id); }

            return data;
        }

        public UserPreferences CreateDefaultUserPreferences(string user_id){
            UserPreferences prefs = new UserPreferences();
            prefs.User_Id = user_id;
            prefs.DefaultMapMode = "LIGHT";
            prefs.DefaultDownloadFormat = "CSV";
            _ctx.UserPreferences.Add(prefs);
            _ctx.SaveChanges();
            return prefs;
        }

        public UserPreferences UpdateUserPreferences(UserPreferences prefs)
        {
            var existingPreferences = _ctx.UserPreferences.Include("DefaultParameters")
                                            .Single(u => prefs.User_Id == u.User_Id);

            existingPreferences.DefaultMapMode = prefs.DefaultMapMode;
            existingPreferences.DefaultDownloadFormat = prefs.DefaultDownloadFormat;
            existingPreferences.DefaultStationId = prefs.DefaultStationId;

            // Find matching parameters
            IEnumerable<string> paramNames = from tmp in prefs.DefaultParameters select tmp.Name;
            IEnumerable<Parameter> defaultParams = from parameter in _ctx.Parameters where paramNames.Contains(parameter.Name)  select parameter;

            existingPreferences.DefaultParameters = new Collection<Parameter>(defaultParams.ToList());

            _ctx.SaveChanges();

            return existingPreferences;
        }

        public void Dispose()
        {
            _ctx.Dispose();
            _userManager.Dispose();

        }
    }
}

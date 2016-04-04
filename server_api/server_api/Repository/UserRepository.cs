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

        public Boolean IsValidPreferences(UserPreferences preferences)
        {
            Station station = _ctx.Stations
                                    .Where(s => preferences.DefaultStationId.Equals(s.Id)).FirstOrDefault();

            if (station != null)
            {
                switch (preferences.DefaultMapMode.ToUpper())
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

                switch (preferences.DefaultDownloadFormat.ToUpper())
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

            return false;
        }

        public UserPreferences GetUserPreferences(string id) 
        {
            UserPreferences data = _ctx.UserPreferences
                                        .Where(u => id.Equals(u.User_Id))
                                        .FirstOrDefault();

            return data;
        }

        public void RemoveOldParameterDefaults(string id)
        {
            
        }

        public UserPreferences UpdateUserPreferences(UserPreferences preferences)
        {
            UserPreferences result = GetUserPreferences(preferences.User_Id);

            if (result != null)
            {
                // updates existing record
                result.DefaultStationId = preferences.DefaultStationId;
                result.DefaultMapMode = preferences.DefaultMapMode;
                result.DefaultDownloadFormat = preferences.DefaultDownloadFormat;
                
                // saves updated record
                _ctx.Entry(result).State = System.Data.Entity.EntityState.Modified;
                _ctx.SaveChanges();
            }
            else
            {
                // adds a new record
                _ctx.UserPreferences.Add(preferences);
            }

            // returns the user's new preferences
            return GetUserPreferences(preferences.User_Id);
        }

        public void Dispose()
        {
            _ctx.Dispose();
            _userManager.Dispose();

        }
    }
}
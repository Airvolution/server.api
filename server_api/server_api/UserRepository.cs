using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace server_api
{
    public class UserRepository
    {
        private AirUDBCOE _context;

        public UserRepository(AirUDBCOE context)
        {
            _context = context;
        }

        public bool RegisterUser(string email, string password)
        {
            User existingUser = _context.Users.SingleOrDefault(x => x.Email == email);

            if (existingUser == null)
            {
                // Perform queries to insert new user into database.
                User newUser = new User();
                newUser.Email = email;
                newUser.Pass = password;
                newUser.Password = password;
                newUser.ConfirmPassword = password;

                _context.Users.Add(newUser);
                _context.SaveChanges();

                // Account register success.
                return true;
            }
            else
            {
                // Account register failed. Account with email address: '<user.Email>' already exists. Please try a different email address.
                return false;
            }
        }

        public Device GetUserDevices(string username)
        {
            return null;
        }
    }
}
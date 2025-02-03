using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Interfaces;
using BusinessLogicLayer.Utilities;

namespace BusinessLogicLayer.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public void RegisterUser(User user)
        {
            // Hash the password before saving
            user.Password = PasswordHelper.HashPassword(user.Password);

            // Save the user to the database
            _userRepository.RegisterUser(user);
        }

        public User LoginUser(string email, string password)
        {
            var user = _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                return null; // Email does not exist
            }

            // Verify the password
            bool isPasswordValid = PasswordHelper.VerifyPassword(password, user.Password);
            if (!isPasswordValid)
            {
                return null; // Password does not match
            }

            return user;
        }
    }
}
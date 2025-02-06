using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Utilities;
using System;

namespace BusinessLogicLayer.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly AuthService _authService;

        public UserService(IUserRepository userRepository, IEmailService emailService, AuthService authService)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _authService = authService;
        }

        public string RegisterUser(User user)
        {
            var existingUser = _userRepository.GetUserByEmail(user.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            user.Password = PasswordHelper.HashPassword(user.Password);
            _userRepository.RegisterUser(user);

            var token = _authService.GenerateJwtToken(user);

            var subject = "Your Registration Token";
            var body = $"Welcome! Use the following token to complete your registration: {token}";
            _emailService.SendEmail(user.Email, subject, body);

            return token;
        }

        public User LoginUser(string email, string password)
        {
            var user = _userRepository.GetUserByEmail(email);
            if (user == null || !PasswordHelper.VerifyPassword(password, user.Password))
            {
                throw new InvalidOperationException("Invalid email or password.");
            }

            return user;
        }

        public bool ResetPassword(string token, string currentPassword, string newPassword)
        {
            // Decode the JWT token and extract the user's email
            var claims = _authService.DecodeJwtToken(token);
            if (claims == null || !claims.ContainsKey("Email"))
            {
                throw new InvalidOperationException("Invalid or expired token.");
            }

            string email = claims["Email"].ToString();
            var user = _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            if (!PasswordHelper.VerifyPassword(currentPassword, user.Password))
            {
                throw new InvalidOperationException("Current password is incorrect.");
            }

            user.Password = PasswordHelper.HashPassword(newPassword);
            _userRepository.UpdateUser(user);

            return true;
        }

        public bool ForgotPassword(string email)
        {
            var user = _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            var token = _authService.GenerateJwtToken(user);

            var subject = "Password Reset Request";
            var body = $"To reset your password.=   {token}";
            _emailService.SendEmail(email, subject, body);

            return true;
        }

        public bool ResetPasswordConfirm(string token, string newPassword)
        {
            // Decode the JWT token and extract the user's email
            var claims = _authService.DecodeJwtToken(token);
            if (claims == null || !claims.ContainsKey("Email"))
            {
                throw new InvalidOperationException("Invalid or expired token.");
            }

            string email = claims["Email"].ToString();
            var user = _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            user.Password = PasswordHelper.HashPassword(newPassword);
            _userRepository.UpdateUser(user);

            return true;
        }

        public int GetUserIdFromToken(string token)
        {
            var claims = _authService.DecodeJwtToken(token);
            if (claims == null || !claims.ContainsKey("UserId"))
            {
                throw new InvalidOperationException("Invalid token.");
            }

            return int.Parse(claims["UserId"].ToString());
        }
    }
}

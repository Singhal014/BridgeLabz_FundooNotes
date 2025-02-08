using BusinessLogicLayer.Interfaces;
using RepoLayer.Interfaces;
using DataAccessLayer.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using RepoLayer.Entity;

namespace BusinessLogicLayer.Services
{
    public class UserBl : IUserBL
    {
        private readonly IUserRL _userRepository;
        private readonly IConfiguration _configuration;

        public UserBl(IUserRL userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
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

            var token = GenerateJwtToken(user);

            var subject = "Your Registration Token";
            var body = $"Welcome! Use the following token to complete your registration: {token}";
            SendEmail(user.Email, subject, body);

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
            var claims = DecodeJwtToken(token);
            if (claims == null || !claims.ContainsKey("Email"))
            {
                throw new InvalidOperationException("Invalid or expired token.");
            }

            string email = claims["Email"];
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

            var token = GenerateJwtToken(user);

            var subject = "Password Reset Request";
            var body = $"To reset your password, use this token: {token}";
            SendEmail(email, subject, body);

            return true;
        }

        public bool ResetPasswordConfirm(string token, string newPassword)
        {
            var claims = DecodeJwtToken(token);
            if (claims == null || !claims.ContainsKey("Email"))
            {
                throw new InvalidOperationException("Invalid or expired token.");
            }

            string email = claims["Email"];
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
            var claims = DecodeJwtToken(token);

            if (claims.TryGetValue("UserId", out string userIdString) && int.TryParse(userIdString, out int userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("Invalid token.");
        }

        public void SendEmail(string to, string subject, string body)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("singhalps014@gmail.com", "boiy wbwm ufkm lwnk"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("singhalps014@gmail.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(to);
            smtpClient.Send(mailMessage);
        }

        
        public string GenerateJwtToken(User user, int expiresInMinutes = 15)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");

            var claims = new List<Claim>
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim("Email", user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private Dictionary<string, string> DecodeJwtToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
        }
    }
}

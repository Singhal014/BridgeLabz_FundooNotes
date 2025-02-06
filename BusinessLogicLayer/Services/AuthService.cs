using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using DataAccessLayer.Models;
using System.Linq;

namespace BusinessLogicLayer.Services
{
    public class AuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateJwtToken(User user, int expiresInMinutes = 15)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");

            var claims = new List<Claim>
            {
                new Claim("UserId", user.Id.ToString()),  // Include UserId
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

        public Dictionary<string, string> DecodeJwtToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
        }

        public int GetUserIdFromToken(string token)
        {
            try
            {
                var claims = DecodeJwtToken(token);

                if (claims.ContainsKey("UserId") && int.TryParse(claims["UserId"], out int userId))
                {
                    return userId;
                }
                else
                {
                    throw new InvalidOperationException("UserId claim is missing or invalid.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error decoding token: " + ex.Message);
            }
        }

        public bool IsTokenExpired(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var expirationDate = jwtToken.ValidTo;

                return expirationDate < DateTime.UtcNow;
            }
            catch (Exception)
            {
                return true; // If there's any issue with decoding, assume token is invalid/expired
            }
        }
    }
}

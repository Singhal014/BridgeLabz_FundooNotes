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
using Microsoft.AspNetCore.Connections;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace BusinessLogicLayer.Services
{
    public class UserBl : IUserBL
    {
        private readonly IUserRL _userRepository;
        private readonly IConfiguration _configuration;

        private readonly ConnectionFactory _rabbitMQFactory;

        public UserBl(IUserRL userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;

            var rabbitMQConfig = _configuration.GetSection("RabbitMQ");
            _rabbitMQFactory = new ConnectionFactory()
            {
                HostName = rabbitMQConfig["Host"],
                UserName = rabbitMQConfig["Username"],
                Password = rabbitMQConfig["Password"]
            };

            StartRabbitMQConsumer();
        }


        public string RegisterUser(User user)
        {
            // Check if the user already exists
            var existingUser = _userRepository.GetUserByEmail(user.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            // Hash the password
            user.Password = PasswordHelper.HashPassword(user.Password);

            // Register the user in the database
            _userRepository.RegisterUser(user);

            // Generate a JWT token for the user
            var token = GenerateJwtToken(user);

            // Prepare the email message
            var emailMessage = new
            {
                Email = user.Email,
                Subject = "Registration Successful - FundooNotes",
                Body = $"Congratulations! You have successfully registered on FundooNotes.<br/> Your token: {token}"
            };

            // Publish the email message to RabbitMQ
            PublishToQueue("emailQueue", JsonConvert.SerializeObject(emailMessage));

            return token;
        }

        private void PublishToQueue(string queueName, string message)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = "localhost", 
                    UserName = "guest",     
                    Password = "guest"     
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                // Declare the queue
                channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                // Convert the message to bytes
                var body = Encoding.UTF8.GetBytes(message);

                // Publish the message to the queue
                channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);

                Console.WriteLine($" [x] Sent message to {queueName}: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing to RabbitMQ: {ex.Message}");
            }
        }

        private void StartRabbitMQConsumer()
        {
            Task.Run(() =>
            {
                try
                {
                    var connection = _rabbitMQFactory.CreateConnection();
                    var channel = connection.CreateModel();

                    channel.QueueDeclare(queue: "emailQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        try
                        {
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);
                            var emailMessage = JsonConvert.DeserializeObject<EmailMessage>(message);

                            Console.WriteLine($" [x] Received message: {message}");

                            // Send Email
                            SendEmail(emailMessage.Email, emailMessage.Subject, emailMessage.Body);

                            Console.WriteLine($" [x] Email sent to: {emailMessage.Email}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing message: {ex.Message}");
                        }
                    };

                    channel.BasicConsume(queue: "emailQueue", autoAck: true, consumer: consumer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in RabbitMQ consumer: {ex.Message}");
                }
            });
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

            // Prepare the email message
            var emailMessage = new
            {
                Email = email,
                Subject = "Password Reset Request - FundooNotes",
                Body = $"To reset your password, click the link: {token}"
            };

            // Publish the email message to RabbitMQ
            PublishToQueue("emailQueue", JsonConvert.SerializeObject(emailMessage));

            return true;
        }

        public User GetUserByEmail(string email)
        {
            return _userRepository.GetUserByEmail(email);
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
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("singhalps014@gmail.com", "acmc xprc ycvh rayz"),
                    EnableSsl = true,
                    Timeout = 60000 
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
                Console.WriteLine($"Email sent successfully to: {to}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
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
        private class EmailMessage
        {
            public string Email { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
        }

        private Dictionary<string, string> DecodeJwtToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
        }
    }
}

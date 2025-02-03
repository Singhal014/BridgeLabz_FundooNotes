using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Services;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly AuthService _authService;

        public UserController(IUserService userService, AuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        // POST: api/User/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (user == null || string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName) ||
                string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                return BadRequest(new UserResponseModel
                {
                    Message = "First name, last name, email, and password are required.",
                    Success = false,
                    Data = null
                });
            }

            // Register the user
            _userService.RegisterUser(user);

            // Return the response without the Id
            return Ok(new UserResponseModel
            {
                Message = "User registered successfully.",
                Success = true,
                Data = new
                {
                    user.FirstName,
                    user.LastName,
                    user.Email
                }
            });
        }

        // POST: api/User/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel loginModel)
        {
            if (loginModel == null || string.IsNullOrEmpty(loginModel.Email) || string.IsNullOrEmpty(loginModel.Password))
            {
                return BadRequest(new UserResponseModel
                {
                    Message = "Email and password are required.",
                    Success = false,
                    Data = null
                });
            }

            var authenticatedUser = _userService.LoginUser(loginModel.Email, loginModel.Password);
            if (authenticatedUser == null)
            {
                return Unauthorized(new UserResponseModel
                {
                    Message = "Invalid credentials.",
                    Success = false,
                    Data = null
                });
            }

            // Generate JWT token
            var token = _authService.GenerateJwtToken(authenticatedUser);

            // Return the response without the Id
            return Ok(new UserResponseModel
            {
                Message = "Login successful.",
                Success = true,
                Data = new
                {
                    authenticatedUser.FirstName,
                    authenticatedUser.LastName,
                    authenticatedUser.Email,
                    Token = token
                }
            });
        }
    }
}
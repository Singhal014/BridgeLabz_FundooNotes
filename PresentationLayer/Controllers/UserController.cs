using BusinessLogicLayer.Interfaces;
using ModelLayer.Models;
using RepoLayer.Entity;
using Microsoft.AspNetCore.Authorization; // Add this for [Authorize]
using Microsoft.AspNetCore.Mvc;

[Route("users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserBL _userService;

    public UserController(IUserBL userService)
    {
        _userService = userService;
    }

    // Register user
    [HttpPost]
    public IActionResult Register([FromBody] User user)
    {
        try
        {
            var token = _userService.RegisterUser(user);
            return Ok(new { Message = "User registered. Check your email for the token.", Success = true, Token = token });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message, Success = false });
        }
    }

    // Login user
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel loginModel)
    {
        try
        {
            var user = _userService.LoginUser(loginModel.Email, loginModel.Password);
            var token = _userService.GenerateJwtToken(user);
            return Ok(new { Message = "Login successful.", Token = token });
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { Message = ex.Message, Success = false });
        }
    }

    // Reset Password (Token from Authorization Header)
    [Authorize] // Ensure token is sent in the Authorization header
    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetPasswordModel model)
    {
        try
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();

            var result = _userService.ResetPassword(token, model.CurrentPassword, model.NewPassword);
            return Ok(new { Message = "Password reset successfully.", Success = true });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message, Success = false });
        }
    }

    // Forgot Password
    [HttpPost("forgot-password")]
    public IActionResult ForgotPassword([FromBody] ForgotPasswordModel model)
    {
        try
        {
            var result = _userService.ForgotPassword(model.Email);
            return Ok(new { Message = "Password reset link sent to your email.", Success = true });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message, Success = false });
        }
    }

    
    [Authorize] 
    [HttpPost("reset-password-confirm")]
    public IActionResult ResetPasswordConfirm([FromBody] NewPasswordModel model)
    {
        try
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();

            var result = _userService.ResetPasswordConfirm(token, model.NewPassword);
            return Ok(new { Message = "Password reset successfully.", Success = true });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message, Success = false });
        }
    }
}

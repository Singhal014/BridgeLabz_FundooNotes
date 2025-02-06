using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Services;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using System;

[Route("api/users")]
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
            var token = _authService.GenerateJwtToken(user);
            return Ok(new { Message = "Login successful.", Token = token });
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { Message = ex.Message, Success = false });
        }
    }

    // Reset Password (using JWT token)
    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetPasswordModel model, [FromHeader] string token)
    {
        try
        {
            // Use the token to reset password without requiring email
            var result = _userService.ResetPassword(token, model.CurrentPassword, model.NewPassword);
            return Ok(new { Message = "Password reset successfully.", Success = true });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message, Success = false });
        }
    }

    // Forgot Password (sends a token to the email)
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

    // Confirm Reset Password (using JWT token)
    [HttpPost("reset-password-confirm")]
    public IActionResult ResetPasswordConfirm([FromQuery] string token, [FromBody] NewPasswordModel model)
    {
        try
        {
            // Use the token to reset the password without requiring email
            var result = _userService.ResetPasswordConfirm(token, model.NewPassword);
            return Ok(new { Message = "Password reset successfully.", Success = true });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message, Success = false });
        }
    }
}

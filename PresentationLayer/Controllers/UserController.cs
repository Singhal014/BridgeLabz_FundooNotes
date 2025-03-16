using BusinessLogicLayer.Interfaces;
using ModelLayer.Models;
using RepoLayer.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[Route("users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserBL _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserBL userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost]
    public IActionResult Register([FromBody] UserRegistrationModel userModel)
    {
        try
        {
            _logger.LogInformation("User registration request received for email: {Email}", userModel.Email);

            var token = _userService.RegisterUser(userModel);

            _logger.LogInformation("User registered successfully: {Email}", userModel.Email);
            return Ok(new { Message = "User registered. Check your email for the token.", Success = true, Token = token });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "User registration failed for email: {Email}", userModel.Email);
            return Conflict(new { Message = ex.Message, Success = false });
        }
    }


    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel loginModel)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", loginModel.Email);
            var (accessToken, refreshToken) = _userService.LoginUser(loginModel.Email, loginModel.Password);
            _logger.LogInformation("User logged in successfully: {Email}", loginModel.Email);
            return Ok(new { Message = "Login successful.", AccessToken = accessToken, RefreshToken = refreshToken });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Login failed for email: {Email}", loginModel.Email);
            return Unauthorized(new { Message = ex.Message, Success = false });
        }
    }

    [Authorize]
    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetPasswordModel model)
    {
        try
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();
            _logger.LogInformation("Password reset request received.");
            _userService.ResetPassword(token, model.CurrentPassword, model.NewPassword);
            _logger.LogInformation("Password reset successful.");
            return Ok(new { Message = "Password reset successfully.", Success = true });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Password reset failed.");
            return BadRequest(new { Message = ex.Message, Success = false });
        }
    }

    [HttpPost("forgot-password")]
    public IActionResult ForgotPassword([FromBody] ForgotPasswordModel model)
    {
        try
        {
            _logger.LogInformation("Forgot password request received for email: {Email}", model.Email);
            _userService.ForgotPassword(model.Email);
            _logger.LogInformation("Password reset link sent to email: {Email}", model.Email);
            return Ok(new { Message = "Password reset link sent to your email.", Success = true });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Forgot password failed for email: {Email}", model.Email);
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
            _logger.LogInformation("Password reset confirmation request received.");
            _userService.ResetPasswordConfirm(token, model.NewPassword);
            _logger.LogInformation("Password reset confirmed successfully.");
            return Ok(new { Message = "Password reset successfully.", Success = true });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Password reset confirmation failed.");
            return BadRequest(new { Message = ex.Message, Success = false });
        }
    }
    [HttpPost("refresh-token")]
    public IActionResult RefreshToken([FromBody] RefreshTokenModel request)
    {
        try
        {
            var newAccessToken = _userService.RefreshAccessToken(request.RefreshToken);
            return Ok(new { AccessToken = newAccessToken });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
    }

}

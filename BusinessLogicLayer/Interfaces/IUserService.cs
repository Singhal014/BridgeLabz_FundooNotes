using DataAccessLayer.Models;

namespace BusinessLogicLayer.Interfaces
{
    public interface IUserService
    {
        string RegisterUser(User user); // Now returns the token after registration
        User LoginUser(string email, string password);
        bool ResetPassword(string token, string currentPassword, string newPassword); // Reset Password using token
        bool ForgotPassword(string email); // Forgot Password generates and sends token to email
        bool ResetPasswordConfirm(string token, string newPassword); // Reset Password confirm using token

        int GetUserIdFromToken(string token); // Extract user ID from token
    }
}

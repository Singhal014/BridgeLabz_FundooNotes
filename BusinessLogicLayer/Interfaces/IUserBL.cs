using ModelLayer.Models;
using RepoLayer.Entity;

namespace BusinessLogicLayer.Interfaces
{
    public interface IUserBL
    {
        string RegisterUser(User user);
        User LoginUser(string email, string password);
        bool ResetPassword(string token, string currentPassword, string newPassword);
        bool ForgotPassword(string email);
        bool ResetPasswordConfirm(string token, string newPassword);
        int GetUserIdFromToken(string token);
        void SendEmail(string to, string subject, string body);

        // Added GenerateJwtToken to the interface
        string GenerateJwtToken(User user, int expiresInMinutes = 15);
    }
}

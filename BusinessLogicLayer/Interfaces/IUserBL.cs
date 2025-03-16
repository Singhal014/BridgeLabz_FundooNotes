using ModelLayer.Models;
using RepoLayer.Entity;

namespace BusinessLogicLayer.Interfaces
{
    public interface IUserBL
    {
        string RegisterUser(UserRegistrationModel userModel);

        (string AccessToken, string RefreshToken) LoginUser(string email, string password);

        bool ResetPassword(string token, string currentPassword, string newPassword);
        bool ForgotPassword(string email);
        bool ResetPasswordConfirm(string token, string newPassword);
        int GetUserIdFromToken(string token);
        void SendEmail(string to, string subject, string body);
        User GetUserByEmail(string email);

        string GenerateJwtToken(User user, int expiresInMinutes = 60);

        string GenerateRefreshToken();
        string RefreshAccessToken(string refreshToken);
    }
}

using ModelLayer.Models;
using RepoLayer.Entity;

namespace BusinessLogicLayer.Interfaces
{
    public interface IUserBL
    {
        string RegisterUser(User user);

        // Modify LoginUser to return both tokens
        (string AccessToken, string RefreshToken) LoginUser(string email, string password);

        bool ResetPassword(string token, string currentPassword, string newPassword);
        bool ForgotPassword(string email);
        bool ResetPasswordConfirm(string token, string newPassword);
        int GetUserIdFromToken(string token);
        void SendEmail(string to, string subject, string body);
        User GetUserByEmail(string email);

        // Updated to return only access token
        string GenerateJwtToken(User user, int expiresInMinutes = 15);

        // New methods for refresh token handling
        string GenerateRefreshToken();
        string RefreshAccessToken(string refreshToken);
    }
}

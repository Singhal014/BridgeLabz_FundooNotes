using DataAccessLayer.Models;

namespace DataAccessLayer.Interfaces
{
    public interface IUserRepository
    {
        void RegisterUser(User user);
        User GetUserByEmail(string email);
    }
}
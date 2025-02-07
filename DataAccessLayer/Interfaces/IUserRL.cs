using DataAccessLayer.Models;

namespace DataAccessLayer.Interfaces
{
    public interface IUserRL
    {
        void RegisterUser(User user);
        User GetUserByEmail(string email);
        void UpdateUser(User user);
    }
}

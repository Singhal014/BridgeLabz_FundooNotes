using DataAccessLayer.Models;

namespace BusinessLogicLayer.Interfaces
{
    public interface IUserService
    {
        void RegisterUser(User user);
        User LoginUser(string email, string password);
    }
}

using ModelLayer.Models;
using RepoLayer.Interfaces;
using RepoLayer.Entity;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Context;

namespace DataAccessLayer.Repositories
{
    public class UserRL : IUserRL
    {
        private readonly ApplicationDbContext _context;

        public UserRL(ApplicationDbContext context)
        {
            _context = context;
        }

        public void RegisterUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public User GetUserByEmail(string email)
        {
            return _context.Users.SingleOrDefault(u => u.Email == email);
        }

        public void UpdateUser(User user)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser != null)
            {
                // Update only the necessary fields
                existingUser.IsVerified = user.IsVerified;
                //existingUser.VerificationToken = user.VerificationToken;
                existingUser.Password = user.Password;


                _context.Entry(existingUser).State = EntityState.Modified;
                _context.SaveChanges();
            }
        }
    }
}
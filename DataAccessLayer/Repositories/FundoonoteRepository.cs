using DataAccessLayer.Models;
using DataAccessLayer.Context;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class FundoonoteRepository : IFundoonoteRepository
    {
        private readonly ApplicationDbContext _context;

        public FundoonoteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddFundoonote(Fundoonote fundoonote)
        {
            _context.Fundoonotes.Add(fundoonote);
            _context.SaveChanges();
        }

        public Fundoonote GetFundoonoteById(int fundoonoteId)
        {
            return _context.Fundoonotes.FirstOrDefault(f => f.Id == fundoonoteId);
        }

        public IEnumerable<Fundoonote> GetAllFundoonotes()
        {
            return _context.Fundoonotes.ToList();
        }

        public void UpdateFundoonote(Fundoonote fundoonote)
        {
            _context.Fundoonotes.Update(fundoonote);
            _context.SaveChanges();
        }

        public void DeleteFundoonote(int fundoonoteId)
        {
            var fundoonote = _context.Fundoonotes.FirstOrDefault(f => f.Id == fundoonoteId);
            if (fundoonote != null)
            {
                _context.Fundoonotes.Remove(fundoonote);
                _context.SaveChanges();
            }
        }
    }
}

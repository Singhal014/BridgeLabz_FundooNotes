using DataAccessLayer.Models;

namespace DataAccessLayer.Interfaces
{
    public interface IFundoonoteRepository
    {
        void AddFundoonote(Fundoonote fundoonote);
        Fundoonote GetFundoonoteById(int fundoonoteId);
        IEnumerable<Fundoonote> GetAllFundoonotes();
        void UpdateFundoonote(Fundoonote fundoonote);
        void DeleteFundoonote(int fundoonoteId);
    }
}

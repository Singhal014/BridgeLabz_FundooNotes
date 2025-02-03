using DataAccessLayer.Models;

namespace BusinessLogicLayer.Interfaces
{
    public interface IFundoonoteService
    {
        void AddFundoonote(Fundoonote fundoonote);
        Fundoonote GetFundoonoteById(int fundoonoteId);
        IEnumerable<Fundoonote> GetAllFundoonotes();
        void EditFundoonote(Fundoonote fundoonote);
        void TrashFundoonote(int fundoonoteId);
    }
}

using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Services
{
    public class FundoonoteService : IFundoonoteService
    {
        private readonly IFundoonoteRepository _fundoonoteRepository;

        public FundoonoteService(IFundoonoteRepository fundoonoteRepository)
        {
            _fundoonoteRepository = fundoonoteRepository;
        }

        public void AddFundoonote(Fundoonote fundoonote)
        {
            _fundoonoteRepository.AddFundoonote(fundoonote);
        }

        public Fundoonote GetFundoonoteById(int fundoonoteId)
        {
            return _fundoonoteRepository.GetFundoonoteById(fundoonoteId);
        }

        public IEnumerable<Fundoonote> GetAllFundoonotes()
        {
            return _fundoonoteRepository.GetAllFundoonotes();
        }

        public void EditFundoonote(Fundoonote fundoonote)
        {
            _fundoonoteRepository.UpdateFundoonote(fundoonote);
        }

        public void TrashFundoonote(int fundoonoteId)
        {
            _fundoonoteRepository.DeleteFundoonote(fundoonoteId);
        }
    }
}

using deha_api_exam.Models;
using deha_api_exam.ViewModels;

namespace deha_api_exam.Services
{
    public interface IVoteService
    {
        Task<IEnumerable<VoteViewModel>> GetAll();
        Task<Result> Delete(string UserID,int PostID);
        Task<Result> Create(VoteViewModel postrequest);
    }
}

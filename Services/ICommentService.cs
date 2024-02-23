using deha_api_exam.Models;
using deha_api_exam.ViewModels;

namespace deha_api_exam.Services
{
    public interface ICommentService
    {
        Task<CommentViewModel> GetById(int? id);
        Task<IEnumerable<CommentViewModel>> GetAllByPost(int? postid);
        Task<IEnumerable<CommentViewModel>> GetAllByPostPaging(int? postid,int? page);
        Task<IEnumerable<CommentViewModel>> GetAll();
        Task<Result> Delete(int id);
        Task<Result> Create(CommentRequest commentRequest,string UserID);
        Task<Result> Update(CommentViewModel commentViewModel);
    }
}

using deha_api_exam.Models;
using deha_api_exam.ViewModels;

namespace deha_api_exam.Services
{
    public interface IAttachmentService
    {
        Task<AttachmentViewModel> GetById(int? id);
        Task<IEnumerable<AttachmentViewModel>> GetAllByPost(int? postid);
        Task<Result> Delete(int id);
        Task<Result> Create(AttachmentViewModel fileinpost);
        Task<Result> Update(AttachmentViewModel fileinpost);
    }
}

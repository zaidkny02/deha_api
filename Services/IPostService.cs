using deha_api_exam.Models;
using deha_api_exam.ViewModels;
using Microsoft.AspNetCore.JsonPatch;

namespace deha_api_exam.Services
{
    public interface IPostService
    {
        Task<PostViewModel> GetById(int? id);
        Task<IEnumerable<PostViewModel>> GetAllByUser(string? userid);
        Task<IEnumerable<PostViewModel>> GetAll();
        Task<Result> Delete(int id);
        Task<Result> Create(PostRequest postrequest,string userID);
        Task<Result> Update(PostViewModel postviewmodel);
        Task<Result> PatchViewCount(int id, JsonPatchDocument<ViewCountPatch> patchDocument);
        Task<Result> PatchVote(int id, JsonPatchDocument<VoteViewModel> patchDocument);

        Task<Result> Unvote(int id, JsonPatchDocument<VoteViewModel> patchDocument);
        Task<IEnumerable<PostViewModel>> GetAllPagingAndFilter(string? keyword, int? page);
        bool PostExists(int id);

        Task<PostwithComment> GetPostwithComment(int PostID,int? page);
    }
}

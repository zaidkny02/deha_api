using deha_api_exam.Models;
using deha_api_exam.ViewModels;

namespace deha_api_exam.Services
{
    public interface IUserService
    {
        Task<string> Authenticate(LoginViewModel request);

        Task<Result> Register(RegisterViewModel request);
        Task<bool> UserExists(string userID);
        Task<UserViewModel> GetById(string userID);
        Task<Result> Delete(string userID);
        Task<Result> Update(UserViewModel userViewModel);
        Task<IEnumerable<UserViewModel>> GetAll(string? keyword, int? page);
    }
}

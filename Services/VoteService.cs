using AutoMapper;
using deha_api_exam.Models;
using deha_api_exam.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace deha_api_exam.Services
{
    public class VoteService : IVoteService
    {
        private readonly MyDBContext _context;
        private readonly IMapper _mapper;
        public VoteService(MyDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result> Create(VoteViewModel postrequest)
        {
            Result result = new Result();
            if (!VoteExists(postrequest.UserID, postrequest.PostID))
            {
                try
                {
                    var vote = _mapper.Map<Vote>(postrequest);
                    _context.Add(vote);
                    await _context.SaveChangesAsync();
                    result.type = "Success";
                    result.message = "Vote Success";
                }
                catch(Exception ex)
                {
                    result.type = "Failure";
                    result.message = ex.ToString();
                }
            }
            else
            {
                result.type = "Failure";
                result.message = "This user already vote this post";
            }
            return result;
        }

        public async Task<Result> Delete(string UserID, int PostID)
        {
            Result result = new Result();
            if (VoteExists(UserID, PostID))
            {
                try
                {
                    var myvote = await _context.Vote.AsNoTracking().Where(x => x.UserID.Equals(UserID) && x.PostID == PostID).FirstOrDefaultAsync();
                    _context.Vote.Remove(myvote);
                    await _context.SaveChangesAsync();
                    result.type = "Success";
                    result.message = "Unvote Success";
                }
                catch (Exception ex)
                {
                    result.type = "Failure";
                    result.message = ex.ToString();
                }
            }
            else
            {
                result.type = "Failure";
                result.message = "This user wasn't vote this post";
            }
            return result;
        }

        public async Task<IEnumerable<VoteViewModel>> GetAll()
        {
            var vote = await _context.Vote.ToListAsync();
            return _mapper.Map<IEnumerable<VoteViewModel>>(vote);
        }
        private bool VoteExists(string UserID,int PostID)
        {
            return (_context.Vote?.Any(e => e.UserID.Equals(UserID) && e.PostID == PostID)).GetValueOrDefault();
        }
    }
}

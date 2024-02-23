using AutoMapper;
using deha_api_exam.Models;
using deha_api_exam.ViewModels;
using deha_api_exam.ViewModels.ViewModelsValidators;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace deha_api_exam.Services
{
    public class CommentService : ICommentService
    {
        private readonly MyDBContext _context;
        private readonly IValidator<CommentRequest> _commentrequestvalidator;
        private readonly IValidator<CommentViewModel> _commentviewmodelvalidator;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        public CommentService(MyDBContext context, IValidator<CommentRequest> commentrequestvalidator, IMapper mapper, IUserService userService,  IValidator<CommentViewModel> commentviewmodelvalidator)
        {
            _context = context;
            _commentrequestvalidator = commentrequestvalidator;
            _mapper = mapper;
            _userService = userService;
            _commentviewmodelvalidator = commentviewmodelvalidator;
        }

        public async Task<Result> Create(CommentRequest commentRequest, string UserID)
        {
            Result result = new Result();
            if ((_context.Post?.Any(e => e.Id == commentRequest.PostID)).GetValueOrDefault() && await _userService.UserExists(UserID))
            {
                ValidationResult validationResult = _commentrequestvalidator.Validate(commentRequest);
                if (validationResult.IsValid)
                {
                    try
                    {
                        var newcomment = _mapper.Map<Comment>(commentRequest);
                        newcomment.DateCreated = DateTime.Now;
                        newcomment.UserID = UserID;
                        newcomment.CommentUserName = "";
                        _context.Add(newcomment);
                        await _context.SaveChangesAsync();
                        result.type = "Success";
                        result.message = newcomment.Id.ToString();
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
                    result.message = "Model isn't valid";
                }
            }
            else
            {
                result.type = "NotFound";
                result.message = "Post not found or user not found";
            }
            return result;
        }

        public async  Task<Result> Delete(int id)
        {
            Result result = new Result();
            var comment = await _context.Comment.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(c => new Comment
                {
                    Id = c.Id,
                    Content = c.Content,
                    DateCreated = c.DateCreated,
                    PostID = c.PostID,
                    UserID = c.UserID,
                    CommentUserName = c.User.UserName // Lấy UserName của User thông qua Post
                })
                .FirstOrDefaultAsync();
            try
            {
                if (comment != null)
                {
                    _context.Comment.Remove(comment);
                    await _context.SaveChangesAsync();
                    result.type = "Success";
                    result.message = "Success";
                    return result;
                }
                result.type = "NotFound";
                result.message = "NotFound";
                return result;
            }
            catch (Exception ex)
            {
                //   throw new Exception("Can't remove course because : " + ex);
                result.type = "Failure";
                result.message = ex.ToString();
                return result;
            }
        }

        public async Task<IEnumerable<CommentViewModel>> GetAll()
        {
            var comment = await _context.Comment.ToListAsync();
            return _mapper.Map<IEnumerable<CommentViewModel>>(comment);
        }

        public async Task<IEnumerable<CommentViewModel>> GetAllByPost(int? postid)
        {
            var comment = await _context.Comment
                .Where(x => x.PostID.Equals(postid))
                .Select(c => new Comment
                {
                    Id = c.Id,
                    Content = c.Content,
                    DateCreated = c.DateCreated,
                    PostID = c.PostID,
                    UserID = c.UserID,
                    CommentUserName = c.User.UserName // Lấy UserName của User thông qua Post
                }).
               ToListAsync();
            return _mapper.Map<IEnumerable<CommentViewModel>>(comment);
        }

        public async Task<IEnumerable<CommentViewModel>> GetAllByPostPaging(int? postid,int? page)
        {
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            int pageSize = 1;
            var comment = await _context.Comment.Where(x => x.PostID.Equals(postid))
                .Select(c => new Comment
                {
                    Id = c.Id,
                    Content = c.Content,
                    DateCreated = c.DateCreated,
                    PostID = c.PostID,
                    UserID = c.UserID,
                    CommentUserName = c.User.UserName // Lấy UserName của User thông qua Post
                }).
               ToListAsync();
            var returnlistcomment = comment.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return _mapper.Map<IEnumerable<CommentViewModel>>(returnlistcomment);
        }

        public async Task<CommentViewModel> GetById(int? id)
        {
            var comment = await _context.Comment.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(c => new Comment
                {
                    Id = c.Id,
                    Content = c.Content,
                    DateCreated = c.DateCreated,
                    PostID = c.PostID,
                    UserID = c.UserID,
                    CommentUserName = c.User.UserName // Lấy UserName của User thông qua Post
                })
                .FirstOrDefaultAsync();
            return _mapper.Map<CommentViewModel>(comment);
        }

        public async  Task<Result> Update(CommentViewModel commentViewModel)
        {
            Result result = new Result();
            if (!CommentExists(commentViewModel.Id))
            {
                result.type = "NotFound";
                result.message = "NotFound";
                return result;
            }
            var mycomment = await _context.Comment.AsNoTracking().FirstOrDefaultAsync(m => m.Id == commentViewModel.Id);
            commentViewModel.DateCreated = mycomment.DateCreated;
            commentViewModel.UserID = mycomment.UserID;
            commentViewModel.PostID = mycomment.PostID;
            ValidationResult validationResult = _commentviewmodelvalidator.Validate(commentViewModel);
            if (validationResult.IsValid)
            {
                try
                {
                    _context.Update(_mapper.Map<Comment>(commentViewModel));
                    await _context.SaveChangesAsync();
                    result.type = "Success";
                    result.message = "Success";
                    return result;
                }
                catch (Exception ex)
                {
                    result.type = "Failure";
                    result.message = ex.ToString();
                    return result;
                }
            }
            else
            {
                result.type = "Failure";
                /* foreach(var err in validationResult.Errors)
                 {
                     result.message = result.message + err.ErrorMessage + "\n";
                 } */
                result.message = "Model isn't valid";
                return result;
            }
        }

        private bool CommentExists(int id)
        {
            return (_context.Comment?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

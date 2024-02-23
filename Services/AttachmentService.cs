using AutoMapper;
using deha_api_exam.Models;
using deha_api_exam.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

namespace deha_api_exam.Services
{
    public class AttachmentService : IAttachmentService
    {
        private readonly MyDBContext _context;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;
        private readonly IPostService _postService;
        private const string USER_CONTENT_FOLDER_NAME = "Uploads/user-content";
        public AttachmentService(MyDBContext context, IMapper mapper, IStorageService storageService, IPostService postService)
        {
            _context = context;
            _mapper = mapper;
            _storageService = storageService;
            _postService = postService;
        }

        private async Task<string> SaveFile(IFormFile file)
        {
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName!.Trim('"');
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
            // return "/" + USER_CONTENT_FOLDER_NAME + "/" + fileName;
            return "/"+USER_CONTENT_FOLDER_NAME + "/" + fileName;
        }

        public async Task<Result> Create(AttachmentViewModel fileinpost)
        {
            Result result = new Result();
            var post = _postService.GetById(fileinpost.PostID);
            if (post != null)
            {
                var file = _mapper.Map<Attachment>(fileinpost);
                if (fileinpost.file != null)
                {
                    file.fileUrl = await SaveFile(fileinpost.file);
                }
                try
                {
                    _context.Add(file);
                    await _context.SaveChangesAsync();
                    result.type = "Success";
                    result.message = file.Id.ToString();
                }
                catch (Exception ex)
                {
                    result.type = "Failure";
                    result.message = ex.ToString();
                }
            }
            else
            {
                result.type = "NotFound";
                result.message = "Can't create attachment because post not found";
            }
            return result;
        }

        public async Task<Result> Delete(int id)
        {
            Result result = new Result();
            var file = await _context.Attachment.FindAsync(id);
            try
            {
                if (file != null)
                {
                    if (!string.IsNullOrEmpty(file.fileUrl))
                          await _storageService.DeleteFileAsync(file.fileUrl.Replace("/" + USER_CONTENT_FOLDER_NAME + "/", ""));
                     //   await _storageService.DeleteFileAsync(file.fileUrl);
                    _context.Attachment.Remove(file);
                    await _context.SaveChangesAsync();
                    result.type = "Success";
                    result.message = "Delete File Success";
                    return result;
                }
                result.type = "NotFound";
                result.message = "NotFound";
                return result;
            }
            catch (Exception ex)
            {
                result.type = "Failure";
                result.message = ex.ToString();
                return result;
            }
        }

        public async Task<IEnumerable<AttachmentViewModel>> GetAllByPost(int? postid)
        {
            var listfile = await _context.Attachment.Where(x => x.PostID == postid).
                ToListAsync();
            return _mapper.Map<IEnumerable<AttachmentViewModel>>(listfile);
        }

        public async Task<AttachmentViewModel> GetById(int? id)
        {
            var file = await _context.Attachment.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(c => new Attachment
                {
                    Id = c.Id,
                    Title = c.Title,
                    fileUrl = c.fileUrl,
                    PostID = c.PostID,
                    fileSize = c.fileSize,
                    PostUserID = c.Post.UserID // Lấy UserName của User thông qua Post
                })
                .FirstOrDefaultAsync();

            return _mapper.Map<AttachmentViewModel>(file);
        }

        public async Task<Result> Update(AttachmentViewModel fileinpost)
        {
            Result result = new Result();
            if (!FileExists(fileinpost.Id))
            {
                // throw new Exception("Lesson does not exist");
                result.type = "NotFound";
                result.message = "NotFound";
                return result;
            }
            try
            {
                if (fileinpost.file != null)
                {
                    var myfile = await GetById(fileinpost.Id);
                    if (!string.IsNullOrEmpty(myfile.fileUrl))
                        await _storageService.DeleteFileAsync(myfile.fileUrl.Replace("/" + USER_CONTENT_FOLDER_NAME + "/", ""));
                    fileinpost.fileUrl = await SaveFile(fileinpost.file);
                    //change name 
                    fileinpost.Title = fileinpost.file.FileName;
                    //change size
                    fileinpost.fileSize = fileinpost.file.Length.ToString();
                    //add postid
                    fileinpost.PostID = myfile.PostID;
                    fileinpost.PostUserID = "";
                }

                _context.Update(_mapper.Map<Attachment>(fileinpost));
                await _context.SaveChangesAsync();
                result.type = "Success";
                result.message = "Update File Success";
                return result;
            }
            catch (Exception ex)
            {
                result.type = "Failure";
                result.message = ex.ToString();
                return result;
            }
        }

        private bool FileExists(int id)
        {
            return (_context.Attachment?.Any(e => e.Id == id)).GetValueOrDefault();
        }

    }
}

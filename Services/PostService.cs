using AutoMapper;
using deha_api_exam.Models;
using deha_api_exam.ViewModels;
using deha_api_exam.ViewModels.ViewModelsValidators;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Text.Json;

namespace deha_api_exam.Services
{
    public class PostService : IPostService
    {
        private readonly IValidator<PostRequest> _postrequestvalidator;
        private readonly IValidator<PostViewModel> _postviewmodelvalidator;
        private readonly MyDBContext _context;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IAttachmentService _attachmentService;
        private readonly IVoteService _voteService;
        private readonly ICommentService _commentService;
        private readonly IDistributedCache _distributedCache;
        private string cacheKey = "list_post";
        public PostService(MyDBContext context, IUserService userService, IMapper mapper, IAttachmentService attachmentService, IValidator<PostRequest> postrequestvalidator, IValidator<PostViewModel> postviewmodelvalidator, IVoteService voteService, ICommentService commentService, IDistributedCache distributed)
        {
            _context = context;
            _userService = userService;
            _mapper = mapper;
            _attachmentService = attachmentService;
            _postrequestvalidator = postrequestvalidator;
            _postviewmodelvalidator = postviewmodelvalidator;
            _voteService = voteService;
            _commentService = commentService;
            _distributedCache = distributed;
        }

        public async Task<PostwithComment> GetPostwithComment(int PostID, int? page)
        {
            var post = await GetById(PostID);
                
            if(post == null)
            {
                return null;    
            }
            else
            {
                var mypostwithcomment = _mapper.Map<PostwithComment>(post);
                var listcomment = await _commentService.GetAllByPostPaging(post.Id,page);
                var listcommentview = _mapper.Map<IEnumerable<CommentView>>(listcomment); 
                mypostwithcomment.ListComments = listcommentview.ToList();
                return mypostwithcomment;
            }
        }

        public async Task<Result> Create(PostRequest postrequest, string userID)
        {
            Result result = new Result();
            if (!await _userService.UserExists(userID))
            {
                result.type = "NotFound";
                result.message = "User not found so can't create post";
            }
            else
            {
                ValidationResult validationResult = _postrequestvalidator.Validate(postrequest);
                if (validationResult.IsValid)
                {
                    try
                    {

                        var post = _mapper.Map<Post>(postrequest);
                        post.UserID = userID;
                        // lấy datetime
                        post.DateCreated = DateTime.Now;
                        _context.Add(post);
                        await _context.SaveChangesAsync();
                        post.Attachments = null;
                        int postid = post.Id;
                        if (postrequest.lsfile != null)
                        {
                            foreach (var item in postrequest.lsfile)
                            {
                                var filemodel = new AttachmentViewModel
                                {
                                    Title = item.FileName,
                                    file = item,
                                    fileSize = item.Length.ToString(),
                                    PostID = postid
                                };
                                result = await _attachmentService.Create(filemodel);
                            }
                        }
                        result.type = "Success";
                        result.message = postid.ToString();
                        // Xử lý cache
                        var cachedData = await _distributedCache.GetStringAsync(cacheKey);
                        if (cachedData != null)
                        {
                            // Cập nhật lại item bằng data mới và lưu vào cache
                            // Chuyển đổi chuỗi JSON
                            List<PostViewModel> cachedDataList;
                            cachedDataList = JsonSerializer.Deserialize<List<PostViewModel>>(cachedData);
                            int currentdbcount = _context.Post.Count();
                            int cachesize = cachedDataList.Count();
                            var newitemToInsert = _mapper.Map<PostViewModel>(post);
                            newitemToInsert.User = null;
                            cachedDataList.Insert(0, newitemToInsert);
                            if (currentdbcount > cachesize)
                            {
                                cachedDataList.RemoveAt(cachesize);
                            }
                            // Chuyển đổi cachedDataList thành chuỗi JSON
                            IEnumerable<PostViewModel> myupdatetocache = cachedDataList;
                            var updatedDataJson = JsonSerializer.Serialize(myupdatetocache);
                            var updatedEncodedData = Encoding.UTF8.GetBytes(updatedDataJson);
                            // Lưu dữ liệu đã cập nhật vào cache
                            var cacheOptions = new DistributedCacheEntryOptions
                            {
                                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                            };
                            await _distributedCache.SetAsync(cacheKey, updatedEncodedData, cacheOptions);
                        }
                        //send email
                        /*   var liststudent = await _classDetailService.GetAllByClass(lesson.ClassID);
                           if (liststudent.Count() > 0)
                           {
                               foreach (var item in liststudent)
                                   await _emailService.SendEmailAsync(item.User.Email, "Teacher just add lesson " + lesson.Title + " in " + myclass.Title + " class", "lesson created at " + lesson.DateCreated);
                           }  */
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
            return result;
        }

        public async Task<Result> Delete(int id)
        {
            Result result = new Result();
            var post = await _context.Post.FindAsync(id);
            try
            {
                if (post != null)
                {
                    //delete file
                    var listfile = await _attachmentService.GetAllByPost(post.Id);
                    foreach (var item in listfile)
                    {
                        await _attachmentService.Delete(item.Id);
                    }
                    _context.Post.Remove(post);
                    await _context.SaveChangesAsync();
                    result.type = "Success";
                    result.message = "Success";
                    // xử lý delete trong cache
                    var cachedData = await _distributedCache.GetStringAsync(cacheKey);
                    if (cachedData != null)
                    {
                        // Cập nhật lại item bằng data mới và lưu vào cache
                        // Chuyển đổi chuỗi JSON
                        IEnumerable<PostViewModel> cachedDataList;
                        cachedDataList = JsonSerializer.Deserialize<IEnumerable<PostViewModel>>(cachedData);
                        var itemToDelete = cachedDataList.FirstOrDefault(x => x.Id == post.Id);
                        if (itemToDelete != null)
                        {
                            await _distributedCache.RemoveAsync(cacheKey);
                        }
                    }
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

        public async Task<IEnumerable<PostViewModel>> GetAllPagingAndFilter(string? keyword, int? page)
        {
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            int pageSize = 10;
            int pageCachedNumber = 5;

            if (keyword != null)
            {
               // keyword = "";
                var post = await _context.Post.Where(x => x.Title.ToLower().Contains(keyword.ToLower()))
                    .Select(c => new Post
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Content = c.Content,
                        DateCreated = c.DateCreated,
                        ViewCount = c.ViewCount,
                        Votes = c.Votes,
                        UserID = c.UserID,
                        PostUserName = c.User.UserName // Lấy UserName của User thông qua Post
                    })
                    .OrderByDescending(x => x.DateCreated).ToListAsync();
                var returnlistwithkeyword = post.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                //var object.listcomment = returnlist
                //return object
                return _mapper.Map<IEnumerable<PostViewModel>>(returnlistwithkeyword);
            }
            else
            {
                IEnumerable<PostViewModel> cachedDataList;
                var cachedData = await _distributedCache.GetStringAsync(cacheKey);
                if (cachedData == null)
                {
                    var post = await _context.Post
                        .Select(c => new Post
                        {
                            Id = c.Id,
                            Title = c.Title,
                            Content = c.Content,
                            DateCreated = c.DateCreated,
                            ViewCount = c.ViewCount,
                            Votes = c.Votes,
                            UserID = c.UserID,
                            PostUserName = c.User.UserName // Lấy UserName của User thông qua Post
                        })
                        .OrderByDescending(x => x.DateCreated).Take(pageCachedNumber* pageSize).ToListAsync();
                    var listpost = _mapper.Map<IEnumerable<PostViewModel>>(post);
                    var newDataJson = JsonSerializer.Serialize(listpost);
                    var encodedData = Encoding.UTF8.GetBytes(newDataJson);
                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                    };
                    await _distributedCache.SetAsync(cacheKey, encodedData, cacheOptions);
                    cachedData = newDataJson;
                }
                if (pageNumber < pageCachedNumber + 1)
                {
                    // Chuyển đổi chuỗi JSON
                    cachedDataList = JsonSerializer.Deserialize<IEnumerable<PostViewModel>>(cachedData);
                    var cachedDataListReturn = cachedDataList.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                    return cachedDataListReturn;
                }
                else
                {
                    var listpost_fornotcached = await GetAll();
                    return listpost_fornotcached.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                }
            }
        }

        public async Task<IEnumerable<PostViewModel>> GetAll()
        {
            var post = await _context.Post.
                Select(c => new Post
                {
                    Id = c.Id,
                    Title = c.Title,
                    Content = c.Content,
                    DateCreated = c.DateCreated,
                    ViewCount = c.ViewCount,
                    Votes = c.Votes,
                    UserID = c.UserID,
                    PostUserName = c.User.UserName // Lấy UserName của User thông qua Post
                }).OrderByDescending(x => x.DateCreated).ToListAsync();
            return _mapper.Map<IEnumerable<PostViewModel>>(post);
        }

        public async Task<IEnumerable<PostViewModel>> GetAllByUser(string? userid)
        {
            var post = await _context.Post.Where(x => x.UserID.Equals(userid))
                .Select(c => new Post
                {
                    Id = c.Id,
                    Title = c.Title,
                    Content = c.Content,
                    DateCreated = c.DateCreated,
                    ViewCount = c.ViewCount,
                    Votes = c.Votes,
                    UserID = c.UserID,
                    PostUserName = c.User.UserName // Lấy UserName của User thông qua Post
                }).
               ToListAsync();
            return _mapper.Map<IEnumerable<PostViewModel>>(post);
        }

        public async Task<PostViewModel> GetById(int? id)
        {
            var post = await _context.Post.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(c => new Post
                {
                    Id = c.Id,
                    Title = c.Title,
                    Content = c.Content,
                    DateCreated = c.DateCreated,
                    ViewCount = c.ViewCount,
                    Votes = c.Votes,
                    UserID = c.UserID,
                    PostUserName = c.User.UserName // Lấy UserName của User thông qua Post
                   
                }) 
                .FirstOrDefaultAsync();
            var myattachmentlist = await _attachmentService.GetAllByPost(id);
            var mylist = _mapper.Map<IEnumerable<Attachment>>(myattachmentlist);
            post.Attachments = mylist.ToList();
            return _mapper.Map<PostViewModel>(post);
        }

        public async Task<Result> Update(PostViewModel postviewmodel)
        {
            Result result = new Result();
            if (!PostExists(postviewmodel.Id))
            {
                // throw new Exception("Lesson does not exist");
                result.type = "NotFound";
                result.message = "NotFound";
                return result;
            }
            
            var mypost = await _context.Post.AsNoTracking().FirstOrDefaultAsync(m => m.Id == postviewmodel.Id); 
            postviewmodel.DateCreated = mypost.DateCreated;
            postviewmodel.UserID = mypost.UserID;
            postviewmodel.ViewCount = mypost.ViewCount;
            postviewmodel.Votes = mypost.Votes;
            ValidationResult validationResult = _postviewmodelvalidator.Validate(postviewmodel);
            if (validationResult.IsValid)
            {
                try
                {
                    /*  if (lesson.Image != null)
                      {
                          if (!string.IsNullOrEmpty(lesson.ImagePath))
                              await _storageService.DeleteFileAsync(lesson.ImagePath.Replace("/" + USER_CONTENT_FOLDER_NAME + "/", ""));
                          lesson.ImagePath = await SaveFile(lesson.Image);
                      }    */
                    // khong update attaachment trong day
                  /*  int postid = postviewmodel.Id;
                    if (postviewmodel.lsfile != null)
                    {
                        foreach (var item in postviewmodel.lsfile)
                        {
                            var filemodel = new AttachmentViewModel
                            {
                                Title = item.FileName,
                                file = item,
                                fileSize = item.Length.ToString(),
                                PostID = postid
                            };
                            result = await _attachmentService.Update(filemodel);
                        }
                    }   */
                    _context.Update(_mapper.Map<Post>(postviewmodel));
                    await _context.SaveChangesAsync();
                    result.type = "Success";
                    result.message = "Success";
                    // Xử lý cache
                    var cachedData = await _distributedCache.GetStringAsync(cacheKey);
                    if (cachedData != null)
                    {
                        // Cập nhật lại item bằng data mới và lưu vào cache
                        // Chuyển đổi chuỗi JSON
                        List<PostViewModel> cachedDataList;
                        cachedDataList = JsonSerializer.Deserialize<List<PostViewModel>>(cachedData);
                        var itemToUpdate = cachedDataList.FirstOrDefault(x => x.Id == postviewmodel.Id);
                        if (itemToUpdate != null)
                        {
                            itemToUpdate.Title = postviewmodel.Title;
                            itemToUpdate.Content = postviewmodel.Content;
                            // Chuyển đổi cachedDataList thành chuỗi JSON
                            IEnumerable<PostViewModel> myupdatetocache = cachedDataList;
                            var updatedDataJson = JsonSerializer.Serialize(myupdatetocache);
                            var updatedEncodedData = Encoding.UTF8.GetBytes(updatedDataJson);
                            // Lưu dữ liệu đã cập nhật vào cache
                            var cacheOptions = new DistributedCacheEntryOptions
                            {
                                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                            };
                            await _distributedCache.SetAsync(cacheKey, updatedEncodedData, cacheOptions);
                        }
                    }
                    
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

        public async Task<Result> Unvote(int id, JsonPatchDocument<VoteViewModel> patchDocument)
        {
            var result = new Result();
            if (patchDocument == null)
            {
                result.type = "Failure";
                result.message = "BadRequest";
                return result;
            }
            var postbyid = await _context.Post.FindAsync(id);
            if (postbyid == null)
            {
                result.type = "NotFound";
                result.message = "NotFound";
                return result;
            }
            VoteViewModel myvote = new VoteViewModel();
            patchDocument.ApplyTo(myvote);
            var deletevoteresult = await _voteService.Delete(myvote.UserID,myvote.PostID);
            if (deletevoteresult.type.Equals("Success"))
            {
                postbyid.Votes = postbyid.Votes - 1;
                await _context.SaveChangesAsync();
                result.type = "Success";
                result.message = deletevoteresult.message;

                // Xử lý cache
                var cachedData = await _distributedCache.GetStringAsync(cacheKey);
                if (cachedData != null)
                {
                    // Cập nhật lại item bằng data mới và lưu vào cache
                    // Chuyển đổi chuỗi JSON
                    List<PostViewModel> cachedDataList;
                    cachedDataList = JsonSerializer.Deserialize<List<PostViewModel>>(cachedData);
                    var itemToUpdate = cachedDataList.FirstOrDefault(x => x.Id == postbyid.Id);
                    if (itemToUpdate != null)
                    {
                        postbyid.User = null;
                        itemToUpdate.Votes = postbyid.Votes;
                        // Chuyển đổi cachedDataList thành chuỗi JSON
                        IEnumerable<PostViewModel> myupdatetocache = cachedDataList;
                        var updatedDataJson = JsonSerializer.Serialize(myupdatetocache);
                        var updatedEncodedData = Encoding.UTF8.GetBytes(updatedDataJson);
                        // Lưu dữ liệu đã cập nhật vào cache
                        var cacheOptions = new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                        };
                        await _distributedCache.SetAsync(cacheKey, updatedEncodedData, cacheOptions);
                    }
                }

            }
            else
            {
                result.type = "Failure";
                result.message = deletevoteresult.message;
            }
            return result;

        }

        public async Task<Result> PatchVote(int id , JsonPatchDocument<VoteViewModel> patchDocument)
        {
            var result = new Result();
            if (patchDocument == null)
            {
                result.type = "Failure";
                result.message = "BadRequest";
            }
            var postbyid = await _context.Post.FindAsync(id);
            if (postbyid == null)
            {
                result.type = "NotFound";
                result.message = "NotFound";
            }
            //   var VotePatch = new VotePatch { Vote = postbyid.Votes + 1 };
            // Update the vote
            VoteViewModel myvote = new VoteViewModel();
            patchDocument.ApplyTo(myvote);
            var insertvoteresult = await _voteService.Create(myvote);
            if (insertvoteresult.type.Equals("Success"))
            {
                postbyid.Votes = postbyid.Votes + 1;
                await _context.SaveChangesAsync();
                result.type = "Success";
                result.message = insertvoteresult.message;

                // Xử lý cache
                var cachedData = await _distributedCache.GetStringAsync(cacheKey);
                if (cachedData != null)
                {
                    // Cập nhật lại item bằng data mới và lưu vào cache
                    // Chuyển đổi chuỗi JSON
                    List<PostViewModel> cachedDataList;
                    cachedDataList = JsonSerializer.Deserialize<List<PostViewModel>>(cachedData);
                    var itemToUpdate = cachedDataList.FirstOrDefault(x => x.Id == postbyid.Id);
                    if (itemToUpdate != null)
                    {
                        postbyid.User = null;
                        itemToUpdate.Votes = postbyid.Votes;
                        // Chuyển đổi cachedDataList thành chuỗi JSON
                        IEnumerable<PostViewModel> myupdatetocache = cachedDataList;
                        var updatedDataJson = JsonSerializer.Serialize(myupdatetocache);
                        var updatedEncodedData = Encoding.UTF8.GetBytes(updatedDataJson);
                        // Lưu dữ liệu đã cập nhật vào cache
                        var cacheOptions = new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                        };
                        await _distributedCache.SetAsync(cacheKey, updatedEncodedData, cacheOptions);
                    }
                }



            }
            else
            {
                result.type = "Failure";
                result.message = insertvoteresult.message;
            }
            return result;
        /*    var postmodel = _mapper.Map<PostViewModel>(postbyid);
            ValidationResult validationResult = _postviewmodelvalidator.Validate(postmodel);
            if (validationResult.IsValid)
            {
                try
                {
                    //    postbyid.ViewCount = postmodel.ViewCount;
                    // Mark viewcount as modified
                    //     _context.Entry(postbyid).Property(x => x.ViewCount).IsModified = true;
                    await _context.SaveChangesAsync();
                    result.type = "Success";
                    result.message = "Success";
                }
                catch (Exception ex)
                {
                    result.type = "Failure";
                    result.message = ex.ToString();
                }
                return result;
            }
            else
            {
                result.type = "Failure";
                result.message = "Model isn't valid";
                return result;
            } */

        }
        public async Task<Result> PatchViewCount(int id, JsonPatchDocument<ViewCountPatch> patchDocument)
        {
            var result = new Result();
            if (patchDocument == null)
            {
                result.type = "Failure";
                result.message = "BadRequest";
            }
            var postbyid = await _context.Post.FindAsync(id);
            if (postbyid == null)
            {
                result.type = "NotFound";
                result.message = "NotFound";
            }

            var viewCountPatch = new ViewCountPatch { ViewCount = postbyid.ViewCount + 1 };
            // Update the view count
            postbyid.ViewCount = viewCountPatch.ViewCount;


            var postmodel = _mapper.Map<PostViewModel>(postbyid);
         //   Console.WriteLine("Original PostModel: " + JsonConvert.SerializeObject(postmodel));
            
       //     patchDocument.ApplyTo(postmodel);
            
       //     Console.WriteLine("Patched PostModel: " + JsonConvert.SerializeObject(postmodel));
            ValidationResult validationResult = _postviewmodelvalidator.Validate(postmodel);
            if (validationResult.IsValid)
            {
                try
                {
                //    postbyid.ViewCount = postmodel.ViewCount;
                    // Mark viewcount as modified
               //     _context.Entry(postbyid).Property(x => x.ViewCount).IsModified = true;
                    await _context.SaveChangesAsync();
                    result.type = "Success";
                    result.message = "Success";

                    // Xử lý cache
                    var cachedData = await _distributedCache.GetStringAsync(cacheKey);
                    if (cachedData != null)
                    {
                        // Cập nhật lại item bằng data mới và lưu vào cache
                        // Chuyển đổi chuỗi JSON
                        List<PostViewModel> cachedDataList;
                        cachedDataList = JsonSerializer.Deserialize<List<PostViewModel>>(cachedData);
                        var itemToUpdate = cachedDataList.FirstOrDefault(x => x.Id == postbyid.Id);
                        if (itemToUpdate != null)
                        {
                            postbyid.User = null;
                            itemToUpdate.ViewCount = postbyid.ViewCount;
                            // Chuyển đổi cachedDataList thành chuỗi JSON
                            IEnumerable<PostViewModel> myupdatetocache = cachedDataList;
                            var updatedDataJson = JsonSerializer.Serialize(myupdatetocache);
                            var updatedEncodedData = Encoding.UTF8.GetBytes(updatedDataJson);
                            // Lưu dữ liệu đã cập nhật vào cache
                            var cacheOptions = new DistributedCacheEntryOptions
                            {
                                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                            };
                            await _distributedCache.SetAsync(cacheKey, updatedEncodedData, cacheOptions);
                        }
                    }



                }
                catch (Exception ex)
                {
                    result.type = "Failure";
                    result.message = ex.ToString();
                }
                return result;
            }
            else
            {
                result.type = "Failure";
                result.message = "Model isn't valid";
                return result;
            }
        }

        public bool PostExists(int id)
        {
            return (_context.Post?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

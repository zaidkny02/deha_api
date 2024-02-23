using AutoMapper;
using deha_api_exam.Models;
using deha_api_exam.ViewModels;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Drawing.Printing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace deha_api_exam.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IValidator<RegisterViewModel> _validator;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly JwtOptions _jwtOptions;
        private readonly string UserRoleName = "Member";
        private readonly IValidator<UserViewModel> _userupdatevalidator;
        private readonly IDistributedCache _distributedCache;
        private string cacheKey = "list_user";
        public UserService(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager, IConfiguration config, IMapper mapper, IOptions<JwtOptions> jwtOptions, IValidator<RegisterViewModel> validator, IValidator<UserViewModel> userupdatevalidator, IDistributedCache distributedCache)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _config = config;
            _mapper = mapper;
            _jwtOptions = jwtOptions.Value;
            _validator = validator;
            _userupdatevalidator = userupdatevalidator;
            _distributedCache = distributedCache;
        }


        public async Task<IEnumerable<UserViewModel>> GetAll(string? keyword, int? page)
        {
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            int pageSize = 2;
            int pageCachedNumber = 5;
            if (keyword != null)
            {
                // keyword = "";
                var post = await _userManager.Users.Where(x => x.FullName.ToLower().Contains(keyword.ToLower())).ToListAsync();
                var returnlistwithkeyword = post.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                //var object.listcomment = returnlist
                //return object
                return _mapper.Map<IEnumerable<UserViewModel>>(returnlistwithkeyword);
            }
            else
            {
                IEnumerable<UserViewModel> cachedDataList;
                var cachedData = await _distributedCache.GetStringAsync(cacheKey);
                if (cachedData == null)
                {
                    var user = await _userManager.Users.Take(pageCachedNumber * pageSize).ToListAsync();
                    var listuser = _mapper.Map<IEnumerable<UserViewModel>>(user);
                    var newDataJson = JsonSerializer.Serialize(listuser);
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
                    cachedDataList = JsonSerializer.Deserialize<IEnumerable<UserViewModel>>(cachedData);
                    var cachedDataListReturn = cachedDataList.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                    return cachedDataListReturn;
                }
                else
                {
                    var listpost_fornotcached = await _userManager.Users.ToListAsync();
                    return _mapper.Map<IEnumerable<UserViewModel>>(listpost_fornotcached).Skip((pageNumber - 1) * pageSize).Take(pageSize);
                }
            }


          /*  if (keyword == null) keyword = "";
            var listuser = await _userManager.Users.Where(x => x.FullName.ToLower().Contains(keyword.ToLower())).ToListAsync();
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            int pageSize = 1;
            var returnlist = listuser.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            // Tạo ViewModel hoặc sử dụng ViewBag để truyền danh sách người dùng và thông tin phân trang đến view
           // ViewBag.PageNumber = page;
           // ViewBag.TotalPages = (int)Math.Ceiling((double)allUsers.Count / PageSize);
            return _mapper.Map<IEnumerable<UserViewModel>>(returnlist);  */
        }

        public async Task<bool> UserExists(string userID)
        {
            var user = await _userManager.FindByIdAsync(userID);
            if (user == null)
            {
                return false;
            }
            else
                return true;
        }

        public async Task<string> Authenticate(LoginViewModel request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName!);
            if (user == null)
            {
                throw new Exception("Couldn't find user with name " + request.UserName);
            }
            var result = await _signInManager.PasswordSignInAsync(user, request.Password!, request.RememberMe, true);
            if (!result.Succeeded)
            {
                throw new Exception("Couldn't sign in");
            }
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new[]
            {
                new Claim(ClaimTypes.Email,user.Email!),
                new Claim(ClaimTypes.GivenName,user.FullName!),
                new Claim(ClaimTypes.Role, string.Join(";",roles)),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey!));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _jwtOptions.Issuer,
                _jwtOptions.Audience,
                claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: signingCredentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<Result> Register(RegisterViewModel request)
        {
            ValidationResult validationResult = _validator.Validate(request);
            Result result = new Result();
         /*   var user = _mapper.Map<User>(request);
            var findmyuser = await _userManager.FindByNameAsync(request.UserName!);
            if (findmyuser != null)
            {
                throw new Exception("already have user with username " + request.UserName);
            }
            var result = await _userManager.CreateAsync(user, request.Password!);
            return result.Succeeded;  */


            if (validationResult.IsValid)
            {
                var userNameExists = await _userManager.FindByNameAsync(request.UserName) != null;
                if (userNameExists)
                {
                    result.type = "Failure";
                    result.message = "Username is already taken. Please choose a different username.";
                    return result;
                }

                var user = _mapper.Map<User>(request);
                user.DateCreated = DateTime.Now;

                //identity result when create
                var createresult = await _userManager.CreateAsync(user, request.Password);

                if (createresult.Succeeded)
                {
                    //Add auto role = member
                    await _userManager.AddToRoleAsync(user, UserRoleName);
                    result.type = "Success";
                    result.message = user.Id;
                    return result;
                }

                result.type = "Failure";
                foreach (var error in createresult.Errors)
                {
                    result.message = result.message + error.Description + "\n";
                }
                return result;

            }

            result.type = "Failure";
            result.message = "Model isn't valid";
            return result;
        }

        public async Task<UserViewModel> GetById(string userID)
        {
            var user = await _userManager.FindByIdAsync(userID);
            if (user == null)
            {
                throw new Exception("Couldn't find user");
            }
            else
                return _mapper.Map<UserViewModel>(user);
        }

        public async Task<Result> Delete(string id)
        {
            Result result = new Result();
            var user = await _userManager.FindByIdAsync(id);
            try
            {
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                    result.type = "Success";
                    result.message = "Success";
                    await _distributedCache.RemoveAsync(cacheKey);
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

        public async Task<Result> Update(UserViewModel userViewModel)
        {
            Result result = new Result();
            var user = await _userManager.FindByIdAsync(userViewModel.Id);
            if (user == null)
            {
                result.type = "NotFound";
                result.message = "NotFound";
            }
            else
            {
                ValidationResult validationResult = _userupdatevalidator.Validate(userViewModel);
                if (validationResult.IsValid)
                {
                    try
                    {
                        user.Dob = userViewModel.Dob;
                        user.FullName = userViewModel.FullName;
                        user.Email = userViewModel.Email;
                        user.PhoneNumber = userViewModel.PhoneNumber;
                        var updatemessage = await _userManager.UpdateAsync(user);
                        if (updatemessage.Succeeded)
                        {
                            result.type = "Success";
                            result.message = "Update Account Successfully";
                        }
                        else
                        {
                            // You can inspect the result.Errors property to get details about the errors
                            result.type = "Failure";
                            foreach (var err in updatemessage.Errors)
                                result.message = result.message + err + "\n";
                        }
                    }
                    catch (Exception ex)
                    {
                        result.type = "Failure";
                        result.message = ex.Message;
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
    }
}

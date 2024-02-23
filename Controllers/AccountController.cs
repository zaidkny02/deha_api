using deha_api_exam.Services;
using deha_api_exam.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace deha_api_exam.Controllers
{
    [Route("api/Accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;
        public AccountController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Authenticate([FromForm] LoginViewModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultToken = await _userService.Authenticate(request);
            return string.IsNullOrEmpty(resultToken) ? BadRequest("Username or password is incorrect.") : Ok(new { token = resultToken });
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromForm] RegisterViewModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.Register(request);
            var myuserobject = new
            {
                UserName = request.UserName,
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                DateofBirth = request.Dob,
            };
            return result.type.Equals("Failure") ? BadRequest("Register is unsuccessful.") : CreatedAtAction(nameof(GetById), new { id = result.message }, myuserobject);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll(string? keyword,int? page)
        {
            var result = await _userService.GetAll(keyword,page);
            return Ok(result);
        }



        [HttpDelete("{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string Id)
        {
            var result = await _userService.Delete(Id);
            return Ok(result.message);
        }
        [Authorize]
        [HttpGet("{Id}", Name = "GetUserByID")]
        public async Task<IActionResult> GetById(string Id)
        {
            var result = await _userService.GetById(Id);
            if (result == null)
                return NotFound();
            else
                return Ok(result);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Update([FromForm] UserViewModel request)
        {
            //Dành cho đọc giá trị token
            string authorizationHeader = HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
            {
                // Lấy giá trị của bearer token
                string bearerToken = authorizationHeader.Substring("Bearer ".Length).Trim();
                // Xử lý đọc token
                var handler = new JwtSecurityTokenHandler();
                try
                {
                    var jsonToken = handler.ReadToken(bearerToken) as JwtSecurityToken;
                    if (jsonToken != null)
                    {
                        var userId = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value.ToString();
                        //Xác thực
                        // Lấy danh sách các claims từ token, trong đó có roles.
                        var claims = jsonToken.Claims;
                        var roles = claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value)
                            .ToArray();
                        if (userId.Equals(request.Id))
                        {
                            var result = await _userService.Update(request);
                            return Ok(result.message);
                        }
                        else
                            return BadRequest("Can't change other user information");
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }

            }
            return BadRequest("This isn't bearer token");




        }
    }
}

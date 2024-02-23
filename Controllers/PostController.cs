using AutoMapper;
using deha_api_exam.Models;
using deha_api_exam.Services;
using deha_api_exam.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Xml.XPath;

namespace deha_api_exam.Controllers
{
    [Route("api/Posts")]
    [Authorize]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IMapper _mapper;
        public PostController(IPostService postService,IMapper mapper)
        {
            _postService = postService;
            _mapper = mapper;
        }

        #region ReadBearerToken
        //message = userid ; claims = claim of jwtToken
        public ReadBearerTokenResult ReadBearerToken()
        {
            var result = new ReadBearerTokenResult();
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

                        //  Console.WriteLine(userId + "/" + roles[0].ToString());
                        //    request.UserID = userId;
                        // lấy datetime
                        // request.DateCreated = DateTime.Now;
                        result.type = true;
                        result.message = userId;
                        result.claims = claims;
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    result.type = false;
                    result.message = ex.Message;
                    return result;
                }

            }
            result.type = false;
            result.message = "This isn't bearer token";
            return result;
        }
        #endregion

        [HttpGet("GetPagingFilter")]
        public async Task<IActionResult> GetAllPagingAndFilter(string? keyword, int? page)
        {
            var result = await _postService.GetAllPagingAndFilter(keyword,page);
            return Ok(result);
        }

        [HttpGet("PostwithComment/{PostID}")]
        public async Task<IActionResult> GetPostWithComment(int PostID,int? page)
        {
            var result = await _postService.GetPostwithComment(PostID,page);
            if (result == null)
                return NotFound();
            else
                return Ok(result);
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _postService.GetAll();
            return Ok(result);
        }

        [HttpGet("User/{UserID}")]
        public async Task<IActionResult> GetAllByUser(string UserID)
        {
            var result = await _postService.GetAllByUser(UserID);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] PostRequest request)
        {
            var tokenresult = ReadBearerToken();
            if (tokenresult.type)
            {
                var userId = tokenresult.message;
                //Code
                var result = await _postService.Create(request, userId);
                switch (result.type)
                {
                case "Success":
                     return CreatedAtAction(nameof(GetById), new { id = result.message }, request);
                case "NotFound":
                     return NotFound(result.message);
                case "Failure":
                     return BadRequest(result.message);
                }
            }
            return BadRequest(tokenresult.message);
        }
        [HttpGet("{Id}")]
        public async Task<IActionResult> GetById(int Id)
        {
            var result = await _postService.GetById(Id);
            if (result == null)
                return NotFound();
            else
                return Ok(result);
        }
        [HttpDelete("{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {
            var tokenresult = ReadBearerToken();
            if (tokenresult.type)
            {
                var userId = tokenresult.message;
                //Code
                var mypost = await _postService.GetById(Id);
                if (mypost == null) return NotFound("Post not found");
                if (mypost.UserID.Equals(userId))
                {
                    var result = await _postService.Delete(Id);
                    switch (result.type)
                    {
                        case "Success":
                            return NoContent();
                        case "NotFound":
                            return NotFound(result.message);
                        case "Failure":
                            return BadRequest(result.message);
                    }
                }
                else
                {
                    return BadRequest("Can't delete this post cause you aren't post's creator");
                }
            }
            return BadRequest(tokenresult.message);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromForm] PostUpdateRequest request)
        {
            var tokenresult = ReadBearerToken();
            if (tokenresult.type)
            {
                var userId = tokenresult.message;
                //Code
                var mypost = await _postService.GetById(request.Id);
                if (mypost == null) return NotFound("Post not found");
                if (mypost.UserID.Equals(userId))
                {
                    var result = await _postService.Update(_mapper.Map<PostViewModel>(request));
                    return Ok(result.message);
                }
                else
                {
                    return BadRequest("Can't edit this post cause you aren't post's creator");
                }
            }
            return BadRequest(tokenresult.message);
        }


        [HttpPatch("increment-vote")]
        public async Task<IActionResult> PatchVote(int id)
        {
            var tokenresult = ReadBearerToken();
            if (tokenresult.type)
            {
                var userId = tokenresult.message;
                //Code
                var patchDocument = new JsonPatchDocument<VoteViewModel>();
                patchDocument.Replace(x => x.UserID, userId);
                patchDocument.Replace(x => x.PostID, id);
                var result = await _postService.PatchVote(id, patchDocument);
                switch (result.type)
                {
                    case "Success":
                        return Ok("Success");
                    //     break;
                    case "NotFound":
                        return NotFound("Post not found");
                    //     break;
                    case "Failure":
                        return BadRequest(result.message);
                    //    break;
                    default:
                        return BadRequest();
                        //    break;
                }
            }
            return BadRequest(tokenresult.message);
        }


        [HttpPatch("unvote")]
        public async Task<IActionResult> Unvote(int id)
        {
            var tokenresult = ReadBearerToken();
            if (tokenresult.type)
            {
                var userId = tokenresult.message;
                //Code
                var patchDocument = new JsonPatchDocument<VoteViewModel>();
                patchDocument.Replace(x => x.UserID, userId);
                patchDocument.Replace(x => x.PostID, id);
                var result = await _postService.Unvote(id, patchDocument);
                switch (result.type)
                {
                    case "Success":
                        return Ok("Success");
                    //     break;
                    case "NotFound":
                        return NotFound("Post not found");
                    //     break;
                    case "Failure":
                        return BadRequest(result.message);
                    //    break;
                    default:
                        return BadRequest();
                        //    break;
                }
            }
            return BadRequest(tokenresult.message);

        }



        [HttpPatch("increment-view-count")]
        public async Task<IActionResult> PatchViewCount(int id, [FromBody] JsonPatchDocument<ViewCountPatch> patchDocument)
        {
            var result = await _postService.PatchViewCount(id, patchDocument);
            switch (result.type)
            {
                case "Success":
                    return Ok("Success");
                //     break;
                case "NotFound":
                    return NotFound();
                //     break;
                case "Failure":
                    return BadRequest(result.message);
                //    break;
                default:
                    return BadRequest();
                    //    break;
            }
        }
    }
}

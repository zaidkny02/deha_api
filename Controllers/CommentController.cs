using AutoMapper;
using deha_api_exam.Services;
using deha_api_exam.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace deha_api_exam.Controllers
{
    [Route("api/Comments")]
    [Authorize]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IMapper _mapper;
        public CommentController(ICommentService commentService,IMapper mapper)
        {
            _commentService = commentService;
            _mapper = mapper;
        }
        [HttpGet("Post/{PostID}")]
        public async Task<IActionResult> GetAllByPost(int PostID)
        {
            var result = await _commentService.GetAllByPost(PostID);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CommentRequest request)
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

                        var result = await _commentService.Create(request,userId);
                        switch(result.type)
                        {
                            case "Success":
                                return CreatedAtAction(nameof(GetById), new { id = result.message }, request);
                            case "NotFound":
                                return NotFound(result.message);
                            case "Failure":
                                return BadRequest(result.message);
                        }

                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }

            }
            return BadRequest("This isn't bearer token");
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetById(int Id)
        {
            var result = await _commentService.GetById(Id);
            if (result == null)
                return NotFound();
            else
                return Ok(result);
        }

        [HttpDelete("{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {

            string authorizationHeader = HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
            {
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
                        var mycomment = await _commentService.GetById(Id);
                        if (mycomment == null) return NotFound("Comment not found");
                        if (mycomment.UserID.Equals(userId))
                        {
                            var result = await _commentService.Delete(Id);
                            return Ok(result.message);
                        }
                        else
                        {
                            return BadRequest("Can't delete this comment cause you aren't comment's creator");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            return BadRequest("This isn't bearer token");


        }

        [HttpPut]
        public async Task<IActionResult> Update([FromForm] CommentUpdateRequest request)
        {

            string authorizationHeader = HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
            {
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
                        var mycomment = await _commentService.GetById(request.Id);
                        if (mycomment == null) return NotFound("Comment not found");
                        if (mycomment.UserID.Equals(userId))
                        {
                            var result = await _commentService.Update(_mapper.Map<CommentViewModel>(request));
                            return Ok(result.message);
                        }
                        else
                        {
                            return BadRequest("Can't edit this comment cause you aren't comment's creator");
                        }
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

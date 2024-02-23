using AutoMapper;
using deha_api_exam.Services;
using deha_api_exam.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace deha_api_exam.Controllers
{
    [Route("api/Attachments")]
    [Authorize]
    [ApiController]
    public class AttachmentController : ControllerBase
    {
        private readonly IAttachmentService _attachmentService;
        private readonly IMapper _mapper;
        private readonly IPostService _postService;
        public AttachmentController(IAttachmentService attachmentService, IMapper mapper, IPostService postService)
        {
            _attachmentService = attachmentService;
            _mapper = mapper;
            _postService = postService;
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] AttachmentRequest request)
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
                        var mypost = await _postService.GetById(request.PostID);
                        if (mypost == null) return NotFound("Post not found");
                        if (mypost.UserID.Equals(userId))
                        {
                            if (request.file != null)
                            {
                                var myFile = _mapper.Map<AttachmentViewModel>(request);
                                myFile.fileSize = request.file.Length.ToString();
                                myFile.Title = request.file.FileName;
                                myFile.PostUserID = "";
                                var result = await _attachmentService.Create(myFile);
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
                            else
                                return BadRequest("File not found");
                        }
                        else
                        {
                            return BadRequest("Can't create this attachment cause you aren't post's creator");
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

        [HttpGet("Post/{PostID}")]
        public async Task<IActionResult> GetAllByPost(int PostID)
        {
            var result = await _attachmentService.GetAllByPost(PostID);
            return Ok(result);
        }
        [HttpGet("{Id}")]
        public async Task<IActionResult> GetById(int Id)
        {
            var result = await _attachmentService.GetById(Id);
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
                        var myattachment = await _attachmentService.GetById(Id);
                        if (myattachment == null) return NotFound("Attachment not found");
                        if (myattachment.PostUserID.Equals(userId))
                        {
                            var result = await _attachmentService.Delete(Id);
                            return Ok(result.message);
                        }
                        else
                        {
                            return BadRequest("Can't delete this attachment cause you aren't attachment's creator");
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
        public async Task<IActionResult> Update([FromForm] AttachmentUpdateRequest request)
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
                        var myattachment = await _attachmentService.GetById(request.Id);
                        if (myattachment == null) return NotFound("attachment not found");
                        if (myattachment.PostUserID.Equals(userId))
                        {
                            var result = await _attachmentService.Update(_mapper.Map<AttachmentViewModel>(request));
                            return Ok(result.message);
                        }
                        else
                        {
                            return BadRequest("Can't edit this attachment cause you aren't attachment's creator");
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

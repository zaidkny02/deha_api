using deha_api_exam.Models;

namespace deha_api_exam.ViewModels
{
    public class AttachmentViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }

        public IFormFile? file { get; set; }
        public string? fileUrl { get; set; }
        public string? fileSize { get; set; }
        public int PostID { get; set; }
        public Post? Post { get; set; }
        public string? PostUserID { get; set; } // Thêm trường này để lưu trữ UserName của User thông qua Post
    }

    public class AttachmentRequest
    {
        public IFormFile? file { get; set; }
        public int PostID { get; set; }
    }

    public class AttachmentUpdateRequest
    {
        public int Id { get; set; }
        public IFormFile? file { get; set; }
    }
}

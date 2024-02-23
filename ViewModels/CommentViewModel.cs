using deha_api_exam.Models;
using System.ComponentModel.DataAnnotations;

namespace deha_api_exam.ViewModels
{
    public class CommentViewModel
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; }
        public Post? Post { get; set; }
        public int PostID { get; set; }
        public User? User { get; set; }
        public string UserID { get; set; }
        public string? CommentUserName { get; set; } // Thêm trường này để lưu trữ UserName của User thông qua Post
    }
    public class CommentView
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; }
        public int PostID { get; set; }
        public string UserID { get; set; }
        public string? CommentUserName { get; set; }
    }

    public class CommentUpdateRequest
    {
        public int Id { get; set; }
        public string? Content { get; set; }
    }

    public class CommentRequest
    {
        public string? Content { get; set; }
        public int PostID { get; set; }
    }

}

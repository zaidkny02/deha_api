using System.ComponentModel.DataAnnotations;

namespace deha_api_exam.Models
{
    public class Comment
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
}

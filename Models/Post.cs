using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace deha_api_exam.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; }
        public int ViewCount { get; set; }
        public int Votes { get; set; }
        public string UserID { get; set; }
        public User? User { get; set; }
        public ICollection<Attachment>? Attachments { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<Vote>? Vote { get; set; }
        public string? PostUserName { get; set; } // Thêm trường này để lưu trữ UserName của User thông qua Post
    }
}

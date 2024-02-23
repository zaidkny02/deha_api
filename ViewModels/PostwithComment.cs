using deha_api_exam.Models;
using System.ComponentModel.DataAnnotations;

namespace deha_api_exam.ViewModels
{
    public class PostwithComment
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; }
        public int ViewCount { get; set; }
        public int Votes { get; set; }
        public string UserID { get; set; }

        public ICollection<Attachment>? Attachments { get; set; }
        public List<CommentView>? ListComments { get; set; }
        public User? User { get; set; }
        public string? PostUserName { get; set; } // Thêm trường này để lưu trữ UserName của User thông qua Post

        public PostwithComment(int id, string? title, string? content, DateTime dateCreated, int viewCount, int votes, string userID,  List<CommentView>? listComments, string? postUserName)
        {
            Id = id;
            Title = title;
            Content = content;
            DateCreated = dateCreated;
            ViewCount = viewCount;
            Votes = votes;
            UserID = userID;
            ListComments = listComments;
            PostUserName = postUserName;
        }

        public PostwithComment() { }
    }
}

using deha_api_exam.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace deha_api_exam.ViewModels
{
    public class PostViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime DateCreated { get; set; }
        public string UserID { get; set; }
        public User? User { get; set; }
        public int ViewCount { get; set; }
        public int Votes { get;set; }
        public ICollection<Attachment>? Attachments { get; set; }
        public ICollection<Comment>? Comments { get; set; }
     //   public string? NewCommentContent { get; set; } // Property to hold the new comment content
        public IList<IFormFile>? lsfile { get; set; }  //this for upload multi file
        public string? PostUserName { get; set; } // Thêm trường này để lưu trữ UserName của User thông qua Post
        public PostViewModel() { }
        public PostViewModel(int id, string? title, string? content, string userID)
        {
            Id = id;
            Title = title;
            Content = content;
            DateCreated = DateTime.Now;
            UserID = userID;
            ViewCount = 0;
            Votes = 0;
        }
    }

    public class PostUpdateRequest
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public PostUpdateRequest(int id, string? title, string? content)
        {
            Id = id;
            Title = title;
            Content = content;
        }
        public PostUpdateRequest() { }
    }


    public class PostRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
    //    [DataType(DataType.DateTime)]
     //   public DateTime DateCreated { get; set; }
    //    public string UserID { get; set; }
       // public User? User { get; set; }
      //  public ICollection<Attachment>? Attachments { get; set; }
     //   public ICollection<Comment>? Comments { get; set; }
      //  public string? NewCommentContent { get; set; } // Property to hold the new comment content
        public IList<IFormFile>? lsfile { get; set; }  //this for upload multi file
        public PostRequest() { }
        public PostRequest(string? title, string? content)
        {
            Title = title;
            Content = content;
        }
    }
}

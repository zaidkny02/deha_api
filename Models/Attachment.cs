namespace deha_api_exam.Models
{
    public class Attachment
    {
        public int Id { get; set; }
        public string? Title { get; set; }

        public string? fileUrl { get; set; }
        public string? fileSize { get; set; }
        public int PostID { get; set; }
        public Post? Post { get; set; }
        public string? PostUserID { get; set; }
    }
}

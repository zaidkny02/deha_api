
namespace deha_api_exam.Models
{
    public class Vote
    {
        public int Id { get; set; }
        public string UserID { get; set; }
        public int PostID { get; set; }
        public User? User { get; set; }
        public Post? Post { get; set; }
    }
}

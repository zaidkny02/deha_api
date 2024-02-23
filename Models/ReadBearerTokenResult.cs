using System.Security.Claims;
namespace deha_api_exam.Models
{
    public class ReadBearerTokenResult
    {
        public bool type;
        public string? message;
        public IEnumerable<Claim>? claims;
    }
}

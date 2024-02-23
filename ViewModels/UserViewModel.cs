using System.ComponentModel;

namespace deha_api_exam.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }

        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string? FullName { get; set; }
        [DisplayName("Date of birth")]
        public DateTime Dob { get; set; }
    }
}

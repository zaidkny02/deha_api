using System.ComponentModel.DataAnnotations;

namespace deha_api_exam.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MaxLength(15)]
        public string? PhoneNumber { get; set; }
        [MaxLength(50)]
        [Required]
        public string? FullName { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Date of birth")]
        public DateTime Dob { get; set; }

    }
}

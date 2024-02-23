using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace deha_api_exam.Models
{
    public class User : IdentityUser
    {
        [MaxLength(50)]
        [Required]
        public string? FullName { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Date of birth")]
        public DateTime Dob { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Date created")]
        public DateTime DateCreated { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<Post>? Posts { get; set; }
        public ICollection<Vote>? Votes { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace HAS.Registration.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}

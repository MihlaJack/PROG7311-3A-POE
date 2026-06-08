using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models
{
    public class LoginViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }
    }

    public class UserSession
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
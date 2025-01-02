using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CommerceBack.DTOs.Auth;

public class SignupDto
{
    [DisplayName("Username")]
    [Required(ErrorMessage = "username is required")]
    public string? Username { get; set; }
    
    [DisplayName("Email")]
    [Required(ErrorMessage = "email is required")]
    [EmailAddress(ErrorMessage = "email is invalid")]
    public string? Email { get; set; }
    
    [DisplayName("Password")]
    [Required(ErrorMessage = "password is required")]
    [MinLength(8, ErrorMessage = "password must be at least 8 characters")]
    [RegularExpression("^(?=.*[A-Z])(?=.*[!@#$%^&*])[A-Za-z\\d!@#$%^&*]+$", 
        ErrorMessage = "Password must contain at least one uppercase letter and one special symbol (!@#$%^&*).")]
    public string? Password { get; set; }


}
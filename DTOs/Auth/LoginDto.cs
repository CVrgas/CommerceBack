using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CommerceBack.DTOs.Auth;

public class LoginDto
{
    [DisplayName("Username or Email")]
    [Required(ErrorMessage = "Credential is required")]
    public string? Credential { get; set; }
    
    [DisplayName("Password")]
    [Required(ErrorMessage = "Password is required")]
    public string? Password { get; set; }
    
    public bool RememberMe { get; set; }
}
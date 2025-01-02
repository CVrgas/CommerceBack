using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommerceBack.Entities;

namespace CommerceBack.DTOs.Token;

public class TokenCreationDto
{
    [DisplayName("Token")]
    [Required(ErrorMessage = "Token is required")]
    public string Token1 { get; set; }
    
}
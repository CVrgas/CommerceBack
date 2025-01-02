using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommerceBack.Common;
using CommerceBack.Entities;
using CommerceBack.Services;
using Microsoft.AspNetCore.Mvc;

namespace CommerceBack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TokenController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly UserService _userService;

    public TokenController(TokenService tokenService, UserService userService)
    {
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }
    
    [HttpGet("types")]
    public async Task<IActionResult> GetTypes()
    {
        var result = await _tokenService.Types.All();
        return  StatusCode((int)result.Code, result.IsOk ? result.Entity!.Select(t => t.Name).ToList() : result.Code);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> CreateType(TokenTypeCreateDto newType)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem();
        }

        var typeClass = new TokenType()
        {
            Name = newType.Name,
            StatusDefault = newType.StatusDefault,
            TimeSpanDefault = newType.TimeSpanDefault,
        };
        
        var result = await _tokenService.Types.Create(typeClass);
        return StatusCode((int)result.Code, result.Message);
    }
    
    [HttpPost("[action]")]
    public async Task<IActionResult> CreateStatus(string name)
    {
        var result = await _tokenService.Statuses.Create(new TokenStatus() { Name = name });
        return StatusCode((int)result.Code, result.Message);
    }

    public class TokenTypeCreateDto
    {
        [DisplayName("Status")]
        [Required(ErrorMessage = "Status is required")]
        public int StatusDefault { get; set; }
        
        [DisplayName("Name")]
        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }
        
        [DisplayName("Expiry")]
        [Required(ErrorMessage = "Expiry span is required")]
        public decimal TimeSpanDefault { get; set; }
    }
}
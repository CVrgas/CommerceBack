using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommerceBack.Entities;
using CommerceBack.Services.Base;
using CommerceBack.Services.Base.CreateService;
using Microsoft.AspNetCore.Mvc;

namespace CommerceBack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TokenController : ControllerBase
{
    private readonly ICreateService<TokenStatus> _tokenStatusService;
    private readonly ICrudService<TokenType> _tokenTypeService;

    public TokenController(ICrudService<TokenStatus> tokenStatusService, ICrudService<TokenType> tokenTypeService)
    {
        _tokenStatusService = tokenStatusService;
        _tokenTypeService = tokenTypeService;
    }
    
    [HttpGet("types")]
    public async Task<IActionResult> GetTypes()
    {
        var result = await _tokenTypeService.GetAll();
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
            Name = newType.Name!,
            StatusDefault = newType.StatusDefault,
            TimeSpanDefault = newType.TimeSpanDefault,
        };
        
        var result = await _tokenTypeService.Create(typeClass);
        return StatusCode((int)result.Code, result.Message);
    }
    
    [HttpPost("[action]")]
    public async Task<IActionResult> CreateStatus(string name)
    {
        var result = await _tokenStatusService.Create(new TokenStatus() { Name = name });
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
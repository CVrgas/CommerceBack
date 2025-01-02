using CommerceBack.DTOs.Auth;
using CommerceBack.Services;
using Microsoft.AspNetCore.Mvc;

namespace CommerceBack.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : Controller
	{
		private readonly AuthService _service;
		public AuthController(AuthService service)
		{
			_service = service ?? throw new ArgumentNullException(nameof(service));
		}

		[HttpPost("signup")]
		public async Task<IActionResult> Signup(SignupDto signupDto)
		{
			if (!ModelState.IsValid)
			{
				return ValidationProblem();
			}
			
			var result = await _service.SignUp(signupDto.Username!, signupDto.Email!, signupDto.Password!);
			return StatusCode((int)result.Code, result.Message);
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login(LoginDto request)
		{
			if (!ModelState.IsValid)
			{
				return ValidationProblem();
			}
			
			var result = await _service.LogIn(request.Credential!, request.Password!, request.RememberMe);
			return StatusCode((int)result.Code, result.IsOk ? result.Entity : result.Message);
		}

		[HttpGet("ResetPassword")]
		public async Task<IActionResult> ResetPassword(string email)
		{
			var result = await _service.RequestPasswordReset(email);
			return StatusCode((int)result.Code, result.IsOk ? result.Entity : result.Message);
		}

		[HttpPost("ResetPassword")]
		public async Task<IActionResult> ResetPassword(NewPasswordDto request)
		{
			if (!ModelState.IsValid)
			{
				return ValidationProblem();
			}
			var result = await _service.ResetPassword(request.Token!, request.Password!);
			return StatusCode((int)result.Code, result.Message);
		}
	}
}

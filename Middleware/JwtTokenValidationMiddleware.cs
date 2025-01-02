using CommerceBack.Common;
using CommerceBack.Services;

namespace CommerceBack.Middleware
{
	public class JwtTokenValidationMiddleware : IMiddleware
	{
		private readonly TokenBlacklistService _blacklistService;
		private readonly Jwt _token;

		public JwtTokenValidationMiddleware(TokenBlacklistService blacklistService, Jwt token)
		{
			_blacklistService = blacklistService;
			_token = token;
		}

		public async Task InvokeAsync(HttpContext context, RequestDelegate next)
		{
			// Check for the token in the Authorization header
			string? token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

			if (token != null && _blacklistService.IsTokenRevoked(token))
			{
				context.Response.StatusCode = 401; // Unauthorized
				return;
			}

			await next(context);
		}


	}
}

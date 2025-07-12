using System.IdentityModel.Tokens.Jwt;
using CommerceBack.Common;
using CommerceBack.Repository;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using CommerceBack.Common.OperationResults;
using CommerceBack.Entities;
using CommerceBack.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Role = CommerceBack.Entities.Role;

namespace CommerceBack.Services
{
	public class AuthService
	{
		//private readonly IEntityStore<PasswordResetCode> _store;
		private readonly IUnitOfWork _unitOfWork;
		private readonly UserService _userService;
		private readonly TokenService _tokenService;
		private readonly Jwt _jwt;
		private readonly ILogger<AuthService> _logger;
		private static readonly Random Random = new Random();
		private const int MaxAccessAttempts = 5;

		public AuthService(UserService userService, Jwt jwt, ILogger<AuthService> logger, TokenService tokenService, IUnitOfWork unitOfWork)
		{
			_userService = userService;
			_jwt = jwt;
			_logger = logger;	
			_tokenService = tokenService;
			_unitOfWork = unitOfWork;
		}

		public async Task<IReturnObject<User>> SignUp(string username, string email, string password)
		{
			await _unitOfWork.BeginTransactionAsync();
			
			try
			{
				if(!IsValidEmail(email)) return new ReturnObject<User>().BadRequest($"{email} is not a valid email.");
				if(!IsValidPassword(password)) return new ReturnObject<User>().BadRequest($"{password} is not a valid password.");
				if (!IsValidUsername(username)) return new ReturnObject<User>().BadRequest($"{username} is not a valid username.");
				var emailExist = await _userService.Exist(u => u.Email == email);
				var usernameExist = await _userService.Exist(u => u.Username == username);

				if (emailExist || usernameExist)
				{
					var message = emailExist 
						? "The provided email address is already in use. Please use a different email." 
						: "The username you entered is already taken. Please choose a different username.";

					return new ReturnObject<User>(false, message, HttpStatusCode.Conflict);
				}

				var salt = Jwt.GetNewSalt();
				var user = new User
				{
					Username = username,
					Email = email,
					Salt = _jwt.ToBase64(salt),
					Password = _jwt.HashPassword(password, salt),
					Role = this.GetDefaultRole(),
					
				};

				// Save user to the database
				user.CreationDate = DateTime.UtcNow;
				user.LastAccessDate = DateTime.UtcNow;
				user.CartNavigation = new Cart() { User = user};
				
				var result = await _userService.Create(user);
				await _unitOfWork.CommitAsync();

				return result.IsOk ? new ReturnObject<User>().Ok(user) : result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "SignUp Error");
				await _unitOfWork.RollbackAsync();
				return new ReturnObject<User>().InternalError();
			}
		}

		private int GetDefaultRole()
		{
			return _unitOfWork.Repository<Role>().Get(r => r.Name.Equals("user", StringComparison.CurrentCultureIgnoreCase)).Id;
		}

		public async Task<IReturnObject<UserDto>> LogIn(string credential, string password, bool remember = false)
		{
			try
			{
				// Get user using credential
				var result = await _userService.Find(u => u.Email == credential || u.Username == credential,
					query => query.Include(u => u.RoleNavigation)
				);
				var responseObject = new ReturnObject<UserDto>();

				// if user not found. prevent login
				if (!result.IsOk || result.Entity == null)
				{
					return responseObject.NotFound("username or password is incorrect.");
				}
				
				var user = result.Entity;
				
				// If user account is disabled or locked. prevent login.
				if (user.IsLocked || user.IsDisabled)
				{
					return responseObject.BadRequest("Account is disabled. Please contact support.");
				}
				
				// compare provided password against user password.
				var hashedPassword = _jwt.HashPassword(password, _jwt.FromBase64(user.Salt));
				if (hashedPassword == user.Password)
				{
					// Generate the access and refresh tokens
					var tokenResult = await _tokenService.CreateToken(user, "access");

					if (!tokenResult.IsOk)
					{
						return new ReturnObject<UserDto>().BadRequest(tokenResult.Message);
					}
					
					if(remember)
					{
						await _tokenService.CreateToken(user, "refresh");
					}

					user.LastAccessDate = DateTime.UtcNow;
					user.AccessAttempts = 0;

					await _userService.Update(user);
					return responseObject.Ok(new UserDto(user, tokenResult.Entity));
				}
				else
				{
					user.AccessAttempts++;
					string message = "username or password is incorrect.";

					if (user.AccessAttempts >= MaxAccessAttempts)
					{
						user.IsLocked = true;
						message = "Too many attempts, account locked.";
					}

					await _userService.Update(user);
					return responseObject.BadRequest(message);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error logging in user {credential}");
				return new ReturnObject<UserDto>().InternalError();
			}
		}

		// Helper methods for validation
		private static bool IsValidEmail(string email)
		{
			// Use regex or other methods to validate email format
			return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
		}
		private static bool IsValidPassword(string password)
		{
			// Implement password strength validation logic
			return password.Length >= 8; // Example rule
		}
		private static bool IsValidUsername(string username)
		{
			// Example validation: check length and allowed characters
			return !string.IsNullOrWhiteSpace(username) && username.Length is >= 3 and <= 20; // Customize as needed
		}

		public async Task<IReturnObject<string>> RequestPasswordReset(string email)
		{
			try
			{
				var userResult = await _userService.Find(u => u.Email == email);

				if (!userResult.IsOk || userResult.Entity == null) return new ReturnObject<string>(userResult.IsOk, userResult.Message, userResult.Code);

				var result = await _tokenService.CreateToken(userResult.Entity, "restore");

				return !result.IsOk ?
					new ReturnObject<string>().BadRequest(result.Message) :
					new ReturnObject<string>().Ok(result.Entity);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Sending Reset code to user({email})");
				return new ReturnObject<string>().InternalError();
			}

		}

		public async Task<IReturnObject<bool>> ResetPassword(string token, string newPassword)
		{
			await _unitOfWork.BeginTransactionAsync();
			try
			{
				var (prc, user) = await ValidateResetToken(token);

				if (user == null || prc == null) return new ReturnObject<bool>().BadRequest("Invalid Token");

				var newSalt = Jwt.GetNewSalt();
				user.Salt = _jwt.ToBase64(newSalt);
				user.Password = _jwt.HashPassword(newPassword, _jwt.FromBase64(user.Salt));
				user.IsLocked = false;
				prc.Status = 2;

				await _userService.Update(user);
				await _unitOfWork.Repository<Token>().Update(prc);

				// Commit the transaction
				await _unitOfWork.CommitAsync();

				return new ReturnObject<bool>().Ok();
			}
			catch (Exception ex)
			{
				await _unitOfWork.RollbackAsync();
				_logger.LogError(ex, $"Resetting password, token {token}");
				return new ReturnObject<bool>().InternalError("Error Curred while Resetting password");
			}
		}

		private async Task<(Token? prc, User? user)> ValidateResetToken(string token)
		{
			var prc = await _unitOfWork.Repository<Token>().Get(prc => prc.Value == token);

			if (prc is not { Status: 1 } || prc.Expiration < DateTime.UtcNow) return (null, null);

			var userResult = await _userService.GetById(prc.UserId);
			if(!userResult.IsOk || userResult.Entity == null) return (null, null);

			return (prc, userResult.Entity);
		}
		
		public IReturnObject<int> GetUserIdFromJwt(string token)
		{
			if(string.IsNullOrWhiteSpace(token)) return new ReturnObject<int>().BadRequest("Invalid Token");

			var claims = _jwt.ValidateToken(token, new TokenType(){ Name = "access"});

			var userIdString = claims?.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

			return int.TryParse(userIdString, out var userId) ? 
				new ReturnObject<int>().Ok(userId) : 
				new ReturnObject<int>().BadRequest("Invalid Token");
		}
	}

	public class UserDto(User user, string? accessToken = null)
	{
		public int UserId { get; set; } = user.Id;
		public string Username { get; set; } = user.Firstname != null ? $"{user.Firstname} {user.Lastname}" : user.Username;
		public string Role { get; set; } = user.RoleNavigation.Name;
		public string Currency { get; set; } = string.Empty;
		public List<int> Cart { get; set; } = [];
		public List<int> Wishlist { get; set; } = [];
		public string? AccessToken { get; set; } = accessToken;

	}

}

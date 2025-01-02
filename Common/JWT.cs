using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CommerceBack.Entities;

namespace CommerceBack.Common
{
	public class Jwt(string secretKey, string issuer, string audience, string refreshAudience, string refreshSecretKey)
	{
		private Random Random { get; set; } = new Random();

		public (string, string) GenerateToken(User user, TokenType type, ClaimsConfig? config = null)
		{

			if (type.Id == 3)
			{
				var code = GenerateSixDigitCode().ToString();
				return (code, code);
			};
			
			var (key, aud) = GetParamsConfig(type);
			
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
			
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			var claimsConfig = config ?? new ClaimsConfig().Set(type, user);

			var token = new JwtSecurityToken(
				issuer: issuer,
				audience: aud,
				claimsConfig.Claims.ToArray(),
				expires: DateTime.UtcNow.AddDays((double)type.TimeSpanDefault),
				signingCredentials: credentials
			);

			var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

			return (tokenString, claimsConfig.Claims.FirstOrDefault(c => c.Type == "jti")?.Value ?? "");
		}

		// Validate a Jwt token and return a ClaimsPrincipal if valid.
		public ClaimsPrincipal? ValidateToken(string token, TokenType type)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var validationParameters = GetValidationParameters(type);

			try
			{
				SecurityToken validatedToken;
				var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
				return principal;
			}
			catch (Exception ex)
			{
				// Handle other exceptions that might occur during token validation.
				Console.WriteLine($"An unexpected error occurred during token validation: {ex.Message}");
				return null;
			}
		}

		// Get the validation parameters for Jwt token validation.
		private TokenValidationParameters GetValidationParameters(TokenType type)
		{
			var (key, aud) = GetParamsConfig(type);
			
			var tokenParams = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = issuer,
				
				ValidAudience = aud,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
				// Optional: Set the tolerance for the token's expiration time.
				//ClockSkew = TimeSpan.FromMinutes(5)
			};
			return tokenParams;
		}

		private (string, string) GetParamsConfig(TokenType type)
		{
			return type.Name switch
			{
				"Refresh" => (refreshSecretKey, refreshAudience),
				_ => (secretKey, audience)
			};
		}

		public static byte[] GetNewSalt(int size = 32)
		{
			byte[] salt = new byte[size];

			using (var rng = new RNGCryptoServiceProvider())
			{
				rng.GetBytes(salt);
			}

			return salt;
		}

		/// <summary>
		/// Convert a byte array to a base64 string
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public string ToBase64(byte[] bytes)
		{
			return Convert.ToBase64String(bytes);
		}

		/// <summary>
		/// Convert a base64 string to a byte array
		/// </summary>
		/// <param name="base64String"></param>
		/// <returns></returns>
		public byte[] FromBase64(string base64String)
		{
			return Convert.FromBase64String(base64String);
		}

		public string HashPassword(string password, byte[] salt)
		{
			using var sha256 = SHA256.Create();
			var completedPassword = $"{password}{salt}";
			var byteValue = Encoding.UTF8.GetBytes(completedPassword);
			var byteHash = sha256.ComputeHash(byteValue);
			var hash = Convert.ToBase64String(byteHash);
			return hash;
		}

		private int GenerateSixDigitCode()
		{
			lock (Random) // Ensure thread safety
			{
				return Random.Next(100000, 1000000); // Generates a random 6-digit integer
			}
		}
	}
	
	public class ClaimsConfig
	{
		public IEnumerable<Claim> Claims { get; set; }

		public ClaimsConfig Set(TokenType type, User user)
		{
			return type.Name.ToLower() switch
			{
				"access" => this.Default(user),
				"refresh" => this.Refresh(user),
				_ => this.Default(user),
			};
		}

		public ClaimsConfig Default(User user)
		{
			return new ClaimsConfig()
			{
				Claims = new List<Claim>()
				{
					new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
					new Claim(ClaimsIdentity.DefaultNameClaimType, "access"),
					new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				}
			};
		}
			
		public ClaimsConfig Refresh( User user)
		{
			return new ClaimsConfig()
			{
				Claims = new List<Claim>()
				{
					new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
					new Claim(ClaimsIdentity.DefaultNameClaimType, "refresh"),
					new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
					new Claim(ClaimTypes.Email, user.Email),
				}
			};
		}
	}
}

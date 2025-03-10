using CommerceBack.Common;
using CommerceBack.Common.OperationResults;
using CommerceBack.Entities;
using CommerceBack.Services.Base;

namespace CommerceBack.Services;

public class TokenService
{
    private readonly ICrudService<Token> _tokens;
    private readonly ICrudService<TokenType> _tokenType;
    private readonly Jwt _jwt;
    private readonly ILogger<TokenService> _logger;

    public TokenService(Jwt jwt, ILogger<TokenService> logger, ICrudService<Token> tokens, ICrudService<TokenType> tokenType)
    {
        _jwt = jwt;
        _logger = logger;
        _tokens = tokens;
        _tokenType = tokenType;
    }

    public async Task<IReturnObject<string>> CreateToken(User user, string type, int status = 0)
    {
        try
        {
            var tokenType = await _tokenType.Find(t => t.Name.ToLower() == type.ToLower());

            if (!tokenType.IsOk) return new ReturnObject<string>(tokenType.IsOk, tokenType.Message, tokenType.Code);

            var (tokenString, jti) = _jwt.GenerateToken(user, tokenType.Entity!);

            if (string.IsNullOrWhiteSpace(tokenString))
                return new ReturnObject<string>().BadRequest("Error generating token");

            var newToken = new Token()
            {
                Value = jti,
                TokenType = tokenType.Entity!.Id,
                UserId = user.Id,
                Status = status > 0 ? status : tokenType.Entity!.StatusDefault,
                Expiration = DateTime.UtcNow.AddDays((double)tokenType.Entity!.TimeSpanDefault),
            };
            
            var prevTokens = (await _tokens.GetAll(t => t.UserId == user.Id && t.TokenType == newToken.TokenType)).Entity;
            if (prevTokens != null)
            {
                var tokens = prevTokens.ToList();
                tokens.ToList().ForEach(token => token.Status = 2);
                await _tokens.BulkUpdate(tokens);
            }
            
            var result = await _tokens.Create(newToken);
            return !result.IsOk ? 
                new ReturnObject<string>(result.IsOk, result.Message, result.Code) :
                new ReturnObject<string>().Ok(entity:tokenString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating token, user id: {user.Id}, type: {type}, status: {status}");
            return new ReturnObject<string>().InternalError("Error generating token");
        }
    }

    public async Task<IReturnObject<bool>> ValidateToken(string token, string type)
    {
        var tokenType = await _tokenType.Find(t => t.Name.ToLower() == type.ToLower());
        
        if(!tokenType.IsOk) return new ReturnObject<bool>(tokenType.IsOk, tokenType.Message, tokenType.Code);
        
        var result = _jwt.ValidateToken(token, tokenType.Entity!);
        var jti = result?.Claims.FirstOrDefault(c => c.Type == "jti") ?? null;
        
        if(result == null || jti == null) return new ReturnObject<bool>().NotFound();

        var isValid = await _tokens.Find(t => t.Value == jti.Value && t.Status != 2);
        
        return isValid.IsOk ? new ReturnObject<bool>().Ok(true) : new ReturnObject<bool>().NotFound();

    }

    public async Task<IReturnObject<Token>> GetToken(User userResultEntity, int type = 1)
    {
        return await _tokens.Find(t => t.UserId == userResultEntity.Id && t.TokenType == type);
    }
}









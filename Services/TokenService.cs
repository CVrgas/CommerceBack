using CommerceBack.Common;
using CommerceBack.Common.OperationResults;
using CommerceBack.Entities;
using CommerceBack.Services.Base;
using CommerceBack.UnitOfWork;

namespace CommerceBack.Services;

public class TokenService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly Jwt _jwt;
    private readonly ILogger<TokenService> _logger;

    public TokenService(Jwt jwt, ILogger<TokenService> logger, IUnitOfWork  unitOfWork)
    {
        _jwt = jwt;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReturnObject<string>> CreateToken(User user, string type, int status = 0)
    {
        try
        {
            var tokenType = await _unitOfWork.Repository<TokenType>().Get(t => t.Name.ToLower() == type.ToLower());

            if (tokenType == null) return new ReturnObject<string>().NotFound();

            var (tokenString, jti) = _jwt.GenerateToken(user, tokenType);

            if (string.IsNullOrWhiteSpace(tokenString))
                return new ReturnObject<string>().BadRequest("Error generating token");

            var newToken = new Token()
            {
                Value = jti,
                TokenType = tokenType.Id,
                UserId = user.Id,
                Status = status > 0 ? status : tokenType.StatusDefault,
                Expiration = DateTime.UtcNow.AddDays((double)tokenType.TimeSpanDefault),
            };
            
            var prevTokens = (await _unitOfWork.Repository<Token>().GetAll(t => t.UserId == user.Id && t.TokenType == newToken.TokenType));
            if (prevTokens != null)
            {
                var tokens = prevTokens.ToList();
                tokens.ToList().ForEach(token => token.Status = 2);
                await _unitOfWork.Repository<Token>().UpdateRange(tokens);
            }
            
            var result = await _unitOfWork.Repository<Token>().Create(newToken);
            return result == null ? 
                new ReturnObject<string>().InternalError("Error creating token") :
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
        var tokenType = await _unitOfWork.Repository<TokenType>().Get(t => t.Name.ToLower() == type.ToLower());
        
        if(tokenType == null) return new ReturnObject<bool>().NotFound();
        
        var result = _jwt.ValidateToken(token, tokenType);
        var jti = result?.Claims.FirstOrDefault(c => c.Type == "jti") ?? null;
        
        if(result == null || jti == null) return new ReturnObject<bool>().NotFound();

        var isValid = await _unitOfWork.Repository<Token>().Get(t => t.Value == jti.Value && t.Status != 2);
        
        return isValid != null ? new ReturnObject<bool>().Ok() : new ReturnObject<bool>().NotFound();

    }

    public async Task<IReturnObject<Token>> GetToken(User userResultEntity, int type = 1)
    {
        var token = await _unitOfWork.Repository<Token>().Get(t => t.UserId == userResultEntity.Id && t.TokenType == type);
        if(token == null) return new ReturnObject<Token>().NotFound();
        return new ReturnObject<Token>().Ok(token);
    }
}









using System.Net;

namespace CommerceBack.Common.OperationResults;

public abstract class ReturnObjectBase<T> : IReturnObject<T>
{
    protected ReturnObjectBase()
    {
        Message = string.Empty;
        Entity = default;
        Code = HttpStatusCode.ExpectationFailed;
        IsOk = false;
        Paginated = null;
    }

    protected ReturnObjectBase(IReturnObject returnObject)
    {
        IsOk = returnObject.IsOk;
        Message = returnObject.Message;
        Code = returnObject.Code;
    }

    protected ReturnObjectBase(bool isOk, string message, HttpStatusCode code, T? entity = default)
    {
        IsOk = isOk;
        Message = message;
        Code = code;
        Entity = entity;
        Paginated = null;
    }

    protected ReturnObjectBase(bool isOk, string message, HttpStatusCode code, PaginatedResponse<T> paginated)
    {
        IsOk = isOk;
        Message = message;
        Code = code;
        Paginated = paginated;

        Entity = default;
    }
    
    public bool IsOk { get; set; }
    public string Message { get; set; }
    public T? Entity { get; set; }
    public PaginatedResponse<T>? Paginated { get; set; }
    public HttpStatusCode Code { get; set; }
    
    
    IReturnObject IReturnObject.NotFound(string? message)
    {
        return NotFound(message);
    }
    
    IReturnObject IReturnObject.InternalError(string? message)
    {
        return InternalError(message);
    }

    IReturnObject IReturnObject.Ok(string? message)
    {
        return Ok(message);
    }

    IReturnObject IReturnObject.BadRequest(string? message)
    {
        return BadRequest(message);
    }
    

    public IReturnObject<T> InternalError(string? message = ReturnObjectDefaultMessage.InternalError)
    {
        return new ReturnObject<T>()
        {
            IsOk = false,
            Message = message!,
            Entity = default,
            Code = HttpStatusCode.InternalServerError,
            Paginated = null,
        };
    }

    public IReturnObject<T> Ok(string message)
    {
        return new ReturnObject<T>()
        {
            IsOk = true,
            Message = message,
            Entity = default,
            Code = HttpStatusCode.OK,
            Paginated = null,
        };
    }

    public IReturnObject<T> BadRequest(string? message = ReturnObjectDefaultMessage.BadRequest)
    {
        return new ReturnObject<T>()
        {
            IsOk = false,
            Message = message,
            Entity = default,
            Code = HttpStatusCode.BadRequest,
            Paginated = null,
        };
    }

    public IReturnObject<T> Ok(T entity, string message = ReturnObjectDefaultMessage.Ok)
    {
        return new ReturnObject<T>()
        {
            IsOk = true,
            Message = message,
            Entity = entity,
            Code = HttpStatusCode.OK,
            Paginated = null,
        };
    }

    public IReturnObject<T> Ok(PaginatedResponse<T>? paginated, string message = ReturnObjectDefaultMessage.Ok)
    {
        return new ReturnObject<T>()
        {
            IsOk = true,
            Message = message,
            Entity = default,
            Code = HttpStatusCode.OK,
            Paginated = paginated,
        };
    }

    public IReturnObject<T> Ok()
    {
        return new ReturnObject<T>()
        {
            IsOk = true,
            Message = "Success",
            Entity = default,
            Code = HttpStatusCode.OK,
            Paginated = null,
        };
    }

    public IReturnObject<T> NotFound(string? message = ReturnObjectDefaultMessage.NotFound)
    {
        return new ReturnObject<T>()
        {
            IsOk = false,
            Message = message ?? $"{typeof(T).Name} is not found",
            Entity = default,
            Code = HttpStatusCode.NotFound,
            Paginated = null,
        };
    }
}
public abstract class ReturnObjectBase : IReturnObject
{
    protected ReturnObjectBase()
    {
        Message = string.Empty;
        Code = HttpStatusCode.ExpectationFailed;
        IsOk = false;
    }

    protected ReturnObjectBase(IReturnObject returnObject)
    {
        IsOk = returnObject.IsOk;
        Message = returnObject.Message;
        Code = returnObject.Code;
    }
    
    protected ReturnObjectBase(bool isOk, string message, HttpStatusCode code)
    {
        IsOk = isOk;
        Message = message;
        Code = code;
    }
    
    public bool IsOk { get; set; }
    public string Message { get; set; }
    public HttpStatusCode Code { get; set; }
    
    public IReturnObject NotFound(string? message = ReturnObjectDefaultMessage.NotFound)
    {
        return new ReturnObject
        {
            IsOk = false,
            Message = message ?? $"not found",
            Code = HttpStatusCode.NotFound
        };
    }
    public IReturnObject BadRequest(string? message = ReturnObjectDefaultMessage.BadRequest)
    {
        return new ReturnObject
        {
            IsOk = false,
            Message = message ?? "Bad Request",
            Code = HttpStatusCode.BadRequest
        };
    }
    public IReturnObject Ok(string message = ReturnObjectDefaultMessage.Ok)
    {
        return new ReturnObject
        {
            IsOk = true,
            Message = message,
            Code = HttpStatusCode.OK
        };
    }
    public IReturnObject InternalError(string message = ReturnObjectDefaultMessage.InternalError)
    {
        return new ReturnObject
        {
            IsOk = false,
            Message = message,
            Code = HttpStatusCode.InternalServerError
        };
    }
}
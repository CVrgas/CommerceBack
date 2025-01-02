using System.Net;

namespace CommerceBack.Common.OperationResults;

public interface IReturnObject<T> : IReturnObject
{
    T? Entity { get; set; }
    PaginatedResponse<T>? Paginated { get; set; }
    new IReturnObject<T> NotFound(string? message = ReturnObjectDefaultMessage.NotFound);
    new IReturnObject<T> InternalError(string? message = ReturnObjectDefaultMessage.InternalError);
    new IReturnObject<T> Ok();
    new IReturnObject<T> Ok(string message);
    new IReturnObject<T> Ok(T entity, string message = ReturnObjectDefaultMessage.Ok);
    new IReturnObject<T> Ok(PaginatedResponse<T>? paginated, string message = ReturnObjectDefaultMessage.Ok);
    new IReturnObject<T> BadRequest(string? message = ReturnObjectDefaultMessage.BadRequest);
}

public interface IReturnObject
{
    bool IsOk { get; set; }
    string Message { get; set; }
    HttpStatusCode Code { get; set; }
    IReturnObject NotFound(string? message = ReturnObjectDefaultMessage.NotFound);
    IReturnObject InternalError(string? message = ReturnObjectDefaultMessage.InternalError);
    IReturnObject Ok(string? message = ReturnObjectDefaultMessage.Ok);
    IReturnObject BadRequest(string? message = ReturnObjectDefaultMessage.BadRequest);
}
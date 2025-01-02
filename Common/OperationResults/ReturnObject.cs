using System.Net;

namespace CommerceBack.Common.OperationResults;

public class ReturnObject<T> : ReturnObjectBase<T>
{
    public ReturnObject()
    {
        Message = string.Empty;
        Entity = default;
        Code = HttpStatusCode.ExpectationFailed;
        IsOk = false;
        Paginated = null;
    }
    public ReturnObject(IReturnObject returnObject)
    {
        IsOk = returnObject.IsOk;
        Message = returnObject.Message;
        Code = returnObject.Code;
        
    }
    public ReturnObject(bool isOk, string message, HttpStatusCode code, T? entity = default)
    {
        IsOk = isOk;
        Message = message;
        Code = code;
        Entity = entity;
        Paginated = null;
    }

    public ReturnObject(bool isOk, string message, HttpStatusCode code, PaginatedResponse<T> paginated)
    {
        IsOk = isOk;
        Message = message;
        Code = code;
        Paginated = paginated;
        Entity = default;
    }
    
}

public class ReturnObject : ReturnObjectBase
{
    public ReturnObject(){}

    public ReturnObject(bool isOk, string message, HttpStatusCode code)
    {
        IsOk = isOk;
        Message = message;
        Code = code;
    }
}
namespace CommerceBack.Common.OperationResults;

public static class ReturnObjectDefaultMessage
{
    public const string Ok = "Operation completed successfully.";
    public const string NotFound = "The requested item could not be found.";
    public const string InternalError = "An error occurred. Please try again later.";
    public const string BadRequest = "Invalid request. Please check the input and try again.";
}
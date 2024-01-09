namespace XWave.Core.Services.Communication;

public record Error
{
    public string Message { get; } = string.Empty;
    public ErrorCode Code { get; } = ErrorCode.None;

    private Error(ErrorCode code, string message)
    {
        Code = code;
        Message = message;
    }

    private static string GetDefaultErrorMessage(ErrorCode code)
    {
        return code switch
        {
            ErrorCode.Undefined => "An unknown error occurred",
            ErrorCode.EntityNotFound => "The requested entity is not found",
            ErrorCode.InvalidState => "The operation is invalid due to the current state of the resource",
            ErrorCode.InvalidArgument => "One or more arguments in the request are invalid",
            ErrorCode.ConflictingState => "The requested operation and existing resource are in conflicting states",
            ErrorCode.AuthorizationError => "User not authorized to access or modify this resource",
            ErrorCode.AuthenticationError => "User must be authenticated to access or modify this resource",
            ErrorCode.None => "No error occurred",
            _ => throw new NotImplementedException()
        };
    }

    public static Error With(ErrorCode code, string? message = null)
    {
        return new Error(code, message ?? GetDefaultErrorMessage(code));
    }

    public static Error UnknownError() => With(ErrorCode.Undefined);

    public static Error NoError() => With(ErrorCode.None);
}
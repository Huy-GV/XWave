namespace XWave.Core.Services.Communication;

public enum ErrorCode
{
    /// <summary>
    /// No error occurred. 
    /// </summary>
 
    None = 0,

    /// <summary>
    /// Undefined error.
    /// </summary>
    Undefined,

    /// <summary>
    /// Requested entity not found.
    /// </summary>
    EntityNotFound,

    /// <summary>
    /// Entity is in an invalid state.
    /// </summary>
    InvalidState,

    /// <summary>
    /// User request not valid.
    /// </summary>
    InvalidArgument,

    /// <summary>
    /// User request in conflict with existing data.
    /// </summary>
    ConflictingState,

    /// <summary>
    /// User unauthorized.
    /// </summary>
    AuthorizationError,

    /// <summary>
    /// User unauthenticated.
    /// </summary>
    AuthenticationError,
}
namespace XWave.Core.Services.Communication;

public enum ErrorCode
{
    /// <summary>
    /// Undefined error.
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// Requested entity not found.
    /// </summary>
    EntityNotFound,

    /// <summary>
    /// Entity already exists.
    /// </summary>
    EntityAlreadyExist,

    /// <summary>
    /// Entity is in an invalid state.
    /// </summary>
    EntityInvalidState,

    /// <summary>
    /// Stored entity and entity from request are inconsistent.
    /// </summary>
    EntityInconsistentStates,

    /// <summary>
    /// User request not valid.
    /// </summary>
    InvalidUserRequest,
}
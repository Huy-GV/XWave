namespace XWave.Services.ResultTemplate
{
    public class ServiceResult
    {
        public bool Succeeded { get; init; } = false;

        public string Error { get; init; } = string.Empty;

        /// <summary>
        /// Helper method that returns a failed result.
        /// </summary>
        /// <param name="error">Error message describing the cause of failure.</param>
        /// <returns></returns>
        public static ServiceResult Failure(string error = "An error occured") => new() { Error = error };

        /// <summary>
        /// Helper method that returns a successful result.
        /// </summary>
        /// <param name="id">ID of resource involved in the service operation.</param>
        /// <returns></returns>
        public static ServiceResult Success() => new() { Succeeded = true };
    }
}
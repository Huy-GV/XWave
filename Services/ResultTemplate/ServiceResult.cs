namespace XWave.Services.ResultTemplate
{
    public class ServiceResult
    {
        // todo: consider better properties
        public bool Succeeded { get; init; } = false;

        public string Error { get; init; } = string.Empty;
        public string ResourceId { get; init; } = string.Empty;

        /// <summary>
        /// Helper method that returns a failed result.
        /// </summary>
        /// <param name="error">Error message describing the cause of failure.</param>
        /// <returns></returns>
        public static ServiceResult Failure(string error)
        {
            return new ServiceResult { Error = error };
        }

        /// <summary>
        /// Helper method that returns a successful result.
        /// </summary>
        /// <param name="id">ID of resource involved in the service operation.</param>
        /// <returns></returns>
        public static ServiceResult Success(string id = "")
        {
            return new ServiceResult
            {
                ResourceId = id,
                Succeeded = true,
            };
        }
    }
}
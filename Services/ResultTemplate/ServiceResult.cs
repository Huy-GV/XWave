using System.Collections.Generic;

namespace XWave.Services.ResultTemplate
{
    public class ServiceResult
    {
        public bool Succeeded { get; init; } = false;

        // todo: replace with IEnumerable<string>
        public string Error { get; init; } = string.Empty;

        public ICollection<string> Errors { get; init; } = new List<string>();

        /// <summary>
        /// Helper method that returns a failed result.
        /// </summary>
        /// <param name="error">Error message describing the cause of failure.</param>
        /// <returns></returns>
        public static ServiceResult Failure(string error = "An error occured") => new() { Error = error };

        /// <summary>
        /// Helper method that returns a failed result.
        /// </summary>
        /// <param name="errors">Error message describing the cause of failure.</param>
        /// <returns></returns>
        public static ServiceResult Failure(params string[] errors) => new() { Errors = errors };

        /// <summary>
        /// Helper method that returns a successful result.
        /// </summary>
        /// <param name="id">ID of resource involved in the service operation.</param>
        /// <returns></returns>
        public static ServiceResult Success() => new() { Succeeded = true };
    }
}
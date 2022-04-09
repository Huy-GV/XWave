using System.Collections.Generic;

namespace XWave.Services.ResultTemplate
{
    // convert to record and move methods to helper objects
    public class ServiceResult
    {
        public bool Succeeded { get; init; } = false;

        public IEnumerable<string> Errors { get; init; } = new List<string>();

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
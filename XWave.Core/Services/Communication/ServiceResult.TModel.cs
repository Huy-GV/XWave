using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XWave.Core.Services.Communication
{
    public record ServiceResult<TResult> : ServiceResult where TResult : notnull
    {
        public TResult? Value { get; init; }

        public static ServiceResult<TResultValue> Success<TResultValue>(TResultValue value) where TResultValue : notnull
        {
            return new ServiceResult<TResultValue>()
            {
                Value = value,
                Succeeded = true,
            };
        }
        public static new ServiceResult<TResult> Failure(Error error)
        {
            return new ServiceResult<TResult>()
            {
                Value = default,
                Succeeded = false,
                Errors = new List<Error> { error },
            };
        }

        public static new ServiceResult<TResult> Failure(IEnumerable<Error> errors)
        {
            return new ServiceResult<TResult>()
            {
                Value = default,
                Succeeded = false,
                Errors = errors.ToList(),
            };
        }

        public static ServiceResult<TResult> DefaultFailure()
        {
            return new ServiceResult<TResult>()
            {
                Value = default,
                Succeeded = false,
                Errors = new List<Error> { Error.Default() },
            };
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XWave.Core.Services.Communication
{
    public record Error
    {
        public string Message { get; init; }
        public ErrorCode ErrorCode { get; init; }

        public static Error Default() => new Error()
        {
            Message = "An internal error occurred",
            ErrorCode = ErrorCode.Undefined,
        };
    }
}
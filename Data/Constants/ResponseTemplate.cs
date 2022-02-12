using System.Collections.Generic;
using System.Linq;

namespace XWave.Data.Constants
{
    public static class ResponseTemplate
    {
        public static object Created(string url) => new
        {
            Status = "Created",
            Message = $"Location: {url}"
        };
        public static object Updated(string url) => new
        {
            Status = "Updated",
            Message = $"Location: {url}"
        };
        //TODO: use this to return errors to users

        public static object InternalServerError(string error = "An error happended with your request") => new
        {
            Status = "Operation Failed",
            Message = $"Error: {error}"
        };
        public static object NonExistentResource() => new
        {
            Status = "Aborted",
            Message = "Operation on an non-existent resource failed"
        };
    }
}

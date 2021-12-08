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
        public static object Deleted(string title, string entityName) => new
        {
            Status = "Deleted",
            Message = $"Deleted: {entityName} with ID {title}"
        };
        public static object InternalServerError() => new
        {
            Status = "Failed",
            Message = $"An error happended with your request"
        };
    }
}

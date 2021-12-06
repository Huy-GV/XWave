using System.Collections.Generic;
using System.Linq;

namespace XWave.Data.Constants.ResponseTemplate
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
    }
}

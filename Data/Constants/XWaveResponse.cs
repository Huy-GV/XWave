namespace XWave.Data.Constants
{
    public static class XWaveResponse
    {
        public static object Created(string url) => new
        {
            Status = "Created.",
            Message = $"Location: {url}."
        };

        public static object Updated(string url) => new
        {
            Status = "Updated.",
            Message = $"Location: {url}."
        };

        public static object Failed(string error = "An error happended with your request.") => new
        {
            Status = "Operation Failed.",
            Message = $"Error: {error}."
        };

        public static object NonExistentResource() => new
        {
            Status = "Operation Aborted.",
            Message = "Operation called on a non-existing resource."
        };
    }
}
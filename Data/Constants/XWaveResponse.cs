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

        public static object Failed(params string[] errors) => new
        {
            Status = "Failed.",
            Message = "One or more errors occured.",
            Errors = errors,
        };

        public static object NonExistentResource() => new
        {
            Status = "Aborted.",
            Message = "Operation called on a non-existing resource."
        };
    }
}
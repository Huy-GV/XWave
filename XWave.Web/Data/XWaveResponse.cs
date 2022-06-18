namespace XWave.Web.Data;

public static class XWaveResponse
{
    public static object Failed(params string[] errors)
    {
        return new
        {
            Status = "Failed.",
            Message = "One or more errors occured.",
            Errors = errors
        };
    }

    public static object NonExistentResource()
    {
        return new
        {
            Status = "Aborted.",
            Message = "Operation called on a non-existing resource."
        };
    }
}
namespace XWave.Services.ResultTemplate
{
    public class ServiceResult
    {
        public bool Succeeded { get; init; } = false;
        public string Error { get; init; }  = string.Empty;
        public string ResourceID { get; init; } = string.Empty;
    }
}

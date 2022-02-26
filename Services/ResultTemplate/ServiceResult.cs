namespace XWave.Services.ResultTemplate
{
    public class ServiceResult
    {
        public bool Succeeded { get; init; } = false;
        public string Error { get; init; }  = string.Empty;
        public string ResourceId { get; init; } = string.Empty;
        public static ServiceResult Failure(string error) 
        {
            return new ServiceResult { Error = error };
        }
        public static ServiceResult Success(string id = "")
        {
            return new ServiceResult 
            { 
                ResourceId = id,
                Succeeded = true,
            };
        }
    }
}

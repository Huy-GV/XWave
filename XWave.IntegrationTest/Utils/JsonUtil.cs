using System.Text.Json;

namespace XWave.IntegrationTest.Utils;
internal class JsonUtil
{
    public static readonly JsonSerializerOptions CaseInsensitiveOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}

namespace XWave.Core.Configuration;

public class JwtCookie
{
    public string Name { get; set; } = string.Empty;
    public bool HttpOnly { get; set; }
    public int DurationInDays { get; set; }
}
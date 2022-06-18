namespace XWave.Web.Configuration;

public class JwtCookie
{
    public string Name { get; set; }
    public bool HttpOnly { get; set; }
    public int DurationInDays { get; set; }
}
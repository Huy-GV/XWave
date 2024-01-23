using System.ComponentModel.DataAnnotations;

namespace XWave.Web.Options;

public class JwtCookieOptions
{
    [Required]
    public required string Name { get; init; }

    [Required] 
    public required bool HttpOnly { get; init; } = true;
    
    [Required]
    public required int DurationInDays { get; init; }
}